using Core.Business;
using System.Collections.Generic;
using UnityEngine;

namespace Core.View
{
    public class UTKitViewLayerManager : MonoBehaviour, IViewLayerManager
    {
        [SerializeField]
        private List<Transform> _layers = new();

        public Transform GetLayerRoot(LayerManager layer)
        {
            if (layer == LayerManager.None)
                return transform;
            return _layers[(int)layer];
        }
    }
}