using Shared.Network;
using System;
using UnityEngine;
using UnityEngine.UI;
using static Microsoft.MixedReality.Toolkit.UX.NonNativeFunctionKey;

namespace Core.Extension
{
    public static class ComponentExtension
    {
        public static T SetActive<T>(this T comp, bool active) where T : Component
        {
            comp.gameObject.SetActive(active);
            return comp;
        }

        public static bool ActiveSelf<T>(this T comp) where T : Component
        {
            return comp.gameObject.activeSelf;
        }

        public static Vector3 ToVector3(this Vec3D vec3D)
        {
            return new Vector3(vec3D.x, vec3D.y, vec3D.z);
        }

        public static Vec3D ToVec3D(this Vector3 vector3)
        {
            return new Vec3D(vector3.x, vector3.y, vector3.z);
        }

        public static Sprite TexToSprite(this Texture2D tex, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
            return newSprite;
        }

        public static Color HexToColor(this string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            Texture2D tex = new(rTex.width, rTex.height, TextureFormat.RGB24, false);
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            return tex;
        }

        public static void ScrollToThisItem(this RectTransform target, ScrollRect scrollRect, RectTransform contentPanel)
        {
            Canvas.ForceUpdateCanvases();

            contentPanel.anchoredPosition =
                    (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                    - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
        }

        public static void ChangeLayersRecursively(this Transform transform, string layerName)
        {
            transform.gameObject.layer = LayerMask.NameToLayer(layerName);

            foreach (Transform child in transform)
                ChangeLayersRecursively(child, layerName);
        }
    }
}
