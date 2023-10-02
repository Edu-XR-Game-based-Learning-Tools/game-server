using MessagePack;
using Shared.Extension;
using System.Linq;

namespace Shared.Network
{
    [MessagePackObject(true)]
    public class PublicUserData
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string AvatarPath { get; set; } = Defines.PrefabKey.DefaultRoomAvatar;
        public Vec3D HeadRotation { get; set; }
        public bool IsHost => Index == -1;

        // In Game Attributes
        public int Score { get; set; }
        public int Rank { get; set; }
    }

    [MessagePackObject(true)]
    public class PrivateUserData : PublicUserData
    {
    }

    [MessagePackObject(true)]
    public class GeneralRoomStatusResponse : GeneralResponse
    {
        public string Id { get; set; }
        public PrivateUserData Self { get; set; }
        public PublicUserData[] AllInRoom { get; set; }
        public PublicUserData[] Others => AllInRoom.WhereNot((ele) => ele.Index == Self.Index).ToArray();
        public int Amount => AllInRoom.Length + 1;
    }

    [MessagePackObject(true)]
    public class RoomStatusResponse : GeneralRoomStatusResponse
    {
        public int MaxAmount { get; set; }
        public string Password { get; set; }
    }

    [MessagePackObject(true)]
    public class InGameStatusResponse : GeneralRoomStatusResponse
    {
    }

    [MessagePackObject(true)]
    public class VirtualRoomTickResponse : GeneralResponse
    {
        public PublicUserData User { get; set; }
        public byte[] Texture { get; set; }
    }
}
