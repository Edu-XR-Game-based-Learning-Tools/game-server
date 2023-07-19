using Core.Business;
using System;
using System.Linq;

namespace Core.EventSignal
{
    public class GameScreenChangeSignal
    {
        public ScreenName Current { get; private set; }
        public ScreenName Previous { get; private set; }

        public GameScreenChangeSignal(ScreenName screenName, ScreenName previousScreenName)
        {
            Current = screenName;
            Previous = previousScreenName;
        }
    }

    public class GameScreenForceChangeSignal
    {
        public ScreenName Current { get; private set; }
        public ScreenName Previous { get; private set; }

        public GameScreenForceChangeSignal(ScreenName screenName, ScreenName previousScreenName)
        {
            Current = screenName;
            Previous = previousScreenName;
        }
    }

    public class CheckDownloadSizeStatusSignal
    {
        public double TotalCapacity;

        public CheckDownloadSizeStatusSignal(double totalCapacity)
        {
            TotalCapacity = totalCapacity;
        }
    }

    public class UpdateLoadingProgressSignal
    {
        public float Progress;

        public UpdateLoadingProgressSignal(float progress)
        {
            Progress = progress;
        }
    }

    public class AddressableErrorSignal
    {
        public string ErrorContent;

        public AddressableErrorSignal(string errorContent)
        {
            ErrorContent = errorContent;
        }
    }

    public abstract class HideOtherModuleSignal
    {
        public ModuleName[] HideModules { get; private set; }

        public HideOtherModuleSignal(ModuleName[] hideModules)
        {
            HideModules = hideModules;
        }
    }

    public class ShowToastSignal : HideOtherModuleSignal
    {
        public bool IsShow { get; private set; }
        public float DespawnTime { get; private set; }
        public string Content { get; private set; }

        public bool IsNewCloseAction { get; private set; }
        public Action OnClose { get; private set; }

        public ShowToastSignal(bool isShow = true, float despawnTime = 2, string content = "", ModuleName[] hideModules = null) : base(hideModules)
        {
            IsShow = isShow;
            DespawnTime = despawnTime;
            Content = content;
        }

        public ShowToastSignal SetOnClose(bool isNewAction = false, Action onClose = null)
        {
            IsNewCloseAction = isNewAction;
            OnClose = onClose;
            return this;
        }
    }

    public class ShowLoadingSignal : HideOtherModuleSignal
    {
        public bool IsShow { get; private set; }
        public float DespawnTime { get; private set; }

        public bool IsNewCloseAction { get; private set; }
        public Action OnClose { get; private set; }

        public ShowLoadingSignal(bool isShow = true, float despawnTime = 0, ModuleName[] hideModules = null) : base(hideModules)
        {
            IsShow = isShow;
            DespawnTime = despawnTime;
        }

        public ShowLoadingSignal SetOnClose(bool isNewAction = false, Action onClose = null)
        {
            IsNewCloseAction = isNewAction;
            OnClose = onClose;
            return this;
        }
    }

    public class ShowPopupSignal : HideOtherModuleSignal
    {
        public bool IsShow { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public string YesContent { get; private set; }
        public string NoContent { get; private set; }
        public Action<string, string> YesAction { get; private set; }
        public Action<string, string> NoAction { get; private set; }

        public bool IsNewCloseAction { get; private set; }
        public Action OnClose { get; private set; }

        public bool[] IsShowInputs { get; private set; }
        public string[] InitialInputValues { get; private set; }
        public string[] InputPlaceholders { get; private set; }

        public ShowPopupSignal(bool isShow = true, string title = "", string content = "", string yesContent = "Yes", string noContent = "No", Action<string, string> yesAction = null, Action<string, string> noAction = null, ModuleName[] hideModules = null) : base(hideModules)
        {
            IsShow = isShow;
            Title = title;
            Content = content;
            YesContent = yesContent;
            NoContent = noContent;
            YesAction = yesAction;
            NoAction = noAction;
        }

        public ShowPopupSignal SetOnClose(bool isNewAction = false, Action onClose = null)
        {
            IsNewCloseAction = isNewAction;
            OnClose = onClose;
            return this;
        }

        public ShowPopupSignal SetInitialInput(bool[] isShowInputs = null, string[] placeholders = null, string[] values = null)
        {
            IsShowInputs = isShowInputs;
            InputPlaceholders = placeholders;
            InitialInputValues = values;
            return this;
        }
    }
}
