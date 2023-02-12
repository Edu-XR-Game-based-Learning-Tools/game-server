using System;

namespace Core.Business
{
    public struct ClientVerificationData
    {
        public bool IsUnderMaintenance { get; set; }
        public string MaintenanceMessage { get; set; }
        public bool IsValidVersion { get; set; }
        public bool IsForceDownload { get; set; }
        public string VersionMessage { get; set; }
        public string DownloadUrl { get; set; }
        public int FileSize { get; set; }
    }

    public interface IGenericServices
    {
        ClientVerificationData VerifyClient(string clientVersion);

        DateTime GetServerTime();
    }
}