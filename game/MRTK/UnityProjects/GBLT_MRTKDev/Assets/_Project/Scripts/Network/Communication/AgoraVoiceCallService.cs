using Agora.Rtc;
using Core.Business;
using Shared.Network;
using UnityEngine;
using VContainer.Unity;
using System;
using Shared.Extension;
using Core.Framework;
using Cysharp.Threading.Tasks;

namespace Core.Network
{
    public class AgoraVoiceCallService : IVoiceCallService, ITickable, IDisposable
    {
        private readonly GameStore _gameStore;
        private AgoraConfig _agoraConfig;

        private IRtcEngine _rtcEngine = null;

        public AgoraVoiceCallService(GameStore gameStore)
        {
            _gameStore = gameStore;

            Init(_gameStore.EnvConfig.AgoraConfig);
        }

        private void InitEngine()
        {
            if (_agoraConfig.AppId.IsNullOrEmpty())
            {
                Debug.LogWarning("Invalid Agora App ID");
                return;
            }

            _rtcEngine = RtcEngine.CreateAgoraRtcEngine();
            RtcEngineContext context = new RtcEngineContext(_agoraConfig.AppId, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
            _rtcEngine.Initialize(context);
        }

        public IVoiceCallService Init(AgoraConfig agoraConfig)
        {
            if (_rtcEngine != null) return this;
            _agoraConfig = agoraConfig;
            InitEngine();
            return this;
        }

        public UniTask JoinChannelAsync(string token = null, string channel = null)
        {
            token ??= _agoraConfig.Token;
            channel ??= _agoraConfig.AppChannel;
            if (channel.IsNullOrEmpty() || token.IsNullOrEmpty())
            {
                Debug.LogWarning("Invalid Agora Token or Channel");
                return UniTask.CompletedTask;
            }
            _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            _rtcEngine.EnableAudio();
            _rtcEngine.JoinChannel(token, channel);

            UserEventHandler handler = new UserEventHandler();
            _rtcEngine.InitEventHandler(handler);

            return UniTask.CompletedTask;
        }

        public UniTask LeaveChannelAsync()
        {
            if (_rtcEngine == null) return UniTask.CompletedTask;
            _rtcEngine.InitEventHandler(null);
            _rtcEngine.LeaveChannel();
            _rtcEngine.DisableAudio();
            return UniTask.CompletedTask;
        }

        public void Tick()
        {
            PermissionHelper.RequestMicrophonePermission();
        }

        public void Dispose()
        {
            if (_rtcEngine == null) return;
            LeaveChannelAsync();
            _rtcEngine.Dispose();
            _rtcEngine = null;
        }

        (byte[], int) IVoiceCallService.GetSamples()
        {
            throw new NotImplementedException();
        }

        public UniTask TransmissionAll()
        {
            throw new NotImplementedException();
        }

        public UniTask TransmitToChannel(string channelName = null)
        {
            throw new NotImplementedException();
        }

        public UniTask TransmitToNone()
        {
            throw new NotImplementedException();
        }
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        public UserEventHandler()
        {
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log(string.Format(
                "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                connection.localUid, elapsed));
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            Debug.Log("OnLeaveChannelSuccess");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            Debug.Log(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid,
                elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            Debug.Log(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }

        public override void OnError(int error, string msg)
        {
            Debug.Log(
                string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            Debug.Log(string.Format("OnConnectionLost "));
        }
    }

    #endregion -- Agora Event ---
}
