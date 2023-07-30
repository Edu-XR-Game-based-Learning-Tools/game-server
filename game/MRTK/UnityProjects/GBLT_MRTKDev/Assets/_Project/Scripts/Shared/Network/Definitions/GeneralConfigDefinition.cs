using MasterMemory;
using MessagePack;
using System;

namespace Shared.Network
{
    [Serializable]
    [MessagePackObject(true)]
    public partial struct GenericNotificationMessage
    {
        public string Title;
        public string Message;
    }

    [Serializable]
    [MessagePackObject(true)]
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
    [MessagePackObject(true)]
    public partial class FirebaseConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

    [Serializable]
    [MemoryTable("GeneralConfig"), MessagePackObject(true)]
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
