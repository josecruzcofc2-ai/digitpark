using UnityEngine;

namespace DigitPark.UI
{
    /// <summary>
    /// Runtime utility to find the main UI Canvas,
    /// excluding TransitionCanvas, EffectsCanvas and animation containers.
    /// </summary>
    public static class UICanvasHelper
    {
        /// <summary>
        /// Finds the main UI Canvas at runtime.
        /// Prioritizes root Canvas named "Canvas", then any root Canvas
        /// that is not a transition/effects overlay.
        /// </summary>
        public static Canvas FindMainCanvas()
        {
            // Priority 1: root Canvas named "Canvas"
            foreach (var c in Object.FindObjectsOfType<Canvas>(true))
            {
                if (c.transform.parent == null && c.gameObject.name == "Canvas")
                    return c;
            }

            // Priority 2: any valid root Canvas
            foreach (var c in Object.FindObjectsOfType<Canvas>(true))
            {
                if (IsValidMainCanvas(c))
                    return c;
            }

            // Fallback: any Canvas at all
            return Object.FindObjectOfType<Canvas>();
        }

        private static bool IsValidMainCanvas(Canvas c)
        {
            if (c == null || c.transform.parent != null) return false;

            string name = c.gameObject.name;
            if (name.Contains("Transition") || name.Contains("Effects"))
                return false;

            return true;
        }
    }
}
