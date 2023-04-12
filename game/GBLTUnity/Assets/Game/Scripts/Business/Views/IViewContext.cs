using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Core.Business
{
    public enum DefaultFunc
    {
        OnReady,
        GetConfig,
        Show,
        Hide,
        SetIndex,
    }

    public class BaseViewConfig
    {
        public string Bundle { get; set; }
        public UIType UIType { get; set; }
        public BaseViewConfig(string bundle, UIType uiType)
        {
            Bundle = bundle;
            UIType = uiType;
        }

        public string ViewId;
        public string Config;
        public LayerManager Layer = LayerManager.None;
        public AnchorPresets AnchorPreset = AnchorPresets.StretchAll;
        public Vector2 AnchorPos = Vector2.zero;
        public Vector2 SizeDelta = Vector2.zero;
        public bool KeepLayout;
    }

    public interface IViewContext
    {
        IBaseModule Module { get; set; }
        GameObject View { get; }

        UniTask TryCreateViewElement(IBaseModule module);

        void Call<T>(T function, params object[] args) where T : Enum;

        void Destroy();

        void Show();

        void Hide();

        void OnReady();

        void SetIndex(int index);
    }

    public interface IBaseScript
    {
        BaseViewConfig GetConfig();
    }
}