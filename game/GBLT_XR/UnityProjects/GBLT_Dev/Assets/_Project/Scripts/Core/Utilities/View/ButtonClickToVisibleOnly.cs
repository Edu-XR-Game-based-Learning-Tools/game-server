using UnityEngine;
using UnityEngine.UI;

namespace Core.Utility
{
    public class ButtonClickToVisibleOnly : MonoBehaviour
    {
        [Range(0, 1)][SerializeField] private float alphaTreshold = 0.5f;

        private void Start()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = alphaTreshold;
        }
    }
}