using Core.Service;
using MagicOnion;
using MagicOnion.Server;
using Shared.Network;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace RpcService.Service
{
    public class GenericService : ServiceBase<IGenericService>, IGenericService
    {
        private readonly ILogger _logger;
        private readonly IMetaService _metaService;
        private readonly IDefinitionDataService _definitionService;

        public GenericService(
            IMetaService metaService,
            IDefinitionDataService definitionService,
            ILogger<GenericService> logger)
        {
            _logger = logger;
            _metaService = metaService;
            _definitionService = definitionService;
        }

        public async UnaryResult<ClientVerificationData> VerifyClient(string clientVersion)
        {
            ClientVerificationData result = new()
            {
                IsValidVersion = false,
                IsUnderMaintenance = false,
            };
            try
            {
                VersionMetaConfig versionConfig = await _metaService.GetVersionMetaConfig();
                MaintenanceMetaConfig maintenanceConfig = await _metaService.GetMaintenanceMetaConfig();
                string[] versions = versionConfig.Versions.Split(',');
                result.IsValidVersion = versions.Contains(clientVersion);
                if (!result.IsValidVersion)
                {
                    result.IsForceDownload = versionConfig.IsForceDownload;
                    result.VersionMessage = versionConfig.VersionMessage;
                    result.DownloadUrl = _definitionService.GetEnvironmentConfig().LauncherUrl;
                    result.FileSize = versionConfig.FileSize;
                }

                string[] peer = GetPeer().Split(':');
                _logger.LogDebug($"VerifyClient peer {GetPeer()} - {peer[1]} - whitelist {maintenanceConfig.WhiteList}");
                string[] whitelist = Array.Empty<string>();
                if (!string.IsNullOrEmpty(maintenanceConfig.WhiteList))
                    whitelist = maintenanceConfig.WhiteList.Split(',');

                if (!whitelist.Contains(peer[1]))
                {
                    result.IsUnderMaintenance = maintenanceConfig.IsUnderMaintenance;
                    result.MaintenanceMessage = maintenanceConfig.IsUnderMaintenance ? maintenanceConfig.MaintenanceMessage : null;
                }
                _logger.LogDebug($"VerifyClient is IsUnderMaintenance {result.IsUnderMaintenance} - {maintenanceConfig.IsUnderMaintenance}");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "empty";
                string errorMessage = "GenericService VerifyClient failed: " +
                    $"\n\t- Error: {ex.Message} " +
                    $"\n\t- InnerException: " + innerMessage +
                    $"\n\t- Stack: {ex.StackTrace} ";
                _logger.LogError(errorMessage);

                result.IsUnderMaintenance = true;
                result.MaintenanceMessage = "GenericService VerifyClient failed. Server is UnderMaintenance";
            }
            return result;
        }

        public UnaryResult<DateTime> GetServerTime()
        {
            DateTime currentTime = DateTime.UtcNow;
            return UnaryResult.FromResult(currentTime);
        }

        public UnaryResult<EnvironmentGenericConfig> GetGenericConfig()
        {
            return UnaryResult.FromResult(_definitionService.GetEnvironmentConfig());
        }

        // ipv4:...:5064
        private string GetPeer()
        {
            return Context.CallContext.Peer;
        }
    }
}