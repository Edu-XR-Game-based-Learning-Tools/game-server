using Core.Business;
using Zenject;

namespace Core.View
{
    public abstract class UnityBaseScript : IBaseScript
    {
        public class Factory<TUnityScript> : PlaceholderFactory<string, IBaseScript, TUnityScript>
        { }

        protected readonly string _scriptId;
        protected readonly IBaseScript _baseScript;

        protected readonly ILogger _logger;

        public UnityBaseScript(
            string id,
            IBaseScript baseScript,
            ILogger logger)
        {
            _scriptId = id;
            _baseScript = baseScript;

            _logger = logger;
        }

        public abstract BaseViewConfig GetConfig();
    }
}