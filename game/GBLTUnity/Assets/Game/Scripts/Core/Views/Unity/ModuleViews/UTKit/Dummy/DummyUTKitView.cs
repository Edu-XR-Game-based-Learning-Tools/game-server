using Core.Business;
using Core.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Core.View
{
    [RequireComponent(typeof(UIDocument))]
    public class DummyUTKitView : UnityView
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

        private void OnClickBackBtn()
        {
            _gameStore.GState.RemoveModel<DummyUTKitModel>();
        }

        private void Awake()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;

            _backBtn = root.Q<Button>("BackBtn");
        }

        private void OnEnable()
        {
            _backBtn.clicked += OnClickBackBtn;
        }

        private void OnDisable()
        {
            _backBtn.clicked -= OnClickBackBtn;
        }

        public override void OnReady()
        {
        }

        public void Refresh()
        {
        }
    }
}