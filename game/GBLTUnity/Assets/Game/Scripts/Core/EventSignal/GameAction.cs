using Core.Business;

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
}