using Core.Business;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Framework
{
    public class AudioChannelPoolObject : BasePoolObject
    {
        private bool _isPause = false;
        private bool _isDespawn = false;
        private float _despawnTimer = -1.0f;
        public AudioMixerType AudioType => _audioType;
        private AudioMixerType _audioType = AudioMixerType.Music;
        private AudioSource _audioSource;
        private string _audioPath;
        public string AudioPath => _audioPath;

        public AudioChannelPoolObject(
            [Inject(Id = "PoolObjectContainer")]
            Transform poolObjectContainer,
            [Inject(Id = BundleLoaderName.Addressable)]
            IBundleLoader bundle,
            AudioPoolManager poolManager) : base(poolObjectContainer, bundle, poolManager)
        {
        }

        public void Initialize(AudioMixerType audioType)
        {
            _audioType = audioType;
            _audioSource = _modelObj.WrappedObj.GetComponent<AudioSource>();
        }

        public override void Reinitialize()
        {
            _isPause = false;
            _despawnTimer = -1f;
            _isDespawn = false;
            PoolObjectContainer = transform.parent;
        }

        #region Setup Channel

        public async void SelfDespawnAfter(float seconds = -1)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds));
            if (!_audioSource.isPlaying && !_isDespawn) Stop(false);
        }

        public AudioChannelPoolObject SetAudioClip(AudioClip clip, string source = "")
        {
            _audioSource.clip = clip;
            _audioPath = source;
            return this;
        }

        public AudioChannelPoolObject SetAudioConfig(float volume = 0f, float pitch = 1f, bool isLoop = false)
        {
            _audioSource.volume = volume;
            _audioSource.pitch = pitch;
            _audioSource.loop = isLoop;
            if (_despawnTimer < 0 && !isLoop)
                SelfDespawnAfter(_audioSource.clip.length);
            return this;
        }

        #endregion Setup Channel

        #region Audio Action

        public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
        {
            float errorDelay = 0.1f;
            _audioSource.PlayOneShot(clip, volumeScale);
            SelfDespawnAfter(clip.length + errorDelay);
        }

        public void Restart()
        {
            _audioSource.Stop();
            _audioSource.Play();
        }

        public void Resume()
        {
            if (_isPause) _audioSource.UnPause();
            else _audioSource.Play();
        }

        public void Pause()
        {
            _isPause = true;
            _audioSource.Pause();
        }

        private void Stop()
        {
            _audioSource.Stop();
            _poolManager.Despawn(this);
            _isDespawn = true;
        }

        public void Stop(bool isFade = true)
        {
            if (_isDespawn) return;
            _isPause = false;
            if (isFade) FadeVolume(() => Stop());
            else Stop();
        }

        #endregion Audio Action

        #region Utilities

        public string GetCurrentAudioClip()
        {
            return _audioSource.clip.name;
        }

        private void FadeVolume(System.Action onCompete = null)
        {
            //float _volume = _audioSource.volume;
            //DOTween
            //.To(
            //    (value) =>
            //    {
            //        _audioSource.volume = value;
            //    },
            //    _volume, 0f, 0.5f)
            //.SetEase(Ease.Linear)
            //.OnComplete(() =>
            //{
            //    onCompete?.Invoke();
            //})
            //.Play();
        }

        #endregion Utilities

        public override void SelfDespawn(IPoolManager poolManager)
        {
            poolManager.Despawn(this);
        }
    }
}