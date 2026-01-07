#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DigitPark.Editor
{
    /// <summary>
    /// Crea elementos UI para Profile: Dividers y Bottom Bars
    /// </summary>
    public class ProfileUIElementsCreator : EditorWindow
    {
        [MenuItem("DigitPark/UI/Create Profile Divider")]
        public static void CreateDivider()
        {
            // Buscar Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró Canvas en la escena", "OK");
                return;
            }

            // Crear Divider
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(canvas.transform, false);

            // Agregar Image
            Image img = divider.AddComponent<Image>();
            img.color = new Color32(0, 255, 255, 128); // Cyan semi-transparente
            img.raycastTarget = false;

            // Configurar RectTransform
            RectTransform rt = divider.GetComponent<RectTransform>();

            // Anchor stretch horizontal
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Tamaño y márgenes
            rt.sizeDelta = new Vector2(-80, 2); // -80 para 40px de margen cada lado
            rt.anchoredPosition = Vector2.zero;

            // Seleccionar el objeto creado
            Selection.activeGameObject = divider;

            EditorUtility.SetDirty(divider);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("[ProfileUI] Divider creado. Muévelo a la posición deseada.");

            EditorUtility.DisplayDialog("Divider Creado",
                "Divider creado en el Canvas.\n\n" +
                "Configuración:\n" +
                "- Height: 2px\n" +
                "- Márgenes: 40px cada lado\n" +
                "- Color: Cyan semi-transparente\n\n" +
                "Ahora muévelo a la posición Y deseada y duplica (Ctrl+D) para crear más.",
                "OK");
        }

        [MenuItem("DigitPark/UI/Create Bottom Bar for Selected Button")]
        public static void CreateBottomBar()
        {
            GameObject selected = Selection.activeGameObject;

            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Selecciona un botón primero.\n\n" +
                    "1. Selecciona FriendsButton, HistoryButton o ChallengeButton\n" +
                    "2. Ejecuta este comando de nuevo",
                    "OK");
                return;
            }

            // Verificar que tiene RectTransform
            RectTransform buttonRT = selected.GetComponent<RectTransform>();
            if (buttonRT == null)
            {
                EditorUtility.DisplayDialog("Error", "El objeto seleccionado no tiene RectTransform", "OK");
                return;
            }

            // Verificar si ya tiene bottom bar
            Transform existingBar = selected.transform.Find("BottomBar");
            if (existingBar != null)
            {
                EditorUtility.DisplayDialog("Ya existe",
                    "Este botón ya tiene un BottomBar.\n" +
                    "Elimínalo primero si quieres crear uno nuevo.",
                    "OK");
                return;
            }

            // Crear BottomBar como hijo del botón
            GameObject bottomBar = new GameObject("BottomBar");
            bottomBar.transform.SetParent(selected.transform, false);

            // Agregar Image
            Image img = bottomBar.AddComponent<Image>();

            // Color según el botón
            bool isChallenge = selected.name.ToLower().Contains("challenge") ||
                              selected.name.ToLower().Contains("retar");

            if (isChallenge)
            {
                img.color = new Color32(255, 69, 0, 255); // Naranja #FF4500
            }
            else
            {
                img.color = new Color32(0, 255, 255, 255); // Cyan #00FFFF
            }

            img.raycastTarget = false;

            // Configurar RectTransform
            RectTransform rt = bottomBar.GetComponent<RectTransform>();

            // Anchor en la parte inferior del botón
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 1f);

            // Tamaño: 80% del ancho del botón, 4px de alto
            float barWidth = buttonRT.sizeDelta.x * 0.8f;
            if (barWidth < 100) barWidth = 200; // Mínimo 200px

            rt.sizeDelta = new Vector2(barWidth, 4);
            rt.anchoredPosition = new Vector2(0, -8); // 8px debajo del botón

            // Seleccionar el bar creado
            Selection.activeGameObject = bottomBar;

            EditorUtility.SetDirty(bottomBar);
            EditorUtility.SetDirty(selected);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string colorName = isChallenge ? "Naranja (#FF4500)" : "Cyan (#00FFFF)";

            Debug.Log($"[ProfileUI] BottomBar creado para {selected.name}");

            EditorUtility.DisplayDialog("Bottom Bar Creado",
                $"Bottom Bar creado para: {selected.name}\n\n" +
                $"Configuración:\n" +
                $"- Width: {barWidth}px (80% del botón)\n" +
                $"- Height: 4px\n" +
                $"- Color: {colorName}\n" +
                $"- Posición: 8px debajo del botón",
                "OK");
        }

        [MenuItem("DigitPark/UI/Create All Bottom Bars (Profile)")]
        public static void CreateAllBottomBars()
        {
            string[] buttonNames = { "FriendsButton", "HistoryButton", "ChallengeButton" };
            int created = 0;

            foreach (string btnName in buttonNames)
            {
                // Buscar el botón
                GameObject btn = GameObject.Find(btnName);
                if (btn == null)
                {
                    // Buscar con otros nombres posibles
                    var allButtons = Object.FindObjectsOfType<Button>(true);
                    foreach (var b in allButtons)
                    {
                        if (b.gameObject.name.ToLower().Contains(btnName.ToLower().Replace("button", "")))
                        {
                            btn = b.gameObject;
                            break;
                        }
                    }
                }

                if (btn == null)
                {
                    Debug.LogWarning($"[ProfileUI] No se encontró: {btnName}");
                    continue;
                }

                // Verificar si ya tiene bottom bar
                if (btn.transform.Find("BottomBar") != null)
                {
                    Debug.Log($"[ProfileUI] {btnName} ya tiene BottomBar, saltando...");
                    continue;
                }

                // Crear bottom bar
                Selection.activeGameObject = btn;
                CreateBottomBarForButton(btn);
                created++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Bottom Bars Creados",
                $"Se crearon {created} Bottom Bars.\n\n" +
                "Botones procesados:\n" +
                "- FriendsButton (cyan)\n" +
                "- HistoryButton (cyan)\n" +
                "- ChallengeButton (naranja)",
                "OK");
        }

        private static void CreateBottomBarForButton(GameObject button)
        {
            RectTransform buttonRT = button.GetComponent<RectTransform>();
            if (buttonRT == null) return;

            GameObject bottomBar = new GameObject("BottomBar");
            bottomBar.transform.SetParent(button.transform, false);

            Image img = bottomBar.AddComponent<Image>();

            bool isChallenge = button.name.ToLower().Contains("challenge") ||
                              button.name.ToLower().Contains("retar");

            img.color = isChallenge
                ? new Color32(255, 69, 0, 255)   // Naranja
                : new Color32(0, 255, 255, 255); // Cyan

            img.raycastTarget = false;

            RectTransform rt = bottomBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 1f);

            float barWidth = buttonRT.sizeDelta.x * 0.8f;
            if (barWidth < 100) barWidth = 200;

            rt.sizeDelta = new Vector2(barWidth, 4);
            rt.anchoredPosition = new Vector2(0, -8);

            EditorUtility.SetDirty(bottomBar);
            EditorUtility.SetDirty(button);

            Debug.Log($"[ProfileUI] BottomBar creado para {button.name}");
        }
    }
}
#endif
