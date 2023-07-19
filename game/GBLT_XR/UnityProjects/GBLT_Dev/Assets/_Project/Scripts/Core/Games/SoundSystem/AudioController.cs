using Core.Business;
using Core.EventSignal;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Shared.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Core.Framework
{
    public class AudioController : MonoBehaviour, IDisposable
    {
        private IBundleLoader _bundleLoader;
        private AudioPoolManager _channelPool;

        [Inject]
        private readonly ISubscriber<GameAudioSignal> _gameAudioSubscriber;

        [Inject]
        private readonly ISubscriber<PlayOneShotAudioSignal> _playOneShotAudioSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        [SerializeField] private Transform[] _parentChannels;
        [SerializeField] private string[] _prefPaths;

        private string _audioPostFix = ".ogg";
        //private Business.ILogger _logger;

        [Inject]
        public void Construct(
            IObjectResolver container
            )
        {
            //_logger = container.Resolve<Business.ILogger>();
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
            _channelPool = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);

            Initialize();
        }

        private void Initialize()
        {
            _disposableBagBuilder = DisposableBag.CreateBuilder();
            _gameAudioSubscriber.Subscribe(HandleGameAudioSignal).AddTo(_disposableBagBuilder);
            _playOneShotAudioSubscriber.Subscribe(HandlePlayOneShotAudioSignal).AddTo(_disposableBagBuilder);
        }

        public void Dispose()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        private async UniTask<T[]> GetChannelOnAction<T>(GameAudioSignal signal) where T : IPoolObject
        {
            T[] channels;
            if (signal.ActionType == AudioActionType.START)
                channels = new T[] {
                    await _channelPool.GetUnactiveChannel<T>(
                    signal.AudioType, signal.Position,
                    _prefPaths[(int)signal.AudioType],
                    _parentChannels[(int)signal.AudioType])
                };
            else
                channels = _channelPool.GetActiveChannels<T>(
                        signal.AudioType, signal.Position, signal.AudioPath ?? "");
            return channels;
        }

        private async void HandleGameAudioSignal(GameAudioSignal signal)
        {
            AudioChannelPoolObject[] channels = await GetChannelOnAction<AudioChannelPoolObject>(signal);
            if (channels == null || channels.Length <= 0) return;

            switch (signal.ActionType)
            {
                case AudioActionType.START:
                    if (string.IsNullOrEmpty(signal.AudioPath)) return;
                    channels[0].SetAudioClip(await _bundleLoader.LoadAssetAsync<AudioClip>(
                        GetCompatibleAudioPath(signal.AudioPath)), signal.AudioPath)
                        .SetAudioConfig(signal.Volume, signal.Pitch, signal.IsLoop)
                        .Resume();
                    break;

                case AudioActionType.RESUME:
                    foreach (AudioChannelPoolObject ch in channels) ch.Resume();
                    break;

                case AudioActionType.RESTART:
                    foreach (AudioChannelPoolObject ch in channels) ch.Restart();
                    break;

                case AudioActionType.STOP:
                    foreach (AudioChannelPoolObject ch in channels) ch.Stop(signal.IsFade);
                    break;

                case AudioActionType.PAUSE:
                    foreach (AudioChannelPoolObject ch in channels) ch.Pause();
                    break;
            }
        }

        private async void HandlePlayOneShotAudioSignal(PlayOneShotAudioSignal signal)
        {
            if (string.IsNullOrEmpty(signal.AudioPath)) return;
            var channel = await _channelPool.GetChannel<AudioChannelPoolObject>(
                signal.AudioType, signal.Position,
                _prefPaths[(int)signal.AudioType],
                _parentChannels[(int)signal.AudioType]);
            AudioClip clip = await _bundleLoader.LoadAssetAsync<AudioClip>(GetCompatibleAudioPath(signal.AudioPath));
            if (clip != null)
                channel.PlayOneShot(clip, signal.Volume);
            else
                Debug.LogWarning($"Error: Cannot load audio clip at {signal.AudioPath}");
        }

        private string GetCompatibleAudioPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            string[] paths = path.Split('.');
            if (new string[] { "mp3", "ogg" }.Contains(paths[paths.Length - 1]))
                paths = paths.Take(paths.Length - 1).ToArray();
            return paths.Join(".") + _audioPostFix;
        }
    }
}
