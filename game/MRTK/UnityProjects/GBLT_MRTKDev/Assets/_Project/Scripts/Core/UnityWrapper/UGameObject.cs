using Shared.Network;
using System;
using UnityEngine;

namespace Core.Framework
{
    [Serializable]
    public class UGameObject : IGameObject
    {
        public GameObject WrappedObj { get; set; }

        public string Name
        {
            get
            {
                return WrappedObj.name;
            }
            set
            {
                WrappedObj.name = value;
            }
        }

        public Vec3D Position
        {
            get
            {
                Vector3 v = WrappedObj.transform.position;
                return new Vec3D(v.x, v.y, v.z);
            }
            set
            {
                WrappedObj.transform.position = new Vector3(value.x, value.y, value.z);
            }
        }

        public bool IsActive
        {
            get
            {
                return WrappedObj.activeSelf;
            }
        }

        public Vec3D EulerAngles
        {
            get
            {
                Vector3 v = WrappedObj.transform.eulerAngles;
                return new Vec3D(v.x, v.y, v.z);
            }
            set
            {
                WrappedObj.transform.eulerAngles = new Vector3(value.x, value.y, value.z);
            }
        }

        public UGameObject(GameObject obj)
        {
            WrappedObj = obj;
        }

        public void SetActive(bool value)
        {
            WrappedObj.SetActive(value);
        }

        public void SetParent(Transform parent)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying || !Application.isPlaying)
                return;
#endif
            if (this != null && WrappedObj != null && WrappedObj.transform != null)
                WrappedObj.transform.SetParent(parent);
        }
    }
}
