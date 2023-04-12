using Core.Business;
using Core.EventSignal;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

namespace Core.Framework
{
    public class AddressableLoader : IBundleLoader
    {
        // CONVERSION.
        public const float ONE_GIGABYTE_TO_MEGABYTE = 1024.0f;
        public const float ONE_MEGABYTE_TO_BYTE = 1048576.0f;
        public const float ONE_MEGABYTE_TO_KILOBYTE = 1024.0f;

        private const string DEFAULT_CHECK_ERROR_MESSAGE =
            "Something wrong! \n" +
            "Please check your internet connection";

        private const string DEFAULT_DOWNLOAD_ERROR_MESSAGE =
            "Something wrong! \n" +
            "Cannot download resources...!\n" +
            "Please check your internet connection...";

        private readonly CheckDownloadSize _check;
        private readonly DownloadDependency _download;
        private readonly SignalBus _signalBus;

        private Dictionary<string, AsyncOperationHandle> _loadedOperations =
            new Dictionary<string, AsyncOperationHandle>();

        private bool IsInitialized = false;

        public AddressableLoader(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _check = new CheckDownloadSize(signalBus);
            _download = new DownloadDependency(signalBus);

            InitializeAndCheckCatalogUpdates();
        }

        private void InitializeAndCheckCatalogUpdates()
        {
            Addressables.InitializeAsync().Completed += op =>
            {
                CheckForCatalogUpdates();
                IsInitialized = true;
            };
        }

        private void CheckForCatalogUpdates()
        {
            if (IsInitialized)
                return;

            List<string> catalogsToUpdate = new List<string>();
            Addressables.CheckForCatalogUpdates(true).Completed += op =>
            {
                catalogsToUpdate.AddRange(op.Result);
                UpdateCatalogs(catalogsToUpdate);
            };
        }

        private void UpdateCatalogs(List<string> catalogsToUpdate)
        {
            if (catalogsToUpdate.Count > 0)
                Addressables.UpdateCatalogs(catalogsToUpdate, true);
        }

        public void CheckPreloadAsset(object key, string errorMessage = DEFAULT_CHECK_ERROR_MESSAGE)
        {
            _check.CheckSingleKey(key, errorMessage).Forget();
        }

        public void CheckPreloadAssets(List<string> keys, string errorMessage = DEFAULT_CHECK_ERROR_MESSAGE)
        {
            _check.CheckMultipleKeys(keys, errorMessage).Forget();
        }

        public void DownloadPreloadAsset(
            System.Action<DownloadProgress> callback,
            string key,
            string errorMessage = DEFAULT_DOWNLOAD_ERROR_MESSAGE)
        {
            _download.DownloadAsset(callback, key, errorMessage);
        }

        public void DownloadPreloadAssets(
            System.Action<DownloadProgress> callback,
            List<string> keys,
            string errorMessage = DEFAULT_DOWNLOAD_ERROR_MESSAGE)
        {
            _download.DownloadAssets(callback, keys, Addressables.MergeMode.Union, errorMessage);
        }

        public async UniTask<GameObject> InstantiateAssetAsync(string path)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                throw new AddressableRunOnEditorMode(path);
#endif
            try
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(path);

                while (!handle.IsDone)
                    await UniTask.NextFrame();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result;
                }
                else if (
                    handle.Status == AsyncOperationStatus.Failed ||
                    handle.Status == AsyncOperationStatus.None)
                {
                    // TOTO: handle Error...What if addressable cannot instantiate asset.
                    // For example: Display the popup message or something else.
                }

                return null;
            }
            catch
            {
                throw new MissingAddressableAssetAtPath(path);
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                throw new AddressableRunOnEditorMode(path);
#endif
            try
            {
                AssetReference a = new AssetReference(path);
                AsyncOperationHandle<T> handle = a.LoadAssetAsync<T>();

                while (!handle.IsDone)
                    await UniTask.NextFrame();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (!_loadedOperations.ContainsKey(path))
                        _loadedOperations.Add(path, handle);
                    else
                        _loadedOperations[path] = handle;

                    return handle.Result;
                }
                else if (
                    handle.Status == AsyncOperationStatus.Failed ||
                    handle.Status == AsyncOperationStatus.None)
                {
                    // TOTO: handle Error...What if addressable cannot load asset.
                    // For example: Display the popup message or something else.
                }

                return null;
            }
            catch
            {
                throw new MissingAddressableAssetAtPath(path);
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(AssetReference reference) where T : Object
        {
            if (reference.Asset == null)
            {
                AsyncOperationHandle<T> handle = reference.LoadAssetAsync<T>();

                while (!handle.IsDone)
                    await UniTask.NextFrame();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // TODO: return "reference.Asset" or something else...
                }
                else if (
                    handle.Status == AsyncOperationStatus.Failed ||
                    handle.Status == AsyncOperationStatus.None)
                {
                    // TOTO: handle Error...What if addressable cannot load asset.
                    // For example: Display the popup message or something else.
                }
            }
            else
            {
                // TODO: What if reference has already loaded...
            }

            return default;
        }

        public void ReleaseAsset(string path)
        {
            if (_loadedOperations.ContainsKey(path))
            {
                if (_loadedOperations[path].IsValid())
                    Addressables.Release(_loadedOperations[path]);
            }
        }

        public void ReleaseAsset(AsyncOperationHandle handle)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        public void ReleaseAsset(AssetReference reference)
        {
            if (reference.Asset != null)
                reference.ReleaseAsset();
        }

        public void ReleaseInstance(GameObject instanceByAddressable)
        {
            if (instanceByAddressable != null)
                Addressables.ReleaseInstance(instanceByAddressable);
        }

        public async UniTask<SceneInstance> LoadSceneAndActiveAsync(string sceneName, LoadSceneMode mode)
        {
            try
            {
                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneName, mode, activateOnLoad: true);

                while (!handle.IsDone)
                {
                    var signal = new UpdateLoadingProgressSignal(handle.PercentComplete);
                    _signalBus.Fire(signal);

                    await UniTask.NextFrame();
                }

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // TODO: Scene has been loaded. Want to do something else ? ...

                    return handle.Result;
                }
                else if (
                    handle.Status == AsyncOperationStatus.Failed ||
                    handle.Status == AsyncOperationStatus.None)
                {
                    // TODO: Handle error...
                    // Here is an example of what happens if we cannot load the scene.

                    var signal = new AddressableErrorSignal("Cannot load scene: " + sceneName);
                    _signalBus.Fire(signal);
                }

                return default;
            }
            catch
            {
                throw new AddressableLoadMissingScene(sceneName);
            }
        }

        public async UniTask UnLoadScene(SceneInstance scene)
        {
            try
            {
                await Addressables.UnloadSceneAsync(scene, true);
            }
            catch (System.Exception error)
            {
                throw error;
            }
        }

        private class MissingAddressableAssetAtPath : System.Exception
        {
            public MissingAddressableAssetAtPath(string message) : base(message) { }
        }

        private class AddressableRunOnEditorMode : System.Exception
        {
            public AddressableRunOnEditorMode(string message) : base(message) { }
        }
        private class AddressableLoadMissingScene : System.Exception
        {
            public AddressableLoadMissingScene(string message) : base(message) { }
        }

    }
}
