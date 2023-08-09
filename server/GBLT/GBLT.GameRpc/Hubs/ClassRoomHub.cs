using Core.Entity;
using Core.Service;
using Grpc.Core;
using MagicOnion.Server.Hubs;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Extension;
using Shared.Network;
using System.Security.Claims;
using System.Text;

namespace RpcService.Hub
{
    public class ClassRoomHub : StreamingHubBase<IClassRoomHub, IClassRoomHubReceiver>, IClassRoomHub
    {
        private readonly IUserDataService _userDataService;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        private Task _timerLoopTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);

        private IGroup _room;
        private JoinClassRoomData _createData;

        private PrivateUserData _self;
        private IInMemoryStorage<PublicUserData> _storage;

        private static HashSet<string> _roomSet = new();
        private static HashSet<string> _inGameSet = new();

        public ClassRoomHub(IUserDataService userDataService, IJwtTokenValidator jwtTokenValidator)
        {
            _userDataService = userDataService;
            _jwtTokenValidator = jwtTokenValidator;
        }

        public async Task Sync()
        {
            if (_timerLoopTask != null) throw new InvalidOperationException("The timer has been already started.");

            _timerLoopTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(_interval, _cancellationTokenSource.Token);

                    var userPrincipal = Context.CallContext.GetHttpContext().User;
                    BroadcastToSelf(_room).OnTick($"UserId={userPrincipal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value}; Name={userPrincipal.Identity?.Name}");
                }
            });
        }

        protected override ValueTask OnDisconnected()
        {
            _cancellationTokenSource.Cancel();
            _timerLoopTask?.Dispose();
            return base.OnDisconnected();
        }

        private static string GenerateRoomId(HashSet<string> set = null)
        {
            set ??= _roomSet;
            string room;
            do
            {
                Random generator = new();
                room = generator.Next(0, 1000000).ToString("D6");
            } while (string.IsNullOrEmpty(room) && !set.Contains(room));
            return room;
        }

        #region Join Room

        private async Task<GeneralResponse> ValidateCreateRoom(JoinClassRoomData data)
        {
            var user = await GetUserIdentity();
            if (user == null)
                return new RoomStatusResponse { Success = false, Message = Defines.INVALID_SESSION };

            if (data.Amount < 24 || data.Amount > 48)
                return new RoomStatusResponse { Success = false, Message = Defines.INVALID_AMOUNT };

            return null;
        }

        private GeneralResponse ValidateJoinRoom(JoinClassRoomData data)
        {
            if (_storage != null && _storage.AllValues.Count > _createData.Amount)
                return new RoomStatusResponse { Success = false, Password = Defines.FULL_AMOUNT };

            if (_createData.Password != data.Password)
                return new RoomStatusResponse { Success = false, Password = Defines.INVALID_PASSWORD };

            return null;
        }

        public async Task<RoomStatusResponse> JoinAsync(JoinClassRoomData data)
        {
            _self = new() { Name = data.UserName };
            RoomStatusResponse validateMsg;
            if (data.RoomId.IsNullOrEmpty())
            {
                validateMsg = (RoomStatusResponse)await ValidateCreateRoom(data);
                if (validateMsg != null) return validateMsg;

                string roomId = GenerateRoomId();
                _roomSet.Add(roomId);
                _createData = data;
            }

            validateMsg = (RoomStatusResponse)ValidateJoinRoom(data);
            if (validateMsg != null) return validateMsg;

            _self.Index = _storage.AllValues.Count - 1; // -1 for not count teacher, teacher seat: index = -1
            (_room, _storage) = await Group.AddAsync(data.RoomId, (PublicUserData)_self);

            BroadcastExceptSelf(_room).OnJoin(_self);
            return new RoomStatusResponse { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName, Password = _createData.Password, MaxAmount = _createData.Amount };
        }

        #endregion Join Room

        public async Task LeaveAsync()
        {
            if (_self.IsHost) _roomSet.Remove(_room.GroupName);
            await _room.RemoveAsync(Context);
            Broadcast(_room).OnLeave(_self);
        }

        public Task VirtualRoomTickSync(VirtualRoomTickData data)
        {
            _self.HeadRotation = data.HeadRotation;
            Broadcast(_room).OnRoomTick(new VirtualRoomTickResponse
            {
                User = _self,
                Texture = data.Texture,
            });
            return Task.CompletedTask;
        }

        public Task CmdToKeepAliveConnection()
        {
            return Task.CompletedTask;
        }

        public async Task<TUser> GetUserIdentity()
        {
            var header = Context.CallContext.RequestHeaders;
            var bytes = header.GetValueBytes("auth-token-bin");
            var token = Encoding.ASCII.GetString(bytes);
            var cp = _jwtTokenValidator.GetPrincipalFromToken(token);

            if (cp != null)
            {
                var id = cp.Claims.First(c => c.Type == JwtClaimIdentifiers.Id);
                return await _userDataService.Find(id.Value);
            }
            return null;
        }
    }
}