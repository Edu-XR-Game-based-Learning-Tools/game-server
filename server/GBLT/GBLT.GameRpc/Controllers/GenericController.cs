using Core.Service;
using Core.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RpcService.Service;
using Shared.Network;
using System.Net;

namespace RpcService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMetaService _metaService;
        private readonly IDefinitionDataService _definitionService;

        public GenericController(
            IMetaService metaService,
            IDefinitionDataService definitionService,
            ILogger<GenericService> logger)
        {
            _logger = logger;
            _metaService = metaService;
            _definitionService = definitionService;
        }

        // POST api/generic/verifyClient
        [HttpPost("verifyClient")]
        public async Task<ActionResult> VerifyClient([FromBody] string clientVersion)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
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
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(result)
            };
            return contentResult;
        }

        // GET api/generic/getServerTime
        [HttpGet("getServerTime")]
        public async Task<ActionResult> GetServerTime()
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            DateTime currentTime = DateTime.UtcNow;
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(currentTime)
            };
            return contentResult;
        }

        // GET api/generic/getDefinitions
        [HttpGet("getDefinitions")]
        public async Task<ActionResult> GetDefinitions()
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(_definitionService.DefinitionsData)
            };
            return contentResult;
        }

        // GET api/generic/getGenericConfig
        [HttpGet("getGenericConfig")]
        public async Task<ActionResult> GetGenericConfig()
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            JsonContentResult contentResult = new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(_definitionService.GetEnvironmentConfig())
            };
            return contentResult;
        }

        // ipv4:...:5064
        private string GetPeer()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}