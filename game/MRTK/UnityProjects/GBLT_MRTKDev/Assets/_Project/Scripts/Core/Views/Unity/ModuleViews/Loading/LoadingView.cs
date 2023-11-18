using Core.Business;
using Core.EventSignal;
using Core.Extension;
using Core.Framework;
using MessagePipe;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Core.View
{
    public class LoadingView : UnityView
    {
        private GameStore _gameStore;
        private AudioPoolManager _audioPoolManager;

        [Inject]
        private readonly ISubscriber<ShowLoadingSignal> _showLoadingSubscriber;

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
            _showLoadingSubscriber.Subscribe(OnShowLoading).AddTo(_disposableBagBuilder);
        }

        private void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        private void GetReferences()
        {
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

        private IEnumerator DespawnLoadingCo(ShowLoadingSignal signal)
        {
            yield return new WaitForSeconds(signal.DespawnTime);
            transform.SetActive(false);
            signal.OnClose?.Invoke();
            OnShowLoading(new ShowLoadingSignal(isShow: false));
        }

        private void OnShowLoading(ShowLoadingSignal signal)
        {
            if (signal.IsShow)
                _gameStore.HideAllExcept(signal.HideModules);
            else
            {
                _gameStore.RestoreLastHideModules();
                signal.OnClose?.Invoke();
            }

            transform.SetActive(signal.IsShow);

            if (signal.IsShow && signal.DespawnTime != 0)
                _showCo = StartCoroutine(DespawnLoadingCo(signal));
            else if (_showCo != null) StopCoroutine(_showCo);
        }
    }
}
