using MasterMemory;
using MessagePack;
using System;

namespace Shared.Network
{
    [Serializable]
    [MemoryTable("ClassRoomDefinition"), MessagePackObject(true)]
    public partial class ClassRoomDefinition : BaseItemDefinition
    {
        [PrimaryKey]
        public override string Id { get; set; }

        public Vec3D TeacherSeatPosition { get; set; }
        public Vec3D TeacherSeatRotation { get; set; }
        public Vec3D StartCenterSeatPosition { get; set; }
        public int MaxColPerRow { get; set; }
        public int MinGenerateSeats { get; set; }
        public float RowSpace { get; set; }
        public float ColSpace { get; set; }
    }
}
