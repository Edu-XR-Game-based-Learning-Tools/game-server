using UnityEngine;

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
    }
}