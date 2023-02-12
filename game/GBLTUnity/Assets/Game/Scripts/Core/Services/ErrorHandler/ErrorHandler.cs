using Core.Business;
using Core.EventSignal;
using System;
using UnityEngine;
using Zenject;

namespace Core.Framework
{
    public class ErrorHandler : IDisposable
    {
        private readonly GameStore _gameStore;
        private readonly SignalBus _signalBus;

        public ErrorHandler(
            SignalBus signalBus,
            GameStore gameStore)
        {
            _gameStore = gameStore;
            _signalBus = signalBus;

            SubscribeToApplicationLogEvent();
        }

        private bool _isQuitting;
        private bool _isShownError;
        private bool _isForceLogout;

        private void SubscribeToApplicationLogEvent()
        {
            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceived += HandleLog;
            _signalBus.Subscribe<GameScreenForceChangeSignal>(OnScreenForceChange);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= Quit;
            UnityEditor.EditorApplication.playModeStateChanged += Quit;
#endif
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    // TODO: send error to server or any where we want, through another class like
                    // TargetLogger.Send(xxxx);
                    break;
                case LogType.Warning:
                    // TODO: send error to server or any where we want, through another class like
                    // TargetLogger.Send(xxxx);
                    break;
                case LogType.Error:
                    // TODO: send error to server or any where we want, through another class like
                    // TargetLogger.Send(xxxx);
                    break;
                case LogType.Exception:
                default:
                    // TODO: send error to server or any where we want, through another class like
                    // TargetLogger.Send(xxxx);
                    break;
            }
            if (type == LogType.Exception || type == LogType.Error)
                ShowErrorPopup(logString);
        }

        public void ShowErrorPopup(string logString)
        {
            InternalShowErrorPopup("Encounter internal error exception.\nPlease reload your game and verify your network.", "Error", "Reload");
        }

        public void ShowUnauthenticatedPopup(string logString, bool isForceLogout = false)
        {
            InternalShowErrorPopup(logString, "Invalid Session", "Reload", isForceLogout);
        }

        public void ShowErroPopupWithCustomTitleAndYesText(string logString, string title, string yesText)
        {
            InternalShowErrorPopup(logString, title, yesText);
        }

        private void InternalShowErrorPopup(string msg, string title, string yesText, bool isForceLogout = false)
        {
            if (_isQuitting || _isShownError)
                return;

            _isForceLogout = isForceLogout;

            // Popup to ReloadGame

            _isShownError = true;
        }

        private void ReloadGame()
        {
            if (_isForceLogout)
            {
                // Logout
            }
            bool isSuccess = _gameStore.ChangeScreen(ScreenName.Restart);
            // if cannot change screen the _isShownError will be false to able show next error
            // if can change screen the _isShownError will be false after the restart finish.
            _isShownError = isSuccess;
        }

        private void OnScreenForceChange(GameScreenForceChangeSignal e)
        {
            _isShownError = false;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameScreenForceChangeSignal>(OnScreenForceChange);
        }

#if UNITY_EDITOR
        private void Quit(UnityEditor.PlayModeStateChange state)
        {
            _isQuitting = !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode
                && UnityEditor.EditorApplication.isPlaying;
        }
#endif
    }
}