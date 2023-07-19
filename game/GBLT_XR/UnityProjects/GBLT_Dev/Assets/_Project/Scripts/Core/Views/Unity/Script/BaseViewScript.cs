using Core.Business;

namespace Core.View
{
    public class BaseViewScript : IBaseScript
    {
        public class Factory
        {
            public Factory()
            {
            }

            public IBaseScript Create(string id, IBaseScript baseScript)
            {
                var script = new BaseViewScript(id, baseScript);
                return script;
            }
        }

        public const string BasePath = "Assets/_Project/Bundles/Prefabs/Views";

        protected readonly string _scriptId;
        protected readonly IBaseScript _baseScript;

        public BaseViewScript(
            string id,
            IBaseScript baseScript)
        {
            _scriptId = id;
            _baseScript = baseScript;
        }

        public virtual BaseViewConfig GetConfig()
        {
            return _baseScript.GetConfig();
        }
    }
}
