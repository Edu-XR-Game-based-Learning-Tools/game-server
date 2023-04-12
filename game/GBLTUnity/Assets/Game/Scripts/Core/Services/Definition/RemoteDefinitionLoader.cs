using Core.Business;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace Core.Framework
{
    public class RemoteDefinitionLoader : IDefinitionLoader
    {
        private readonly ILogger _logger;

        public Dictionary<Type, BaseItemDefinition[]> InMemoryData { get; set; } = new Dictionary<Type, BaseItemDefinition[]>();

        private Dictionary<Type, Func<UniTask<BaseItemDefinition[]>>> _dict;

        public RemoteDefinitionLoader(
            ILogger logger)
        {
            _logger = logger;

            _dict = new Dictionary<Type, Func<UniTask<BaseItemDefinition[]>>>();
            _dict.Add(typeof(GeneralConfigDefinition), GetGeneralConfigDefinitions);
        }

        public async UniTask<TDefinition[]> LoadDefinitions<TDefinition>() where TDefinition : class, IGameDefinition
        {
            return await _dict[typeof(TDefinition)].Invoke() as TDefinition[];
        }

        private async UniTask<List<TDef>> GetNewDefintion<TDef>() where TDef : BaseItemDefinition
        {
            UnityWebRequest request = UnityWebRequest.Get(EnvSetting.DefinitionUrl + ConfigFileName.GetFileName<TDef>());
            await request.SendWebRequest();
            var response = request.downloadHandler.text;
            if (response != null)
            {
                var content = JsonConvert.DeserializeObject<List<TDef>>(response);
                return content;
            }
            return null;
        }

        public async UniTask<BaseItemDefinition[]> GetGeneralConfigDefinitions()
        {
            Type type = typeof(GeneralConfigDefinition);
            if (InMemoryData.ContainsKey(type))
                return InMemoryData[type].ToArray();

            var def = await GetNewDefintion<GeneralConfigDefinition>();
            if (def != null)
            {
                InMemoryData.Add(type, def.OfType<BaseItemDefinition>().ToArray());
                return def.ToArray();
            }
            else
                throw new Exception(type.ToString() + " Not Found");
        }
    }
}