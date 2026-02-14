using UnityEngine;
using UnityEngine.UI;

namespace DigitPark.Editor
{
    /// <summary>
    /// Utilidad compartida para que todos los UIBuilders encuentren el Canvas principal,
    /// evitando que se seleccione TransitionCanvas o EffectsCanvas por error.
    /// </summary>
    public static class UIBuilderCanvasHelper
    {
        /// <summary>
        /// Busca el Canvas principal de la escena.
        /// Prioriza un Canvas raíz llamado "Canvas".
        /// Si no existe, retorna el primer Canvas raíz que NO sea Transition/Effects.
        /// </summary>
        public static Canvas FindMainCanvas()
        {
            // Prioridad 1: Canvas raíz llamado exactamente "Canvas"
            foreach (var c in Object.FindObjectsOfType<Canvas>(true))
                if (c.transform.parent == null && c.gameObject.name == "Canvas")
                    return c;

            // Prioridad 2: Primer Canvas raíz que no sea Transition/Effects
            foreach (var c in Object.FindObjectsOfType<Canvas>(true))
                if (c.transform.parent == null &&
                    !c.gameObject.name.Contains("Transition") &&
                    !c.gameObject.name.Contains("Effects"))
                    return c;

            return null;
        }
    }
}
