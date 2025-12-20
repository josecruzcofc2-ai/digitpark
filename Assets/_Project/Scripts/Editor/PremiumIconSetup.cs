using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Configura el icono de corona para el boton Premium
    /// </summary>
    public class PremiumIconSetup : EditorWindow
    {
        // Color dorado neon
        private static readonly Color NEON_GOLD = new Color(1f, 0.84f, 0f, 1f); // #FFD700
        private static readonly Color NEON_GOLD_BRIGHT = new Color(1f, 0.9f, 0.3f, 1f); // Mas brillante

        [MenuItem("DigitPark/Setup Premium Crown Icon")]
        public static void SetupPremiumIcon()
        {
            // Verificar si estamos en MainMenu
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (!currentScene.path.Contains("MainMenu"))
            {
                if (!EditorUtility.DisplayDialog("Escena incorrecta",
                    "Necesitas estar en la escena MainMenu.\n\n¿Quieres abrir MainMenu ahora?",
                    "Si, abrir MainMenu", "Cancelar"))
                {
                    return;
                }

                EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity", OpenSceneMode.Single);
            }

            // Buscar el icono corona
            Sprite coronaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/corona.png");

            if (coronaSprite == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "No se encontro el sprite 'corona.png' en:\nAssets/_Project/Art/Icons/\n\nAsegurate de que el archivo exista.",
                    "OK");
                return;
            }

            // Buscar el objeto Icon dentro de PremiumButton
            GameObject iconObj = FindIconObject();

            if (iconObj == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "No se encontro el objeto 'Icon' dentro de PremiumButton.\n\nVerifica la jerarquia:\nPremiumButton > Content > Icon",
                    "OK");
                return;
            }

            // Remover TextMeshProUGUI si existe (conflicto con Image)
            var tmpText = iconObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                Object.DestroyImmediate(tmpText);
                Debug.Log("[PremiumIconSetup] Removed TextMeshProUGUI from Icon");
            }

            // Remover Text legacy si existe
            var legacyText = iconObj.GetComponent<UnityEngine.UI.Text>();
            if (legacyText != null)
            {
                Object.DestroyImmediate(legacyText);
                Debug.Log("[PremiumIconSetup] Removed legacy Text from Icon");
            }

            // Remover ThemeApplier - el icono tendra color FIJO dorado
            RemoveThemeApplier(iconObj);

            // Configurar el Image component
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = iconObj.AddComponent<Image>();
            }

            // Asignar sprite y color dorado neon
            iconImage.sprite = coronaSprite;
            iconImage.color = NEON_GOLD;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            // Marcar como modificado
            EditorUtility.SetDirty(iconImage);
            EditorUtility.SetDirty(iconObj);

            // Guardar escena
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            EditorUtility.DisplayDialog("Completado",
                "Icono de corona configurado!\n\n" +
                "- Sprite: corona.png\n" +
                "- Color: Dorado Neon (#FFD700)\n\n" +
                "La escena ha sido guardada.",
                "OK");

            Debug.Log("[PremiumIconSetup] Corona icon applied with neon gold color");
        }

        [MenuItem("DigitPark/Set Premium Icon Color/Gold Neon")]
        public static void SetGoldNeon()
        {
            SetIconColor(NEON_GOLD);
        }

        [MenuItem("DigitPark/Set Premium Icon Color/Bright Gold")]
        public static void SetBrightGold()
        {
            SetIconColor(NEON_GOLD_BRIGHT);
        }

        [MenuItem("DigitPark/Set Premium Icon Color/Cyan (Theme)")]
        public static void SetCyan()
        {
            SetIconColor(new Color(0f, 1f, 1f, 1f)); // Cyan
        }

        [MenuItem("DigitPark/Set Premium Icon Color/White")]
        public static void SetWhite()
        {
            SetIconColor(Color.white);
        }

        private static void SetIconColor(Color color)
        {
            GameObject iconObj = FindIconObject();

            if (iconObj == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "No se encontro el objeto Icon.\n\nAsegurate de estar en la escena MainMenu.",
                    "OK");
                return;
            }

            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null)
            {
                Undo.RecordObject(iconImage, "Change Icon Color");
                iconImage.color = color;
                EditorUtility.SetDirty(iconImage);
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();

                string hexColor = ColorUtility.ToHtmlStringRGB(color);
                EditorUtility.DisplayDialog("Color Cambiado",
                    $"Color del icono actualizado a:\n#{hexColor}\n\nLa escena ha sido guardada.",
                    "OK");

                Debug.Log($"[PremiumIconSetup] Icon color changed to #{hexColor}");
            }
            else
            {
                EditorUtility.DisplayDialog("Error",
                    "El objeto Icon no tiene componente Image.",
                    "OK");
            }
        }

        private static void RemoveThemeApplier(GameObject obj)
        {
            // Buscar y remover ThemeApplier para que el color sea FIJO
            System.Type themeApplierType = System.Type.GetType("DigitPark.Themes.ThemeApplier, Assembly-CSharp");

            if (themeApplierType != null)
            {
                var themeApplier = obj.GetComponent(themeApplierType);
                if (themeApplier != null)
                {
                    Object.DestroyImmediate(themeApplier);
                    Debug.Log($"[PremiumIconSetup] ThemeApplier removido de {obj.name} - color sera FIJO");
                }
            }
        }

        private static GameObject FindIconObject()
        {
            // Buscar PremiumButton > Content > Icon
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.name == "Icon" && obj.scene.IsValid())
                {
                    // Verificar que sea hijo de Content y nieto de PremiumButton
                    Transform parent = obj.transform.parent;
                    if (parent != null && parent.name == "Content")
                    {
                        Transform grandParent = parent.parent;
                        if (grandParent != null && grandParent.name == "PremiumButton")
                        {
                            return obj;
                        }
                    }
                }
            }

            return null;
        }
    }
}
