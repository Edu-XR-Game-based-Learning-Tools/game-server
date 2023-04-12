using Core.Business;

namespace Core.View
{
    public class UnityViewScript : UnityBaseScript
    {
        public UnityViewScript(
            string id,
            IBaseScript baseScript,
            ILogger logger)
            : base(id, baseScript, logger)
        {
        }

        public override BaseViewConfig GetConfig()
        {
            return _baseScript.GetConfig();
        }
    }
}