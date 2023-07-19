// Ignore Spelling: objs

using Core.Business;
using Cysharp.Threading.Tasks;
using Shared.Extension;
using Shared.Network;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Core.Framework
{
    public class AudioPoolManager : BasePoolManager
    {
        protected override PoolName PoolName => PoolName.Audio;

        private readonly GameStore.Setting _gameSetting;

        public AudioPoolManager(IObjectResolver container, GameStore.Setting gameSetting) : base(container)
        {
            _gameSetting = gameSetting;
        }

        public bool IsPlayingBGAudio(string audioPath)
        {
            var activeMusicChannel = GetActiveChannels<AudioChannelPoolObject>(AudioMixerType.Music, default);
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
                await GetPoolObject<T>(result.ModelObj.Name).GetItem();
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
                await GetPoolObject<T>(result.ModelObj.Name).GetItem();

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
            var objs = _instanceLookup.Keys
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
            await WarmPool<T>(prefPath);
            var pool = _prefabLookup[prefPath];

            var obj = await pool.GetItem();
            (obj as AudioChannelPoolObject)?.Initialize(type);
            ((UGameObject)obj.ModelObj).WrappedObj.transform.parent = objParent;

            _instanceLookup.Add(obj, pool);
            _dirtyLog = true;
            return (T)obj;
        }

        #endregion Channel Utilities

        #region Pool Default Handler

        private ObjectPool<T> GetPoolObject<T>(string key) where T : IPoolObject
        {
            return _prefabLookup[key] as ObjectPool<T>;
        }

        private void ReinitializeChannelPool<T>(IPoolObject result, Vec3D position) where T : IPoolObject
        {
            result.Reinitialize();
            result.SetupPoolObjectContainer();
            ((BasePoolObject)result).transform.position = new Vector3(position.x, position.y, position.z);
        }

        #endregion Pool Default Handler
    }
}
