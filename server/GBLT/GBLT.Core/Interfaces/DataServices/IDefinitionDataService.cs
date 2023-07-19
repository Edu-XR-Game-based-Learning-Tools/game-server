using Shared.Network;

namespace Core.Service
{
    public interface IDefinitionDataService
    {
        MemoryDatabase InMemoryData { get; }

        byte[] DefinitionsData { get; }

        GeneralConfigDefinition GetGeneralConfig();

        EnvironmentGenericConfig GetEnvironmentConfig();
    }
}