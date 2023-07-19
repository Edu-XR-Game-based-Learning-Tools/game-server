using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Core.Business
{
    public class DownloadProgress
    {
        public double totalMegaBytes;
        public double downloadedMegaBytes;
        public float progress;
        public string downloadSpeed;
    }

    public interface IBundleLoader
    {
        void CheckPreloadAsset(object key, string errorMessage);

        void CheckPreloadAssets(List<string> keys, string errorMessage);

        void DownloadPreloadAsset(System.Action<DownloadProgress> callback, string key, string errorMessage);

        void DownloadPreloadAssets(System.Action<DownloadProgress> callback, List<string> keys, string errorMessage);

        UniTask<GameObject> InstantiateAssetAsync(string path);

        UniTask<T> LoadAssetAsync<T>(string path) where T : Object;

        UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : Object;

        void ReleaseAsset(string path);

        void ReleaseAsset(AsyncOperationHandle handle);

        void ReleaseAsset(AssetReference reference);

        void ReleaseInstance(GameObject instanceByAddressable);

        UniTask<SceneInstance> LoadSceneAndActiveAsync(string sceneName, LoadSceneMode mode);

        UniTask UnLoadScene(SceneInstance scene);
    }
}