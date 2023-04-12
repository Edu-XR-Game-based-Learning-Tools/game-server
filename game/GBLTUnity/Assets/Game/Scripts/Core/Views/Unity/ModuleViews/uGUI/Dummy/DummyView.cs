using Core.Business;
using Core.Framework;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Core.View
{
    public class DummyView : UnityView
    {
        private GameStore _gameStore;
        private SignalBus _signalBus;
        private AudioPoolManager _audioPoolManager;

        [SerializeField] private Button _backBtn;

        [Inject]
        public void Init(
            GameStore gameStore,
            SignalBus signalBus,
            AudioPoolManager audioPoolManager)
        {
            _gameStore = gameStore;
            _signalBus = signalBus;
            _audioPoolManager = audioPoolManager;
        }

        public override void OnReady()
        {
            _backBtn.onClick.AddListener(() =>
            {
                _gameStore.GState.RemoveModel<DummyModel>();
            });
        }

        public void Refresh()
        {
        }
    }
}