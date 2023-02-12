using Core.Business;

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
}