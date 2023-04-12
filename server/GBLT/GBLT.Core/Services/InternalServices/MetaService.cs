using Core.Entity;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Service
{
    public struct VersionMetaConfig
    {
        public string Versions { get; set; }
        public bool IsForceDownload { get; set; }
        public string VersionMessage { get; set; }
        public string DownloadUrl { get; set; }
        public int FileSize { get; set; }
    }

    public struct MaintenanceMetaConfig
    {
        public bool IsUnderMaintenance { get; set; }
        public string MaintenanceMessage { get; set; }
        public string WhiteList { get; set; }
    }

    public class MetaService : IMetaService
    {
        private const string CLIENT_VERSION_META_KEY = "__client_version_meta";
        private const string SERVER_MAINTENANCE_META_KEY = "__server_maintenance_meta";

        private readonly IMetaDataService _metaDataService;

        public MetaService(
            IMetaDataService metaDataService)
        {
            _metaDataService = metaDataService;
        }

        public async Task<VersionMetaConfig> GetVersionMetaConfig()
        {
            TMeta configMeta = await _metaDataService.GetMetaAsync(CLIENT_VERSION_META_KEY);
            if (configMeta == null)
            {
                VersionMetaConfig config = GetDefaultVersionMeta();
                configMeta = await _metaDataService.CreateMeta(CLIENT_VERSION_META_KEY, JsonSerializer.Serialize(config));
                await _metaDataService.UpdateMetaAsync(configMeta);
            }
            return JsonSerializer.Deserialize<VersionMetaConfig>(configMeta.MetaValue);
        }

        public async Task<MaintenanceMetaConfig> GetMaintenanceMetaConfig()
        {
            TMeta configMeta = await _metaDataService.GetMetaAsync(SERVER_MAINTENANCE_META_KEY);
            if (configMeta == null)
            {
                MaintenanceMetaConfig config = GetDefaultMaintenanceMeta();
                configMeta = await _metaDataService.CreateMeta(SERVER_MAINTENANCE_META_KEY, JsonSerializer.Serialize(config));
                await _metaDataService.UpdateMetaAsync(configMeta);
            }
            return JsonSerializer.Deserialize<MaintenanceMetaConfig>(configMeta.MetaValue);
        }

        private static VersionMetaConfig GetDefaultVersionMeta()
        {
            VersionMetaConfig config = new()
            {
                Versions = "0.1",
                IsForceDownload = true,
                VersionMessage = "Your version is out of date!\nDownload New Version.",
                DownloadUrl = "https://localhost",
                FileSize = 100
            };
            return config;
        }

        private static MaintenanceMetaConfig GetDefaultMaintenanceMeta()
        {
            MaintenanceMetaConfig config = new()
            {
                IsUnderMaintenance = false,
                MaintenanceMessage = "The server is currently under maintenance.\nPlease try again later."
            };
            return config;
        }
    }
}