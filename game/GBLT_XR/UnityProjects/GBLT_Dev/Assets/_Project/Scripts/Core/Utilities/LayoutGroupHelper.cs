using UnityEngine;
using UnityEngine.UI;

namespace Core.Utility
{
    public static class LayoutGroupHelper
    {
        public static void UpdateLayoutGroup_Immediately(
            RectTransform layoutRoot,
            LayoutGroup group,
            ContentSizeFitter contentSize = null)
        {
            group.enabled = true;

            if (contentSize != null)
                contentSize.enabled = true;

            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);

            group.enabled = false;

            if (contentSize != null)
                contentSize.enabled = false;
        }
    }
}