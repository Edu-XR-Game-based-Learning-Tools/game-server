using Core.Business;
using Core.Utility;
using Cysharp.Threading.Tasks;
using Shared.Extension;
using Shared.Network;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework
{

    public class LocalUserCache
    {
        public Dictionary<string, Sprite> AvatarMap = new();

        public async UniTask<Sprite> GetUserAvatar(string path)
        {
            if (!AvatarMap.ContainsKey(path))
                AvatarMap[path] = await IMG2Sprite.FetchImageSprite(path);
            return AvatarMap[path];
        }
    }

    public class UserDataController : IUserDataController
    {
        public UserServerEntity ServerData { get; private set; }
        public LocalUserCache LocalUserCache { get; set; }

        private readonly IDefinitionManager _definitionManager;

        public ClassRoomDefinition[] ClassRoomDefinitions { get => _classRoomDefinitions; }
        private ClassRoomDefinition[] _classRoomDefinitions;

        public UserDataController(
            IDefinitionManager definitionManager)
        {
            _definitionManager = definitionManager;
            ServerData = new UserServerEntity();
        }

        public async UniTaskVoid CacheDefinitions()
        {
            var classRoomDefs = await _definitionManager.GetAllDefinition<ClassRoomDefinition>();
            _classRoomDefinitions = classRoomDefs.ToArray();
        }

        public ClassRoomDefinition GetClassRoomDefinition(string id)
        {
            return _classRoomDefinitions.Find(c => c.Id == id);
        }
    }
}
