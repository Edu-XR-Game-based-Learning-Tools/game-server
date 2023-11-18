using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using Core.Utility;
using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

namespace Core.View
{
    public class ToastView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;

        [SerializeField][DebugOnly] private TextMeshProUGUI _contentTxt;

        [Inject]
        private readonly ISubscriber<ShowToastSignal> _showToastSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        private Coroutine _showCo;

        [Inject]
        public void Init(
            GameStore gameStore,
            IObjectResolver container)
        {
            _gameStore = gameStore;
            _audioPoolManager = (AudioPoolManager)container.Resolve<IReadOnlyList<IPoolManager>>().ElementAt((int)PoolName.Audio);

            _disposableBagBuilder = DisposableBag.CreateBuilder();
            _showToastSubscriber.Subscribe(OnShowToast).AddTo(_disposableBagBuilder);
        }

        private void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        private void GetReferences()
        {
            _contentTxt = transform.Find("CanvasDialog/Canvas/Content").GetComponent<TextMeshProUGUI>();
        }

        private void RegisterEvents()
        {
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

        private IEnumerator DespawnLoadingCo(ShowToastSignal signal)
        {
            yield return new WaitForSeconds(signal.DespawnTime);
            transform.SetActive(false);
            signal.OnClose?.Invoke();
            OnShowToast(new ShowToastSignal(isShow: false));
        }

        private async void OnShowToast(ShowToastSignal signal)
        {
            if (signal.IsShow)
            {
                _gameStore.HideAllExcept(signal.HideModules);
                _contentTxt.text = signal.Content;
            }
            else
            {
                _gameStore.RestoreLastHideModules();
                signal.OnClose?.Invoke();
            }

            transform.SetActive(signal.IsShow);

            if (signal.IsShow && signal.DespawnTime != 0)
            {
                await UniTask.DelayFrame(1);
                _showCo = StartCoroutine(DespawnLoadingCo(signal));
            }
            else if (_showCo != null) StopCoroutine(_showCo);
        }
    }
}
