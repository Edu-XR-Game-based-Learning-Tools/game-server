using Core.Business;
using Shared.Extension;
using System;
using System.Collections.Generic;

namespace Core.View
{
    public class UnityViewScriptManager<TUnityScript> where TUnityScript : UnityBaseScript
    {
        private readonly ILogger _logger;
        private readonly UnityBaseScript.Factory<TUnityScript> _unityScriptFactory;

        private const string _unityViewNameSpace = "Core.View.";

        public UnityViewScriptManager(
            ILogger logger,
            UnityBaseScript.Factory<TUnityScript> unityScriptFactory)
        {
            _logger = logger;
            _unityScriptFactory = unityScriptFactory;
        }

        private readonly Dictionary<string, IBaseScript> _createdScript = new Dictionary<string, IBaseScript>();
        private readonly Dictionary<string, TUnityScript> _scriptLibrary = new Dictionary<string, TUnityScript>();

        private TUnityScript CreateNewFromTextAsset(string scriptId, IBaseScript baseScript)
        {
            TUnityScript unityScript = _unityScriptFactory.Create(scriptId, baseScript);
            return unityScript;
        }

        #region Create New

        private void LoadUnityScript(string scriptId)
        {
            Type viewType = GetType();
            Type scriptType = Type.GetType($"{_unityViewNameSpace + scriptId}, {viewType.Assembly.GetName()}");
            IBaseScript baseScript = (IBaseScript)GeneralExtension.CreateInstance(scriptType);
            _createdScript[scriptId] = baseScript ?? throw new MissingUnityScriptId(scriptId);
        }

        private TUnityScript LoadAndCreateNewFromTextAsset(string scriptId)
        {
            if (_createdScript[scriptId] == null)
                LoadUnityScript(scriptId);

            return CreateNewFromTextAsset(scriptId, _createdScript[scriptId]);
        }

        private TUnityScript CreateNewScript(string scriptId)
        {
            _scriptLibrary.Add(scriptId, null);
            if (!_createdScript.ContainsKey(scriptId))
                _createdScript.Add(scriptId, null);

            TUnityScript unityScript = LoadAndCreateNewFromTextAsset(scriptId);
            _scriptLibrary[scriptId] = unityScript;
            return unityScript;
        }

        #endregion Create New

        public TUnityScript GetScript(string scriptId)
        {
            if (HasUnityScript(scriptId))
                return _scriptLibrary[scriptId];

            return CreateNewScript(scriptId);
        }

        private bool HasUnityScript(string scriptId)
        {
            if (string.IsNullOrEmpty(scriptId))
            {
                return false;
            }
            return _scriptLibrary.ContainsKey(scriptId);
        }

        public void ClearScript(string scriptId)
        {
            if (!HasUnityScript(scriptId))
            {
                _logger.Warning($"Unity Attempting to clear not loaded script [{scriptId}]");
                return;
            }
            _scriptLibrary.Remove(scriptId);
        }

        private class MissingUnityScriptId : Exception
        {
            public MissingUnityScriptId(string message) : base(message)
            {
            }
        }
    }
}