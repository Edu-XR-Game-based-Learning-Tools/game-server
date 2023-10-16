using MessagePack;

namespace Shared.Network
{
    [MessagePackObject(true)]
    public class JoinClassRoomData
    {
        public string RoomId { get; set; }
        public string UserName { get; set; }
        public string AvatarPath { get; set; } = Defines.PrefabKey.DefaultRoomAvatar;
        public string ModelPath { get; set; } = Defines.PrefabKey.DefaultRoomModel;
        public int Amount { get; set; }
        public string Password { get; set; }
    }

    [MessagePackObject(true)]
    public class VirtualRoomTickData
    {
        public Vec3D HeadRotation { get; set; }
        public byte[] Texture { get; set; }
        public bool IsSharing { get; set; }
    }
}
