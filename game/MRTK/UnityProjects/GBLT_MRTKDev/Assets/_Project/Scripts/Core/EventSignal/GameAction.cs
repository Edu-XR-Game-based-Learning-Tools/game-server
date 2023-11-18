using Core.Business;
using Shared.Network;

namespace Core.EventSignal
{
    public class GameActionSignal<TModel>
        where TModel : IModuleContextModel
    {
        public TModel NewModel { get; private set; }
        public GameAction Action { get; private set; }

        public GameActionSignal(GameAction action, TModel newModel)
        {
            if (newModel == null)
                throw new GameActionModelIsNull();

            Action = action;
            NewModel = newModel;
        }

        public class GameActionModelIsNull : System.Exception
        { }
    }

    public enum GameAction
    {
    }

    public class OnNetworkRetryExceedMaxRetriesSignal
    {
    }

    public class UserDataCachedSignal
    {
    }

    public class OnVirtualRoomTickSignal
    {
        public VirtualRoomTickData TickData { get; private set; }
        public SharingTickData SharingTickData { get; private set; }

        public OnVirtualRoomTickSignal(VirtualRoomTickData tickData = null, SharingTickData sharingTickData = null)
        {
            TickData = tickData;
            SharingTickData = sharingTickData;
        }
    }
}
