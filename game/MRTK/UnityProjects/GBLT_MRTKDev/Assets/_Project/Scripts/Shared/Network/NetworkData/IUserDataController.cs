using Core.Utility;
using Cysharp.Threading.Tasks;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Network
{

    [MessagePackObject(true)]
    public struct UserData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class LocalUserCache
    {
        public Dictionary<string, Sprite> AvatarMap = new();

        public async UniTask<Sprite> GetUserAvatar(string path)
        {
            if (!AvatarMap.ContainsKey(path))
                AvatarMap[path] = await IMG2Sprite.FetchImageSprite(path);
            return AvatarMap[path];
        }
    }

    public class RoomStatusVM
    {
        public RoomStatusResponse RoomStatus { get; set; }
        public InGameStatusResponse InGameStatus { get; set; }
        public LocalUserCache LocalUserCache { get; set; }
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
