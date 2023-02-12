using MemoryPack;

namespace Shared.Network
{
    [MemoryPackable]
    public partial class GeneralResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}