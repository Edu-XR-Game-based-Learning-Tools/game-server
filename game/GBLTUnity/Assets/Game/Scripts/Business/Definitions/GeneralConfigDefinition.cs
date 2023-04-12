using System;

namespace Core.Business
{
    [Serializable]
    public class GeneralConfigDefinition : BaseItemDefinition
    {
        public override string Id { get; set; }

        // Addressable
        public string[] AddressableBundleLabels = new string[0];
        public string CheckPreloadAssetErrorMessage = "";
        public string DownloadAssetErrorMessage = "";
    }
}