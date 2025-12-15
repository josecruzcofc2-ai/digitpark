using UnityEngine;

namespace DigitPark.Themes
{
    /// <summary>
    /// Inicializador del sistema de temas
    /// Añadir a la escena Boot para inicializar los temas al inicio de la app
    /// Se auto-crea si no existe
    /// </summary>
    public class ThemeInitializer : MonoBehaviour
    {
        private static bool isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (isInitialized) return;

            // Crear el ThemeManager si no existe
            if (ThemeManager.Instance == null)
            {
                GameObject managerObj = new GameObject("[ThemeManager]");
                managerObj.AddComponent<ThemeManager>();
                DontDestroyOnLoad(managerObj);
                Debug.Log("[ThemeInitializer] ThemeManager creado automáticamente");
            }

            isInitialized = true;
        }

        private void Awake()
        {
            AutoInitialize();
        }

        /// <summary>
        /// Fuerza la reinicialización del sistema de temas
        /// </summary>
        public static void ForceReinitialize()
        {
            isInitialized = false;
            AutoInitialize();
        }
    }
}
