using MagicOnion;

namespace Shared.Network
{
    public interface IRpcUserService : IService<IRpcUserService>
    {
        UnaryResult<UserData> SyncUserData();
    }
}
