using Core.Business;
using Cysharp.Threading.Tasks;
using System;
using Unity.Services.Vivox;
using VContainer.Unity;

namespace Core.Network
{
    public class VivoxVoiceCallService : IVoiceCallService, IStartable, IDisposable
    {
        private string _channelName;

        public VivoxVoiceCallService()
        {
        }

        public void Start()
        {
            VivoxService.Instance.LoggedIn += OnUserLoggedIn;
            VivoxService.Instance.LoggedOut += OnUserLoggedOut;

            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

            VivoxService.Instance.ConnectionRecovered += OnConnectionRecovered;
            VivoxService.Instance.ConnectionRecovering += OnConnectionRecovering;
            VivoxService.Instance.ConnectionFailedToRecover += OnConnectionFailedToRecover;
        }

        #region Lobby Action

        public async UniTask TransmissionAll()
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.All);
        }

        public async UniTask TransmitToChannel(string channelName = null)
        {
            channelName ??= _channelName;
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, channelName);
        }

        public async UniTask TransmitToNone()
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.None);
        }

        #endregion Lobby Action

        #region Session Action

        public async UniTask LoginToVivoxAsync(string userDisplayName)
        {
            LoginOptions options = new()
            {
                DisplayName = userDisplayName,
                ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond
            };
            await VivoxService.Instance.LoginAsync(options);
        }

        public async UniTask JoinChannelAsync(string name = null, string channel = null)
        {
            if (!PermissionHelper.RequestMicrophonePermission())
                throw new Exception("The app required mic permission");
            _channelName = channel ?? "Temp_Room";
            await LoginToVivoxAsync(name);
        }

        public async UniTask LeaveChannelAsync()
        {
            await VivoxService.Instance.LeaveChannelAsync(_channelName);
            await LogoutOfVivoxAsync();
        }

        public async UniTask LogoutOfVivoxAsync()
        {
            await VivoxService.Instance.LogoutAsync();
        }

        public (byte[], int) GetSamples()
        {
            throw new NotImplementedException();
        }

        private async void OnUserLoggedIn()
        {
            await VivoxService.Instance.JoinGroupChannelAsync(_channelName, ChatCapability.AudioOnly);
        }

        private void OnUserLoggedOut()
        { }

        private void OnParticipantAdded(VivoxParticipant participant)
        { UnityEngine.Debug.Log($"OnParticipantAdded {participant.ChannelName}"); }

        private void OnParticipantRemoved(VivoxParticipant participant)
        { UnityEngine.Debug.Log($"OnParticipantRemoved {participant.ChannelName}"); }

        private void OnConnectionRecovering()
        { }

        private void OnConnectionRecovered()
        { }

        private void OnConnectionFailedToRecover()
        { }

        #endregion Session Action

        public void Dispose()
        {
            VivoxService.Instance.LoggedIn -= OnUserLoggedIn;
            VivoxService.Instance.LoggedOut -= OnUserLoggedOut;

            VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;

            VivoxService.Instance.ConnectionRecovered -= OnConnectionRecovered;
            VivoxService.Instance.ConnectionRecovering -= OnConnectionRecovering;
            VivoxService.Instance.ConnectionFailedToRecover -= OnConnectionFailedToRecover;
        }
    }
}
