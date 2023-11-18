using Core.Business;
using Core.Extension;
using Core.Utility;
using Cysharp.Threading.Tasks;
using Shared;
using Shared.Extension;
using Shared.Network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Core.Framework
{
    public class LocalUserCache
    {
        private readonly IBundleLoader _bundleLoader;
        public Dictionary<string, Sprite> SpriteMap = new();
        public Dictionary<string, GameObject> ModelMap = new();

        public LocalUserCache(IBundleLoader bundleLoader)
        {
            _bundleLoader = bundleLoader;
        }

        public async UniTask<Sprite> GetSprite(string path)
        {
            if (!SpriteMap.ContainsKey(path))
            {
                SpriteMap[path] = (await _bundleLoader.LoadAssetAsync<Texture2D>(path)).TexToSprite();
            }
            return SpriteMap[path];
        }

        public async UniTask<GameObject> GetModel(string path)
        {
            if (!ModelMap.ContainsKey(path))
            {
                ModelMap[path] = (await _bundleLoader.LoadAssetAsync<GameObject>(path));
            }
            return ModelMap[path];
        }
    }

    public class UserDataController : IUserDataController
    {
        public UserServerEntity ServerData { get; private set; }
        public LocalUserCache LocalUserCache { get; set; }

        private readonly IDefinitionManager _definitionManager;
        private readonly IBundleLoader _bundleLoader;

        public ClassRoomDefinition[] ClassRoomDefinitions { get => _classRoomDefinitions; }
        private ClassRoomDefinition[] _classRoomDefinitions;

        public UserDataController(
            IObjectResolver container,
            IDefinitionManager definitionManager)
        {
            _definitionManager = definitionManager;
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);

            ServerData = new UserServerEntity();
            LocalUserCache = new(_bundleLoader);
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
