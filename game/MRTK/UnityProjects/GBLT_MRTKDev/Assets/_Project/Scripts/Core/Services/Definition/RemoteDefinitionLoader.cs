using Core.Business;
using Cysharp.Threading.Tasks;
using Shared.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Framework
{
    public class RemoteDefinitionLoader : IDefinitionLoader
    {
        public MemoryDatabase InMemoryData { get; set; }

        private Dictionary<Type, Func<BaseItemDefinition[]>> _dict;

        public RemoteDefinitionLoader()
        {
            _dict = new Dictionary<Type, Func<BaseItemDefinition[]>>
            {
                { typeof(GeneralConfigDefinition), GetGeneralConfigDefinitions },
                { typeof(ClassRoomDefinition), GetClassRoomDefinitions },
            };
        }

        public UniTask<TDefinition[]> LoadDefinitions<TDefinition>() where TDefinition : class, IDefinition
        {
            return UniTask.FromResult(_dict[typeof(TDefinition)].Invoke() as TDefinition[]);
        }

        //private async UniTask<List<TDef>> GetNewDefintion<TDef>() where TDef : BaseItemDefinition
        //{
        //    UnityWebRequest request = UnityWebRequest.Get(EnvSetting.DefinitionUrl + ConfigFileName.GetFileName<TDef>());
        //    await request.SendWebRequest();
        //    var response = request.downloadHandler.text;
        //    if (response != null)
        //    {
        //        var content = JsonConvert.DeserializeObject<List<TDef>>(response);
        //        return content;
        //    }
        //    return null;
        //}

        public void InitMemoryDefinitions(byte[] definitions)
        {
            InMemoryData = new MemoryDatabase(definitions);
        }

        public BaseItemDefinition[] GetGeneralConfigDefinitions()
        {
            return InMemoryData.GeneralConfigDefinitionTable.All.ToArray();
        }

        public BaseItemDefinition[] GetClassRoomDefinitions()
        {
            return InMemoryData.ClassRoomDefinitionTable.All.ToArray();
        }
    }
}
