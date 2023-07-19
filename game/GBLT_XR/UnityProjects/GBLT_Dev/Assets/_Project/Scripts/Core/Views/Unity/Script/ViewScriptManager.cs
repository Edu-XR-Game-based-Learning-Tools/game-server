using Core.Business;
using Shared.Extension;
using System;
using System.Collections.Generic;

namespace Core.View
{
    public class ViewScriptManager
    {
        private readonly BaseViewScript.Factory _viewScriptFactory;

        private const string _unityViewNameSpace = "Core.View.";

        public ViewScriptManager(BaseViewScript.Factory viewScriptFactory)
        {
            _viewScriptFactory = viewScriptFactory;
        }

        private readonly Dictionary<string, IBaseScript> _createdScript = new();
        private readonly Dictionary<string, BaseViewScript> _scriptLibrary = new();

        private BaseViewScript CreateNewFromTextAsset(string scriptId, IBaseScript baseScript)
        {
            BaseViewScript unityScript = (BaseViewScript)_viewScriptFactory.Create(scriptId, baseScript);
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

        private BaseViewScript LoadAndCreateNewFromTextAsset(string scriptId)
        {
            if (_createdScript[scriptId] == null)
                LoadUnityScript(scriptId);

            return CreateNewFromTextAsset(scriptId, _createdScript[scriptId]);
        }

        private BaseViewScript CreateNewScript(string scriptId)
        {
            _scriptLibrary.Add(scriptId, null);
            if (!_createdScript.ContainsKey(scriptId))
                _createdScript.Add(scriptId, null);

            BaseViewScript unityScript = LoadAndCreateNewFromTextAsset(scriptId);
            _scriptLibrary[scriptId] = unityScript;
            return unityScript;
        }

        #endregion Create New

        public BaseViewScript GetScript(string scriptId)
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
                UnityEngine.Debug.LogWarning($"Unity Attempting to clear not loaded script [{scriptId}]");
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
