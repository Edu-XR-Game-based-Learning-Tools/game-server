using Core.Entity;
using Core.Service;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Extension;
using Shared.Network;
using System.Security.Claims;
using System.Text;

namespace RpcService.Hub
{
    public abstract class GenericIdentityHub<TSelf, TReceiver> : StreamingHubBase<TSelf, TReceiver> where TSelf : IStreamingHub<TSelf, TReceiver>
    {
        protected readonly IUserDataService _userDataService;
        protected readonly IJwtTokenValidator _jwtTokenValidator;
        protected readonly IRedisDataService _redisDataService;

        protected static HashSet<string> _roomSet = new();

        public GenericIdentityHub(IUserDataService userDataService, IJwtTokenValidator jwtTokenValidator, IRedisDataService redisDataService)
        {
            _userDataService = userDataService;
            _jwtTokenValidator = jwtTokenValidator;
            _redisDataService = redisDataService;
        }

        public Task CmdToKeepAliveConnection()
        {
            return Task.CompletedTask;
        }

        protected string GenerateRoomId(HashSet<string> set = null)
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

        protected async Task<TUser> GetUserIdentity()
        {
            var header = Context.CallContext.RequestHeaders;
            var bytes = header.GetValueBytes("auth-token-bin");
            if (bytes == null) return null;
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

    public class ClassRoomHub : GenericIdentityHub<IClassRoomHub, IClassRoomHubReceiver>, IClassRoomHub
    {
        private Task _timerLoopTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);

        private IGroup _room;

        private PrivateUserData _self;
        private IInMemoryStorage<PublicUserData> _storage;

        public ClassRoomHub(IUserDataService userDataService, IJwtTokenValidator jwtTokenValidator, IRedisDataService redisDataService) : base(userDataService, jwtTokenValidator, redisDataService)
        {
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
            _ = LeaveAsync();
            return base.OnDisconnected();
        }

        private static string GetRoomCacheKey(string roomId)
        {
            return $"ClassRoom/{roomId}";
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
            if (_storage != null && _storage.AllValues.Count > data.Amount)
                return new RoomStatusResponse { Success = false, Password = Defines.FULL_AMOUNT };

            if (data.Password != data.Password)
                return new RoomStatusResponse { Success = false, Password = Defines.INVALID_PASSWORD };

            return null;
        }

        public async Task<RoomStatusResponse> JoinAsync(JoinClassRoomData data)
        {
            _self = new() { ConnectionId = ConnectionId, Name = data.UserName };
            RoomStatusResponse validateMsg;

            try
            {
                bool isHost = false;
                if (data.RoomId.IsNullOrEmpty())
                {
                    validateMsg = (RoomStatusResponse)await ValidateCreateRoom(data);
                    if (validateMsg != null) return validateMsg;

                    data.RoomId = GenerateRoomId();
                    _roomSet.Add(data.RoomId);
                    await _redisDataService.UpdateCacheAsync(GetRoomCacheKey(data.RoomId), data);
                    isHost = true;
                }

                var joinData = await _redisDataService.GetCacheAsync<JoinClassRoomData>(GetRoomCacheKey(data.RoomId));
                validateMsg = (RoomStatusResponse)ValidateJoinRoom(joinData);
                if (validateMsg != null) return validateMsg;

                (_room, _storage) = await Group.AddAsync(data.RoomId, (PublicUserData)_self);
                _self.Index = isHost ? -1 : _storage.AllValues.Count - 1; // -1 for not count teacher, teacher seat: index = -1

                RoomStatusResponse status = new() { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName, Password = joinData.Password, MaxAmount = joinData.Amount };
                BroadcastExceptSelf(_room).OnJoin(status, _self);
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        #endregion Join Room

        public async Task LeaveAsync()
        {
            var joinData = await _redisDataService.GetCacheAsync<JoinClassRoomData>(GetRoomCacheKey(_room.GroupName));
            if (_self.IsHost)
            {
                _roomSet.Remove(_room.GroupName);
                await _redisDataService.RemoveCacheAsync(GetRoomCacheKey(_room.GroupName));
            }
            await _room.RemoveAsync(Context);

            RoomStatusResponse status = null;
            if (!_self.IsHost) status = new() { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName, Password = joinData.Password, MaxAmount = joinData.Amount };
            Broadcast(_room).OnLeave(status, _self);
        }

        public async Task InviteToGame(InviteToGameData data)
        {
            var quizzesData = await _redisDataService.GetCacheAsync<JoinQuizzesData>(QuizzesHub.GetRoomCacheKey(data.RoomId));
            data.JoinQuizzesData = quizzesData;
            BroadcastExceptSelf(_room).OnInviteToGame(data, _self);
        }

        public Task UpdateAvatar(string name, string modelPath, string avatarPath)
        {
            _self.Name = name;
            _self.AvatarPath = avatarPath;
            _self.ModelPath = modelPath;
            BroadcastExceptSelf(_room).OnUpdateAvatar(_self);
            return Task.CompletedTask;
        }

        public Task VirtualRoomTickSync(VirtualRoomTickData data)
        {
            _self.HeadRotation = data.HeadRotation;
            Broadcast(_room).OnRoomTick(new VirtualRoomTickResponse
            {
                User = _self,
                Texture = data.Texture,
                IsSharing = data.IsSharing,
            });
            return Task.CompletedTask;
        }
    }
}