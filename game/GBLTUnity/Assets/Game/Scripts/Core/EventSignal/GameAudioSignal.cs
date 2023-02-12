using Core.Business;
using UnityEngine;

namespace Core.EventSignal
{
    public class BaseAudioSignal
    {
        public AudioMixerType AudioType;
        public string AudioPath;

        public Vec3D Position = Vec3D.Zero;
        public float Volume = 1f;

        public BaseAudioSignal(string audioPath,
            AudioMixerType audioType = AudioMixerType.Music)
        {
            AudioType = audioType;
            AudioPath = audioPath;
        }

        public virtual T SetupPosition<T>(Vec3D position) where T : BaseAudioSignal
        {
            Position = position;
            return (T)this;
        }

        public virtual T SetupPosition<T>(Vector3 position) where T : BaseAudioSignal
        {
            Position = new Vec3D(position.x, position.y, position.z);
            return (T)this;
        }

        public virtual T SetupAudioSetting<T>(float volume = 1f) where T : BaseAudioSignal
        {
            Volume = volume;
            return (T)this;
        }
    }

    public class PlayOneShotAudioSignal : BaseAudioSignal
    {
        public PlayOneShotAudioSignal(string audioPath,
            AudioMixerType audioType = AudioMixerType.Music)
            : base(audioPath, audioType)
        {
        }
    }

    public class GameAudioSignal : BaseAudioSignal
    {
        public AudioActionType ActionType;
        public float Pitch = 1f;
        public bool IsLoop = false;
        public bool IsFade = true;

        public GameAudioSignal(string audioPath, AudioMixerType audioType = AudioMixerType.Music,
            AudioActionType actionType = AudioActionType.START)
            : base(audioPath, audioType)
        {
            ActionType = actionType;
        }

        public GameAudioSignal SetupAudioSetting(float volume = 1f, float pitch = 1f, bool isLoop = false)
        {
            base.SetupAudioSetting<GameAudioSignal>(volume);
            Pitch = pitch;
            IsLoop = isLoop;
            return this;
        }

        public GameAudioSignal SetupAudioFadeOnStop(bool isFade = true)
        {
            IsFade = isFade;
            return this;
        }
    }
}