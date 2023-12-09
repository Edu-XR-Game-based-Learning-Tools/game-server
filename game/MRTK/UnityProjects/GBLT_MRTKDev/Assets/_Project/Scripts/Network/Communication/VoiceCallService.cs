using Core.Business;
using UnityEngine;
using System;
using Shared.Extension;
using Shared;
using Cysharp.Threading.Tasks;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)

using UnityEngine.Android;

#endif

namespace Core.Network
{
    public class PermissionHelper
    {
        public static bool RequestMicrophonePermission()
        {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
            return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#endif
            return true;
        }

        public static bool RequestCameraPermission()
        {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                return Permission.HasUserAuthorizedPermission(Permission.Camera);
            }
#endif
            return true;
        }
    }

    public class VoiceCallService : MonoBehaviour, IVoiceCallService, IDisposable
    {
        // Audio control variables
        public AudioClip MicAudioClip { get; private set; }

        private int _samplePosition, _lastSamplePosition;

        public UniTask JoinChannelAsync(string token = null, string channel = null)
        {
            MicAudioClip = Microphone.Start(null, true, Defines.MIC_SAMPLE_LENGTH, Defines.MIC_FREQUENCY);
            return UniTask.CompletedTask;
        }

        public UniTask LeaveChannelAsync()
        {
            Microphone.End(null);
            MicAudioClip = null;
            return UniTask.CompletedTask;
        }

        public (byte[], int) GetSamples()
        {
            byte[] data = new byte[0];
            if (!PermissionHelper.RequestMicrophonePermission()) return (data, 0);

            int temp = _lastSamplePosition;
            if ((_samplePosition = Microphone.GetPosition(null)) > 0)
            {
                int diff = _samplePosition - _lastSamplePosition;
                if (diff > 0)
                {
                    // Allocate the space for the new sample.
                    float[] samples = new float[diff * MicAudioClip.channels];
                    MicAudioClip.GetData(samples, _lastSamplePosition);
                    data = samples.ConvertFloatToByte();
                }
                _lastSamplePosition = _samplePosition;
            }

            return (data, temp);
        }

        public void Dispose()
        {
            LeaveChannelAsync();
        }
    }
}
