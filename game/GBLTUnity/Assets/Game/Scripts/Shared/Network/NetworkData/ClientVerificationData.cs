using MemoryPack;

namespace Shared.Network
{
    [MemoryPackable]
    public partial class ClientVerificationData : GeneralResponse
    {
        public bool IsUnderMaintenance { get; set; }
        public string MaintenanceMessage { get; set; }
        public bool IsValidVersion { get; set; }
        public bool IsForceDownload { get; set; }
        public string VersionMessage { get; set; }
        public string DownloadUrl { get; set; }
        public int FileSize { get; set; }
    }
}