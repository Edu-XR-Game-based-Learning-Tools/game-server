using Shared.Network;
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

        public static Vector3 ToVector3(this Vec3D vec3D)
        {
            return new Vector3(vec3D.x, vec3D.y, vec3D.z);
        }

        public static Vec3D ToVec3D(this Vector3 vector3)
        {
            return new Vec3D(vector3.x, vector3.y, vector3.z);
        }
    }
}
