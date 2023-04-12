#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomEditor(typeof(CanvasRenderer))]
    public class CanvasRendererEditor : UnityEditor.Editor
    {
        private CanvasRenderer _instance;
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            _instance = target as CanvasRenderer;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(15.0f);

            GUIStyle helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
            helpBoxStyle.fontSize = 11;
            helpBoxStyle.padding = new RectOffset(7, 7, 7, 7);
            helpBoxStyle.richText = true;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(100));

            EditorGUILayout.TextArea(
                "<b>NOTE!</b>" +
                "\n\n" +
                "- This is an extension for Canvas Renderer component of Unity. " +
                "\n\n" +
                "- Click on Optimize UI button will disable all the checklist below (If any): " +
                "\n\n" +
                "     + Canvas Renderer: " + "\n" +
                "          • Cull Transparent Mesh" + "\n\n" +
                "     + Text & TextMeshPro:" + "\n" +
                "          • Rich Text" + "\n" +
                "          • Best Fit/Auto Size" + "\n" +
                "          • Raycast Target" + "\n" +
                "          • Maskable" + "\n\n" +
                "     + Only TextMeshPro:" + "\n" +
                "          • Is Scale Static" + "\n" +
                "          • Parse Escape Characters" + "\n" +
                "          • Visible Descender" + "\n" +
                "          • Kerning" + "\n" +
                "          • Extra Padding" + "\n\n" +
                "     + Image & Raw Image: " + "\n" +
                "          • Raycast Targer" + "\n" +
                "          • Maskable",
                helpBoxStyle,
                GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();

            GUILayout.Space(15.0f);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.margin.right = 17;
            if (GUILayout.Button(new GUIContent("Optimize UI"), style, GUILayout.Height(22.0f), GUILayout.Width(230.0f)))
            {
                OptimizeUI();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(3.0f);
        }

        private void OptimizeUI()
        {
            _instance.cullTransparentMesh = false;

            OptimizeText();
            OptimizeTextMeshPro();
            OptimizeImage();
            OptimizeRawImage();
        }

        private void OptimizeText()
        {
            UnityEngine.UI.Text text = _instance.GetComponent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.supportRichText = false;
                text.resizeTextForBestFit = false;
                text.raycastTarget = false;
                text.maskable = false;
            }
        }

        private void OptimizeTextMeshPro()
        {
            TMPro.TextMeshProUGUI textPro = _instance.GetComponent<TMPro.TextMeshProUGUI>();
            if (textPro != null)
            {
                textPro.enableAutoSizing = false;
                textPro.isTextObjectScaleStatic = false;
                textPro.richText = false;
                textPro.raycastTarget = false;
                textPro.maskable = false;
                textPro.parseCtrlCharacters = false;
                textPro.useMaxVisibleDescender = false;
                textPro.enableKerning = false;
                textPro.extraPadding = false;
            }
        }

        private void OptimizeImage()
        {
            UnityEngine.UI.Image image = _instance.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.raycastTarget = false;
                image.maskable = false;
            }
        }

        private void OptimizeRawImage()
        {
            UnityEngine.UI.RawImage rawImage = _instance.GetComponent<UnityEngine.UI.RawImage>();
            if (rawImage != null)
            {
                rawImage.raycastTarget = false;
                rawImage.maskable = false;
            }
        }
    }
}
#endif
