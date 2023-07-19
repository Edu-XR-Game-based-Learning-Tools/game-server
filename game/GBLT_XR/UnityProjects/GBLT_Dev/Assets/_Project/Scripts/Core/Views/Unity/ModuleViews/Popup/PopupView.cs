using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Utility;
using MessagePipe;
using Microsoft.MixedReality.Toolkit.UX;
using Shared.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;
using VContainer;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace Core.View
{
    public class PopupView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;

        [SerializeField][DebugOnly] private TextMeshProUGUI _titleTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _contentTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _yesTxt;
        [SerializeField][DebugOnly] private TextMeshProUGUI _noTxt;
        [SerializeField][DebugOnly] private PressableButton _yesBtn;
        [SerializeField][DebugOnly] private PressableButton _noBtn;
        [SerializeField][DebugOnly] private Transform[] _inputFieldContainers;
        [SerializeField][DebugOnly] private MRTKTMPInputField[] _inputFields;

        [Inject]
        private readonly ISubscriber<ShowPopupSignal> _showPopupSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        private ShowPopupSignal _signal;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);

            _disposableBagBuilder = DisposableBag.CreateBuilder();
            _showPopupSubscriber.Subscribe(OnShowPopup).AddTo(_disposableBagBuilder);
        }

        private void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        private void GetReferences()
        {
            _titleTxt = transform.Find("CanvasDialog/Canvas/Header").GetComponent<TextMeshProUGUI>();
            _contentTxt = transform.Find("CanvasDialog/Canvas/Main Text").GetComponent<TextMeshProUGUI>();
            _yesBtn = transform.Find("CanvasDialog/Canvas/Horizontal/Yes_Btn").GetComponent<PressableButton>();
            _noBtn = transform.Find("CanvasDialog/Canvas/Horizontal/No_Btn").GetComponent<PressableButton>();
            _yesTxt = _yesBtn.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>();
            _noTxt = _noBtn.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TextMeshProUGUI>();

            _inputFieldContainers = new Transform[2];
            _inputFields = new MRTKTMPInputField[2];
            _inputFieldContainers[0] = transform.Find("CanvasDialog/Canvas/InputField");
            _inputFields[0] = _inputFieldContainers[0].Find("InputField (TMP)").GetComponent<MRTKTMPInputField>();
            _inputFieldContainers[1] = transform.Find("CanvasDialog/Canvas/InputField (1)");
            _inputFields[1] = _inputFieldContainers[1].Find("InputField (TMP)").GetComponent<MRTKTMPInputField>();
        }

        private void RegisterEvents()
        {
            _yesBtn.OnClicked.AddListener(() =>
            {
                _signal?.YesAction?.Invoke(_inputFields[0].text, _inputFields[1].text);

                transform.SetActive(false);
                _signal?.OnClose?.Invoke();
            });

            _noBtn.OnClicked.AddListener(() =>
            {
                _signal?.NoAction?.Invoke(_inputFields[0].text, _inputFields[1].text);

                transform.SetActive(false);
                _signal?.OnClose?.Invoke();
            });
        }

        public override void OnReady()
        {
            GetReferences();
            RegisterEvents();

            transform.SetActive(false);
        }

        public void Refresh()
        {
        }

        private bool CheckEnableIfNotEmpty(string text, TextMeshProUGUI view)
        {
            bool isEnable = !string.IsNullOrEmpty(text);
            view.SetActive(isEnable);
            if (isEnable) view.text = text;
            return isEnable;
        }

        private bool CheckEnableIfNotEmpty(string text, dynamic action, PressableButton view)
        {
            bool isEnable = !string.IsNullOrEmpty(text) && action != null;
            view.SetActive(isEnable);
            return isEnable;
        }

        private void SetInitialInput(int index, ShowPopupSignal signal)
        {
            _inputFieldContainers[index].SetActive(signal.IsShowInputs.IsNotNullAndGreaterThan(index) ? signal.IsShowInputs[index] : false);
            _inputFields[index].text = signal.InitialInputValues.IsNotNullAndGreaterThan(index) ? signal.InitialInputValues[index] : "";
            _inputFields[index].placeholder.GetComponent<TextMeshProUGUI>().text = signal.InputPlaceholders.IsNotNullAndGreaterThan(index) ? signal.InputPlaceholders[index] : "";
        }

        private void OnShowPopup(ShowPopupSignal signal)
        {
            _signal = signal;

            if (signal.IsShow)
                _gameStore.HideAllExcept(signal.HideModules);
            else
            {
                _gameStore.RestoreLastHideModules();
                signal.OnClose?.Invoke();
            }

            transform.SetActive(signal.IsShow);

            if (!signal.IsShow) return;

            CheckEnableIfNotEmpty(_signal.Title, _titleTxt);
            CheckEnableIfNotEmpty(_signal.Content, _contentTxt);
            CheckEnableIfNotEmpty(_signal.YesContent, _yesTxt);
            CheckEnableIfNotEmpty(_signal.NoContent, _noTxt);
            CheckEnableIfNotEmpty(_signal.YesContent, _signal.YesAction, _yesBtn);
            CheckEnableIfNotEmpty(_signal.NoContent, _signal.NoAction, _noBtn);

            SetInitialInput(0, signal);
            SetInitialInput(1, signal);
        }
    }
}
