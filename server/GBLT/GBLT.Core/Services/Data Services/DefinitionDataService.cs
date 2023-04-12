using Core.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Shared.Network;

namespace Core.Service
{
    public class DefinitionDataService : IDefinitionDataService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        public MemoryDatabase InMemoryData { get; set; }

        private byte[] _definitionsData;
        private readonly string _rootPath;
        private readonly IConfiguration _configuration;

        public DefinitionDataService(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _hostEnvironment = webHostEnvironment;
            _rootPath = Path.GetFullPath(_hostEnvironment.ContentRootPath);
            _configuration = configuration;

            Init();
        }

        private TDefinition[] LoadDefinitions<TDefinition>()
        {
            var fullPath = Path.Combine(_rootPath + "/Definitions", ConfigFileName.GetFileName<TDefinition>());
            return JsonFileReader.Load<TDefinition[]>(fullPath);
        }

        private void Init()
        {
            var builder = new DatabaseBuilder();

            var generalConfig = LoadDefinitions<GeneralConfigDefinition>();
            builder.Append(generalConfig);

            _definitionsData = builder.Build();
            InMemoryData = new MemoryDatabase(_definitionsData);
        }

        public GeneralConfigDefinition GetGeneralConfig()
        {
            return InMemoryData.GeneralConfigDefinitionTable.FindById("Default");
        }

        public EnvironmentGenericConfig GetEnvironmentConfig()
        {
            return _configuration.GetSection("EnvironmentGenericConfig").Get<EnvironmentGenericConfig>();
        }
    }
}