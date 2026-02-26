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
        // Nombres prohibidos - nunca deben seleccionarse como Canvas principal
        private static readonly string[] BLOCKED_NAMES = {
            "TransitionCanvas", "EffectsCanvas"
        };

        // Ancestros prohibidos - Canvas dentro de estos contenedores se ignoran
        private static readonly string[] BLOCKED_ANCESTORS = {
            "---ANIMATION_MANAGERS---", "ANIMATION_MANAGERS",
            "UIAnimationManager", "SceneTransitionManager", "ParticleEffectSpawner"
        };

        /// <summary>
        /// Busca el Canvas principal de la escena.
        /// Prioriza un Canvas raíz llamado "Canvas".
        /// Si no existe, retorna el primer Canvas raíz que NO sea Transition/Effects
        /// y que NO esté dentro del contenedor de animaciones.
        /// </summary>
        public static Canvas FindMainCanvas()
        {
            // Prioridad 1: Canvas raíz llamado exactamente "Canvas"
            foreach (var c in Object.FindObjectsOfType<Canvas>(true))
                if (c.transform.parent == null && c.gameObject.name == "Canvas")
                    return c;

            // Prioridad 2: Primer Canvas raíz que no sea bloqueado
            foreach (var c in Object.FindObjectsOfType<Canvas>(true))
                if (IsValidMainCanvas(c))
                    return c;

            return null;
        }

        /// <summary>
        /// Valida que un Canvas sea apto como Canvas principal.
        /// Retorna false si es un canvas de animación o está dentro del contenedor de animaciones.
        /// </summary>
        public static bool IsValidMainCanvas(Canvas c)
        {
            if (c == null) return false;

            string name = c.gameObject.name;

            // Rechazar por nombre
            foreach (var blocked in BLOCKED_NAMES)
                if (name == blocked || name.Contains("Transition") || name.Contains("Effects"))
                    return false;

            // Rechazar si tiene ancestro prohibido
            Transform current = c.transform;
            while (current != null)
            {
                string parentName = current.gameObject.name;
                foreach (var blocked in BLOCKED_ANCESTORS)
                    if (parentName.Contains(blocked))
                        return false;
                current = current.parent;
            }

            // Solo aceptar Canvas raíz (sin padre) que no esté en la lista negra
            return c.transform.parent == null;
        }
    }
}
