using Shared.Network;

namespace Core.Service
{
    public interface IDefinitionDataService
    {
        MemoryDatabase InMemoryData { get; set; }

        GeneralConfigDefinition GetGeneralConfig();

        EnvironmentGenericConfig GetEnvironmentConfig();
    }
}