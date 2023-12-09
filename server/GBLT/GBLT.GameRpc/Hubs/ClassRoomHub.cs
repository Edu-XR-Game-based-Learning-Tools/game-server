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
using System.Net.NetworkInformation;
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
        private IInMemoryStorage<PublicUserData> _storage; // -1 index is host

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

        private async Task<RoomStatusResponse> GetRoomDataFromCache(string roomId = null)
        {
            if (roomId == null && _room == null) return null;
            var status = await _redisDataService.GetCacheAsync<RoomStatusResponse>(GetRoomCacheKey(roomId ?? _room.GroupName));
            return status;
        }

        private async Task SaveRoomDataToCache(RoomStatusResponse status)
        {
            await _redisDataService.UpdateCacheAsync(GetRoomCacheKey(status.JoinClassRoomData.RoomId), status);
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

        private static GeneralResponse ValidateJoinRoom(JoinClassRoomData data, RoomStatusResponse status)
        {
            if (status == null || status.JoinClassRoomData == null) return new RoomStatusResponse { Success = false, Message = Defines.ROOM_UNAVAILABLE };

            if (status.AllInRoom.Length - 1 > status.JoinClassRoomData.Amount)
                return new RoomStatusResponse { Success = false, Message = Defines.FULL_AMOUNT };

            if (!status.JoinClassRoomData.Password.IsNullOrEmpty() && data.Password != status.JoinClassRoomData.Password)
                return new RoomStatusResponse { Success = false, JoinClassRoomData = status.JoinClassRoomData, Message = Defines.INVALID_PASSWORD };

            return null;
        }

        private async Task<RoomStatusResponse> TryCreateRoom(JoinClassRoomData data)
        {
            if (!data.RoomId.IsNullOrEmpty()) return null;

            try
            {
                RoomStatusResponse validateMsg = (RoomStatusResponse)await ValidateCreateRoom(data);
                if (validateMsg != null) return validateMsg;

                data.RoomId = GenerateRoomId();
                _roomSet.Add(data.RoomId);

                _self.Index = -1;
                (_room, _storage) = await Group.AddAsync(data.RoomId, (PublicUserData)_self);

                RoomStatusResponse status = new() { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName, JoinClassRoomData = data };
                BroadcastExceptSelf(_room).OnJoin(status, _self);
                await SaveRoomDataToCache(status);
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<RoomStatusResponse> JoinAsync(JoinClassRoomData data)
        {
            _self = new() { ConnectionId = ConnectionId, Name = data.UserName.IsNullOrEmpty() ? "Name" : data.UserName };

            var response = await TryCreateRoom(data);
            if (response != null) return response;

            try
            {
                RoomStatusResponse status = await GetRoomDataFromCache(data.RoomId);
                RoomStatusResponse validateMsg = (RoomStatusResponse)ValidateJoinRoom(data, status);
                if (validateMsg != null) return validateMsg;

                var allIndexesInRoom = status.AllInRoom.Select(user => user.Index).OrderBy(index => index);
                int index = -1;
                for (int idx = 0; idx < allIndexesInRoom.Count(); idx++)
                {
                    if (index != allIndexesInRoom.ElementAt(idx)) break;
                    index++;
                }
                _self.Index = index; // -1 for not count teacher, teacher seat: index = -1
                (_room, _storage) = await Group.AddAsync(data.RoomId, (PublicUserData)_self);

                status.Self = _self;
                status.AllInRoom = _storage.AllValues.ToArray();
                BroadcastExceptSelf(_room).OnJoin(status, _self);
                await SaveRoomDataToCache(status);
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        #endregion Join Room

        private async Task<bool> RemoveContextIfNotExistRoomData(RoomStatusResponse status)
        {
            if (_room == null) return true;
            status ??= await GetRoomDataFromCache();
            if (status == null)
            {
                BroadcastToSelf(_room).OnLeave(status, _self);
                await _room.RemoveAsync(Context);
                return true;
            }
            return false;
        }

        public async Task LeaveAsync()
        {
            try
            {
                RoomStatusResponse status = null;
                if (_self.IsHost)
                {
                    _roomSet.Remove(_room.GroupName);
                    await _redisDataService.RemoveCacheAsync(GetRoomCacheKey(_room.GroupName));
                }
                else
                {
                    status = await GetRoomDataFromCache();
                    if (await RemoveContextIfNotExistRoomData(status)) return;
                    status.AllInRoom = _storage.AllValues.WhereNot(ele => ele.ConnectionId == ConnectionId).ToArray();
                    await SaveRoomDataToCache(status);
                }
                Broadcast(_room).OnLeave(status, _self);

                await _room.RemoveAsync(Context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task InviteToGame(InviteToGameData data)
        {
            var quizzesStatus = await _redisDataService.GetCacheAsync<QuizzesStatusResponse>(QuizzesHub.GetRoomCacheKey(data.RoomId));
            data.JoinQuizzesData = quizzesStatus.JoinQuizzesData;
            BroadcastExceptSelf(_room).OnInviteToGame(data, _self);
        }

        public Task UpdateAvatar(string name, string modelPath, string avatarPath)
        {
            _self.Name = name;
            _self.AvatarPath = avatarPath;
            _self.ModelPath = modelPath;
            Broadcast(_room).OnUpdateAvatar(_self);
            return Task.CompletedTask;
        }

        public Task VirtualRoomTickSync(VirtualRoomTickData data)
        {
            _self.HeadRotation = data.HeadRotation;
            _self.VoiceSamples = data.VoiceData;
            _self.SamplePosition = data.SamplePosition;
            Broadcast(_room).OnRoomTick(new VirtualRoomTickResponse
            {
                User = _self,
            });
            return Task.CompletedTask;
        }

        public Task SharingTickSync(SharingTickData data)
        {
            Broadcast(_room).OnSharingTick(new SharingTickData
            {
                User = _self,
                Texture = data.Texture,
                IsSharing = data.IsSharing,
                IsSharingQuizzesGame = data.IsSharingQuizzesGame,
            });
            return Task.CompletedTask;
        }

        public async Task Tick(string message = "")
        {
            (_room, _) = await Group.AddAsync("data.RoomId", new PublicUserData());
            Broadcast(_room).OnTick(message);
        }
    }
}