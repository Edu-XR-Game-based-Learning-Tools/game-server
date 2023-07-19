using Core.Business;
using UnityEngine;

namespace Core.View
{
    public interface IViewLayerManager
    {
        Transform GetLayerRoot(LayerManager layer);
    }
}