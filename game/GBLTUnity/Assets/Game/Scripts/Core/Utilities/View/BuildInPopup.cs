using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Utility
{
    public class BuildInPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _message;

        [SerializeField] private Button _closeBtn;

        private CanvasGroup _canvas;

        private void Awake()
        {
            _canvas = GetComponent<CanvasGroup>();
            _canvas.blocksRaycasts = false;
            _canvas.interactable = false;
            _canvas.alpha = 0;
        }

        public void Show(string message, System.Action closeAction)
        {
            _canvas.blocksRaycasts = true;
            _canvas.interactable = true;
            _closeBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.AddListener(() =>
            {
                if (closeAction != null)
                    closeAction();
                Hide();
            });
            _message.text = message;
            //_canvas.DOFade(1, 0.5f).Play();
        }

        public void Hide()
        {
            _closeBtn.onClick.RemoveAllListeners();
            //_canvas.DOFade(0, 0.25f).Play();
            _canvas.blocksRaycasts = false;
            _canvas.interactable = false;
        }
    }
}