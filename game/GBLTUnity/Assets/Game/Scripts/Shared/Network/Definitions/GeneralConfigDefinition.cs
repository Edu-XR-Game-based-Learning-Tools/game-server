using MasterMemory;
using MemoryPack;
using System;

namespace Shared.Network
{
    [Serializable]
    [MemoryPackable]
    public partial struct GenericNotificationMessage
    {
        public string Title;
        public string Message;
    }

    [Serializable]
    [MemoryPackable]
    public partial class GoogleConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scopes { get; set; }
        public string LoginUri { get; set; }
        public string TokenUri { get; set; }
        public string UserInfoUri { get; set; }
    }

    [Serializable]
    [MemoryPackable]
    public partial class FirebaseConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

    [Serializable]
    [MemoryTable("GeneralConfig"), MemoryPackable]
    public partial class GeneralConfigDefinition : BaseItemDefinition
    {
        [PrimaryKey]
        public override string Id { get; set; }

        public string[] AddressableBundleLabels;
        public string CheckPreloadAssetErrorMessage;
        public string DownloadAssetErrorMessage;

        public FirebaseConfig FirebaseConfig;
        public GoogleConfig GoogleConfig;
    }
}