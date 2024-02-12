using Cysharp.Threading.Tasks;
using MessagePack;

namespace Shared.Network
{

    [System.Serializable]
    [MessagePackObject(true)]
    public struct UserData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    [System.Serializable]
    public class RoomStatusVM
    {
        public RoomStatusResponse RoomStatus { get; set; }
        public QuizzesStatusResponse InGameStatus { get; set; }
    }

    [System.Serializable]
    public class UserServerEntity
    {
        public UserData UserData { get; set; }
        public RoomStatusVM RoomStatus { get; set; }
        public bool IsSharing { get; set; } = false;
        public bool IsSharingQuizzesGame { get; set; } = false;
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
