using Core.Business;
using Core.EventSignal;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Zenject;

namespace Core.Framework
{
    public class DownloadDependency
    {
        private readonly Stopwatch _downloadTimeCounter = new Stopwatch();
        private readonly SignalBus _signalBus;

        private Action<DownloadProgress> _downloadCallback;
        private string _errorMessage;

        public DownloadDependency(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Init(Action<DownloadProgress> callback, string errorMessage)
        {
            _downloadCallback = callback;
            _errorMessage = errorMessage;
        }

        public void DownloadAsset(Action<DownloadProgress> callback, string key, string errorMessage)
        {
            Init(callback, errorMessage);
            var handle = Addressables.DownloadDependenciesAsync(key);
            Shared_DownloadAssets(handle).Forget();
        }

        public void DownloadAssets(
            Action<DownloadProgress> callback,
            List<string> keys,
            Addressables.MergeMode mergeMode,
            string errorMessage)
        {
            Init(callback, errorMessage);
            var handle = Addressables.DownloadDependenciesAsync(keys, mergeMode);
            Shared_DownloadAssets(handle).Forget();
        }

        public void DownloadAssets(
            Action<DownloadProgress> callback,
            IList<IResourceLocation> locations,
            string errorMessage)
        {
            Init(callback, errorMessage);
            var handle = Addressables.DownloadDependenciesAsync(locations);
            Shared_DownloadAssets(handle).Forget();
        }

        private async UniTaskVoid Shared_DownloadAssets(AsyncOperationHandle handle)
        {
            var downloadStatus = GetDownloadStatus_Obj(handle);

            _downloadTimeCounter.Start();

            while (!handle.GetDownloadStatus().IsDone)
            {
                UpdateProgress(downloadStatus, handle);
                await UniTask.NextFrame();
            }

            _downloadTimeCounter.Stop();

            DownloadDone(handle);
            Addressables.Release(handle);
        }

        private DownloadProgress GetDownloadStatus_Obj(AsyncOperationHandle handle)
        {
            DownloadProgress downloadStatus = new DownloadProgress()
            {
                totalMegaBytes =
                    handle.GetDownloadStatus().TotalBytes /
                    AddressableLoader.ONE_MEGABYTE_TO_BYTE
            };
            return downloadStatus;
        }

        private void UpdateProgress(DownloadProgress status, AsyncOperationHandle handle)
        {
            long downloadedBytes = handle.GetDownloadStatus().DownloadedBytes;
            float percent = handle.GetDownloadStatus().Percent;

            status.downloadedMegaBytes = downloadedBytes / AddressableLoader.ONE_MEGABYTE_TO_BYTE;
            status.progress = percent;

            double KBsPerSecond =
                status.downloadedMegaBytes *
                AddressableLoader.ONE_MEGABYTE_TO_KILOBYTE /
                _downloadTimeCounter.Elapsed.TotalSeconds;

            if (KBsPerSecond >= AddressableLoader.ONE_MEGABYTE_TO_KILOBYTE)
            {
                status.downloadSpeed =
                    string.Format(
                        "({0:0.00 MB/s})",
                        KBsPerSecond / AddressableLoader.ONE_MEGABYTE_TO_KILOBYTE);
            }
            else
            {
                status.downloadSpeed =
                    string.Format(
                        "({0:0.00 KB/s})",
                        KBsPerSecond);
            }

            _downloadCallback.Invoke(status);
        }

        private void DownloadDone(AsyncOperationHandle handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // TODO: Want to do something after we downloaded completely ? ...
                _signalBus.Fire(new CheckDownloadSizeStatusSignal(0));
            }
            else if (
                handle.Status == AsyncOperationStatus.Failed ||
                handle.Status == AsyncOperationStatus.None)
            {
                // TODO: Handle error...
                // Here is an example of what happens if we catch an error.

                var signal = new AddressableErrorSignal(_errorMessage);
                _signalBus.Fire(signal);
            }
        }
    }
}