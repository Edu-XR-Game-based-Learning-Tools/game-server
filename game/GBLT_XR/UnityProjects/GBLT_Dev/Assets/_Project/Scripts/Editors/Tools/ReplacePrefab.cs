#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class ReplacePrefab : EditorWindow
    {
        [SerializeField] private GameObject _prefab;

        [Header("Config")]
        [SerializeField] private bool _isChangeName;

        [MenuItem("Game Tools/Replace With Prefab")]
        static void CreateReplaceWithPrefab()
        {
            EditorWindow.GetWindow<ReplacePrefab>();
        }

        private void OnGUI()
        {
            _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);
            _isChangeName = EditorGUILayout.Toggle("Is Change Name", _isChangeName);

            if (GUILayout.Button("Replace"))
            {
                var selection = Selection.gameObjects;

                for (var i = selection.Length - 1; i >= 0; --i)
                {
                    var selected = selection[i];
                    var prefabType = PrefabUtility.GetPrefabType(_prefab);
                    GameObject newObject;

                    if (prefabType == PrefabType.Prefab)
                    {
                        newObject = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
                    }
                    else
                    {
                        newObject = Instantiate(_prefab);
                        if (_isChangeName) newObject.name = _prefab.name;
                    }

                    if (newObject == null)
                    {
                        Debug.LogError("Error instantiating prefab");
                        break;
                    }

                    Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
                    newObject.transform.parent = selected.transform.parent;
                    newObject.transform.localPosition = selected.transform.localPosition;
                    newObject.transform.localRotation = selected.transform.localRotation;
                    newObject.transform.localScale = selected.transform.localScale;
                    if (_isChangeName) newObject.transform.name = selected.transform.name;
                    newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                    Undo.DestroyObjectImmediate(selected);
                }
            }

            GUI.enabled = false;
            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
#endif
