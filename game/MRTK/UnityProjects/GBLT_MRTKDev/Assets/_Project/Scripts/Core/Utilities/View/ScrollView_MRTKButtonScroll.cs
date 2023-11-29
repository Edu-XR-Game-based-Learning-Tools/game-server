using Core.Extension;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Utility
{
    public class ScrollView_MRTKButtonScroll : MonoBehaviour
    {
        [System.Serializable]
        public class ScrollView_Buttons
        {
            public PressableButton PreviousBtn;
            public PressableButton NextBtn;

            public void SetActive(bool isActive = true)
            {
                PreviousBtn?.SetActive(isActive);
                NextBtn?.SetActive(isActive);
            }
        }

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField][DebugOnly] private RectTransform _scrollRectTransform;
        [SerializeField][DebugOnly] private ScrollView_Buttons _verticalScrollBtns = new();
        [SerializeField][DebugOnly] private ScrollView_Buttons _horizontalScrollBtns = new();

        [SerializeField][DebugOnly] private RectTransform _content;
        [SerializeField] private float _scrollNormalizedValue = 0.1f;

        public void EnableButtonOnChangeContentSize()
        {
            _verticalScrollBtns.SetActive(_content.rect.height >= _scrollRectTransform.rect.height);
            _horizontalScrollBtns.SetActive(_content.rect.width >= _scrollRectTransform.rect.width);
        }

        private void Awake()
        {
            _scrollRect ??= GetComponent<ScrollRect>();
            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
            _content = _scrollRect.transform.Find("Viewport/Content").GetComponent<RectTransform>();

            var vertical = transform.Find("SV_VerticalAction");
            var horizontal = transform.Find("SV_HorizontalAction");

            if (vertical)
            {
                _verticalScrollBtns = new()
                {
                    PreviousBtn = vertical.Find("Previous_Btn").GetComponent<PressableButton>(),
                    NextBtn = vertical.Find("Next_Btn").GetComponent<PressableButton>()
                };
            }

            if (horizontal)
            {
                _horizontalScrollBtns = new()
                {
                    PreviousBtn = horizontal.Find("Previous_Btn").GetComponent<PressableButton>(),
                    NextBtn = horizontal.Find("Next_Btn").GetComponent<PressableButton>()
                };
            }

            RegisterEvents();
            EnableButtonOnChangeContentSize();
        }

        private void RegisterEvents()
        {
            if (_verticalScrollBtns.PreviousBtn != null)
            {
                _verticalScrollBtns.PreviousBtn.OnClicked.AddListener(() =>
                {
                    _scrollRect.verticalNormalizedPosition += _scrollNormalizedValue;
                });
                _verticalScrollBtns.NextBtn.OnClicked.AddListener(() =>
                {
                    _scrollRect.verticalNormalizedPosition -= _scrollNormalizedValue;
                });
            }

            if (_horizontalScrollBtns.PreviousBtn != null)
            {
                _horizontalScrollBtns.PreviousBtn.OnClicked.AddListener(() =>
                {
                    _scrollRect.horizontalNormalizedPosition -= _scrollNormalizedValue;
                });
                _horizontalScrollBtns.NextBtn.OnClicked.AddListener(() =>
                {
                    _scrollRect.horizontalNormalizedPosition += _scrollNormalizedValue;
                });
            }
        }

        private void Update()
        {
            EnableButtonOnChangeContentSize();
        }
    }
}
