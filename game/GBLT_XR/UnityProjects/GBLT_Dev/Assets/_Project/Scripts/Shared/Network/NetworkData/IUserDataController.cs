using Cysharp.Threading.Tasks;
using MessagePack;

namespace Shared.Network
{

    [MessagePackObject(true)]
    public struct UserData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RoomStatusVM
    {
        public RoomStatusResponse RoomStatus { get; set; }
        public InGameStatusResponse InGameStatus { get; set; }
    }

    public class UserServerEntity
    {
        public UserData UserData { get; set; }
        public RoomStatusVM RoomStatus { get; set; }
        public bool IsSharing { get; set; } = false;
        public bool IsInRoom => RoomStatus != null && RoomStatus.RoomStatus != null;
        public bool IsInGame => RoomStatus != null && RoomStatus.InGameStatus != null;
    }

    public interface IUserDataController
    {
        UserServerEntity ServerData { get; }
        ClassRoomDefinition[] ClassRoomDefinitions { get; }

        UniTaskVoid CacheDefinitions();

        ClassRoomDefinition GetClassRoomDefinition(string id);
    }
}
