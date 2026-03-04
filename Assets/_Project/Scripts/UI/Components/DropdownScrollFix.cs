using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.UI
{
    /// <summary>
    /// Fixes TMP_Dropdown clipping when inside a ScrollView with RectMask2D.
    /// Reparents the "Dropdown List" to the root Canvas after TMP_Dropdown
    /// finishes positioning it, preserving world position.
    /// After reparenting, forces ScrollRect to recalculate content bounds.
    /// </summary>
    public class DropdownScrollFix : MonoBehaviour
    {
        private Canvas rootCanvas;

        private void Awake()
        {
            rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        }

        private void OnTransformChildrenChanged()
        {
            Transform popup = transform.Find("Dropdown List");
            if (popup != null && rootCanvas != null && popup.parent == transform)
            {
                StartCoroutine(ReparentAndFix(popup));
            }
        }

        private IEnumerator ReparentAndFix(Transform popup)
        {
            // Wait one frame for TMP_Dropdown to finish positioning
            yield return null;

            if (popup == null || rootCanvas == null) yield break;

            popup.SetParent(rootCanvas.transform, true); // worldPositionStays = true

            // Wait another frame for layout to settle after reparenting
            yield return null;

            if (popup == null) yield break;

            // Force ScrollRect to recalculate content bounds (stale after reparent)
            var scrollRect = popup.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.enabled = false;
                scrollRect.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(popup.GetComponent<RectTransform>());
            }
        }
    }
}
