using System.Threading.Tasks;

namespace Core.Service
{
    public interface IMetaService
    {
        Task<VersionMetaConfig> GetVersionMetaConfig();
        Task<MaintenanceMetaConfig> GetMaintenanceMetaConfig();
    }
}