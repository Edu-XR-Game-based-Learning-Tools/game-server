using MagicOnion;
using System.Threading.Tasks;

namespace Shared.Network
{
    public interface IClassRoomHub : IStreamingHub<IClassRoomHub, IClassRoomHubReceiver>
    {
        // The method must return `ValueTask`, `ValueTask<T>`, `Task` or `Task<T>` and can have up to 15 parameters of any type.
        Task Sync();

        Task<RoomStatusResponse> JoinAsync(JoinClassRoomData data);

        Task LeaveAsync();

        Task InviteToGame(InviteToGameData data);

        Task UpdateAvatar(string name, string modelPath, string avatarPath);

        Task VirtualRoomTickSync(VirtualRoomTickData data);

        Task CmdToKeepAliveConnection();
    }

    public interface IClassRoomHubReceiver
    {
        // The method must have a return type of `void` and can have up to 15 parameters of any type.
        void OnJoin(RoomStatusResponse status, PublicUserData user);

        void OnLeave(RoomStatusResponse status, PublicUserData user);

        void OnInviteToGame(InviteToGameData data, PublicUserData inviter);

        void OnUpdateAvatar(PublicUserData user);

        void OnRoomTick(VirtualRoomTickResponse response);

        void OnTick(string message);
    }
}
