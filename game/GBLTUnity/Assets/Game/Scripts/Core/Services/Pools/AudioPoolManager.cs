using Core.Business;
using Core.Extension;
using Cysharp.Threading.Tasks;
using Shared.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zenject;

namespace Core.Framework
{
    public class AudioPoolManager : IPoolManager
    {
        private readonly GameStore.Setting _gameSetting;
        private readonly DiContainer _container;
        private readonly IBundleLoader _bundleLoader;

        private Dictionary<string, IPoolObject> _channelGos = new Dictionary<string, IPoolObject>();

        public AudioPoolManager(
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundleLoader,
            DiContainer container,
            GameStore.Setting gameSetting)
        {
            _bundleLoader = bundleLoader;
            _container = container;
            _gameSetting = gameSetting;
        }

        public bool IsPlayingBGAudio(string audioPath)
        {
            var activeMusicChannel = GetActiveChannels<AudioChannelPoolObject>(AudioMixerType.Music, default(Vec3D));
            return activeMusicChannel.Any(channel => channel.AudioPath == audioPath);
        }

        public async UniTask<T> GetChannel<T>(AudioMixerType type, Vec3D position, string prefPath, Transform objParent) where T : IPoolObject
        {
            var result = GetChannelOnPosition(type, position);
            if (result == null)
            {
                result = await GetUnactiveChannel<T>(type, position, prefPath, objParent);
            }
            else if (!result.ModelObj.IsActive)
            {
                GetPoolObject<T>(result.ModelObj.Name).Spawn();
                ReinitializeChannelPool<T>(result, position);
            }

            return (T)result;
        }

        public T[] GetActiveChannels<T>(AudioMixerType type, Vec3D position, string path = "") where T : IPoolObject
        {
            var result = GetActiveChannelsOnPosition(type, position, path);
            return result.Cast<T>().ToArray();
        }

        public async UniTask<T> GetUnactiveChannel<T>(AudioMixerType type, Vec3D position, string prefPath, Transform objParent) where T : IPoolObject
        {
            var result = GetUnactiveChannel(type);
            if (result == null)
                result = await SpawnNewChannelPool<T>(type, prefPath, objParent);
            else
                GetPoolObject<T>(result.ModelObj.Name).Spawn();

            ReinitializeChannelPool<T>(result, position);
            return (T)result;
        }

        #region Get Channel Object

        private IPoolObject GetUnactiveChannel(AudioMixerType type)
        {
            var validChannelObj = GetChannelsOnType(type)
                .Where(poolObj => !poolObj.ModelObj.IsActive).FirstOrDefault();
            return validChannelObj;
        }

        private IPoolObject[] GetActiveChannelsOnPosition(AudioMixerType type, Vec3D position, string path = "")
        {
            var validChannels = GetChannelsOnType(type)
                .Where(poolObj => poolObj.ModelObj.IsActive)
                .Where(poolObj => CheckValidPosition(poolObj, position))
                .Where(poolObj => poolObj.ModelObj.Name.Contains(path)).ToArray();
            return validChannels;
        }

        private IPoolObject GetChannelOnPosition(AudioMixerType type, Vec3D position)
        {
            var validChannelObj = GetChannelsOnType(type)
                .Where(poolObj => CheckValidPosition(poolObj, position)).FirstOrDefault();
            return validChannelObj;
        }

        private IPoolObject[] GetChannelsOnType(AudioMixerType type)
        {
            var objs = _channelGos.Values
                .Where(poolObj => poolObj != null && poolObj.ModelObj != null)
                .Where(poolObj => CheckAudioMixerType(poolObj, type)).ToArray();
            return objs;
        }

        #endregion Get Channel Object

        #region Channel Utilities

        private bool CheckValidPosition(IPoolObject poolObj, Vec3D position)
        {
            var objPos = poolObj.ModelObj.Position;
            return position.x.IsBetweenRange(objPos.x - _gameSetting.ValidAudioDistance, objPos.x + _gameSetting.ValidAudioDistance) &&
                position.y.IsBetweenRange(objPos.y - _gameSetting.ValidAudioDistance, objPos.y + _gameSetting.ValidAudioDistance) &&
                position.y.IsBetweenRange(objPos.z - _gameSetting.ValidAudioDistance, objPos.z + _gameSetting.ValidAudioDistance);
        }

        private bool CheckAudioMixerType(IPoolObject poolObj, AudioMixerType type)
        {
            return (poolObj as AudioChannelPoolObject).AudioType == type;
        }

        private async UniTask<T> SpawnNewChannelPool<T>(AudioMixerType type, string prefPath, Transform objParent) where T : IPoolObject
        {
            string key = new StringBuilder(_gameSetting.ChannelPrefix, 100).Append(_channelGos.Keys.Count).ToString();
            var result = CreateNewPool<T>(key).Spawn();
            _channelGos[key] = result;
            await CreateUGoChannelForObj(result, key, prefPath, type, objParent);
            return (T)result;
        }

        #endregion Channel Utilities

        #region Pool Default Handler

        private MemoryPoolObject GetPoolObject<T>(string key) where T : IPoolObject
        {
            return _container.ResolveId<MemoryPoolObject>(key);
        }

        private void ReinitializeChannelPool<T>(IPoolObject result, Vec3D position) where T : IPoolObject
        {
            result.Reinitialize();
            result.SetupPoolObjectContainer();
            ((BasePoolObject)result).transform.position = new Vector3(position.x, position.y, position.z);
        }

        private async UniTask CreateUGoChannelForObj<T>(T result, string key, string prefPath, AudioMixerType type, Transform objParent) where T : IPoolObject
        {
            UGameObject UGo = await InstantiateUGameObject(prefPath);
            UGo.WrappedObj.transform.parent = objParent;
            AssignUGoToObj(UGo, result, key);
            (result as AudioChannelPoolObject)?.Initialize(type);
        }

        private T AssignUGoToObj<T>(UGameObject UGo, T result, string prefPath) where T : IPoolObject
        {
            result.ModelObj = UGo;
            result.ModelObj.Name = prefPath;
            return result;
        }

        private async UniTask<UGameObject> InstantiateUGameObject(string prefPath)
        {
            GameObject go = await _bundleLoader.InstantiateAssetAsync(prefPath);
            return new UGameObject(go);
        }

        private MemoryPoolObject CreateNewPool<T>(string key) where T : IPoolObject
        {
            _container.BindMemoryPool<IPoolObject, MemoryPoolObject>().WithId(key).WithInitialSize(10).ExpandByDoubling().To<T>().AsCached();
            return GetPoolObject<T>(key);
        }

        public void Despawn(IPoolObject obj)
        {
            MemoryPoolObject pool = _container.ResolveId<MemoryPoolObject>(obj.ModelObj.Name);
            pool.Despawn(obj);
        }

        public UniTask<T> GetObject<T>(string prefPath) where T : IPoolObject
        {
            throw new System.NotImplementedException();
        }

        public void ClearPool(IPoolObject obj)
        {
            throw new System.NotImplementedException();
        }

        public void ClearPool(string prefabPath)
        {
            throw new System.NotImplementedException();
        }

        #endregion Pool Default Handler
    }
}