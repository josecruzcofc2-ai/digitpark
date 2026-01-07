#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Ajusta tamaños de Inputs y Buttons en Login, Register y MainMenu
    /// NO modifica BackButtons ni el botón Pro
    /// </summary>
    public class InputButtonSizeAdjuster : EditorWindow
    {
        // Tamaños para Inputs
        private const float INPUT_WIDTH = 880f;
        private const float INPUT_HEIGHT = 115f;
        private const float INPUT_FONT_SIZE = 40f;
        private const float INPUT_PLACEHOLDER_SIZE = 36f;

        // Tamaños para Buttons
        private const float BUTTON_WIDTH = 650f;
        private const float BUTTON_HEIGHT = 115f;
        private const float BUTTON_FONT_SIZE = 42f;

        [MenuItem("DigitPark/UI/Adjust Inputs & Buttons (Login, Register, MainMenu)")]
        public static void AdjustAll()
        {
            string[] scenes = new string[]
            {
                "Assets/_Project/Scenes/Login.unity",
                "Assets/_Project/Scenes/Register.unity",
                "Assets/_Project/Scenes/MainMenu.unity"
            };

            int totalInputs = 0;
            int totalButtons = 0;

            foreach (string scenePath in scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"[Adjuster] Procesando: {sceneName}");

                int inputs = AdjustInputs();
                int buttons = AdjustButtons(sceneName);

                totalInputs += inputs;
                totalButtons += buttons;

                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Adjuster] {sceneName}: {inputs} inputs, {buttons} buttons ajustados");
            }

            EditorUtility.DisplayDialog("Ajuste Completado",
                $"Inputs ajustados: {totalInputs}\n" +
                $"Buttons ajustados: {totalButtons}\n\n" +
                $"Tamaños aplicados:\n" +
                $"- Inputs: {INPUT_WIDTH}x{INPUT_HEIGHT}, fuente {INPUT_FONT_SIZE}px\n" +
                $"- Buttons: {BUTTON_WIDTH}x{BUTTON_HEIGHT}, fuente {BUTTON_FONT_SIZE}px",
                "OK");
        }

        private static int AdjustInputs()
        {
            int count = 0;
            var inputs = Object.FindObjectsOfType<TMP_InputField>(true);

            foreach (var input in inputs)
            {
                RectTransform rt = input.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Ajustar tamaño
                rt.sizeDelta = new Vector2(INPUT_WIDTH, INPUT_HEIGHT);

                // Ajustar texto del input
                if (input.textComponent != null)
                {
                    var tmpText = input.textComponent as TextMeshProUGUI;
                    if (tmpText != null)
                    {
                        tmpText.fontSize = INPUT_FONT_SIZE;
                        // CENTRAR VERTICALMENTE
                        tmpText.alignment = TextAlignmentOptions.MidlineLeft;
                        tmpText.verticalAlignment = VerticalAlignmentOptions.Middle;
                        EditorUtility.SetDirty(tmpText);
                    }
                }

                // Ajustar placeholder
                if (input.placeholder != null)
                {
                    var placeholder = input.placeholder as TextMeshProUGUI;
                    if (placeholder != null)
                    {
                        placeholder.fontSize = INPUT_PLACEHOLDER_SIZE;
                        // CENTRAR VERTICALMENTE
                        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
                        placeholder.verticalAlignment = VerticalAlignmentOptions.Middle;
                        EditorUtility.SetDirty(placeholder);
                    }
                }

                // Ajustar Text Area para que ocupe bien el espacio
                var textArea = input.transform.Find("Text Area");
                if (textArea != null)
                {
                    var textAreaRT = textArea.GetComponent<RectTransform>();
                    if (textAreaRT != null)
                    {
                        // Padding interno: 24px izquierda/derecha, centrado vertical
                        textAreaRT.anchorMin = Vector2.zero;
                        textAreaRT.anchorMax = Vector2.one;
                        textAreaRT.offsetMin = new Vector2(24, 0);  // left, bottom
                        textAreaRT.offsetMax = new Vector2(-24, 0); // right, top
                        EditorUtility.SetDirty(textAreaRT);
                    }
                }

                // Buscar placeholders personalizados (como PasswordPlaceholder)
                // que no son el placeholder estándar del TMP_InputField
                foreach (Transform child in input.transform)
                {
                    string childName = child.gameObject.name.ToLower();
                    if (childName.Contains("placeholder") && child.gameObject != input.placeholder?.gameObject)
                    {
                        var customPlaceholder = child.GetComponent<TextMeshProUGUI>();
                        if (customPlaceholder != null)
                        {
                            customPlaceholder.fontSize = INPUT_PLACEHOLDER_SIZE;
                            customPlaceholder.alignment = TextAlignmentOptions.MidlineLeft;
                            customPlaceholder.verticalAlignment = VerticalAlignmentOptions.Middle;
                            EditorUtility.SetDirty(customPlaceholder);
                            Debug.Log($"    [CustomPlaceholder] {child.gameObject.name} centrado");
                        }
                    }
                }

                EditorUtility.SetDirty(input.gameObject);
                count++;
                Debug.Log($"  [Input] {input.gameObject.name}: {INPUT_WIDTH}x{INPUT_HEIGHT} (texto centrado)");
            }

            return count;
        }

        private static int AdjustButtons(string sceneName)
        {
            int count = 0;
            var buttons = Object.FindObjectsOfType<Button>(true);

            foreach (var btn in buttons)
            {
                string name = btn.gameObject.name.ToLower();

                // EXCLUIR BackButtons
                if (name.Contains("back") || name.Contains("arrow") || name.Contains("return"))
                {
                    Debug.Log($"  [Skip] {btn.gameObject.name} (BackButton)");
                    continue;
                }

                // EXCLUIR Pro button en MainMenu
                if (sceneName == "MainMenu" && name.Contains("pro"))
                {
                    Debug.Log($"  [Skip] {btn.gameObject.name} (Pro button)");
                    continue;
                }

                // En MainMenu, solo ajustar UserButton y SearchButton
                if (sceneName == "MainMenu")
                {
                    if (!name.Contains("user") && !name.Contains("search"))
                    {
                        Debug.Log($"  [Skip] {btn.gameObject.name} (MainMenu - no es user/search)");
                        continue;
                    }
                }

                RectTransform rt = btn.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Ajustar tamaño del botón
                rt.sizeDelta = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);

                // Ajustar texto del botón
                var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.fontSize = BUTTON_FONT_SIZE;
                    EditorUtility.SetDirty(btnText);
                }

                EditorUtility.SetDirty(btn.gameObject);
                count++;
                Debug.Log($"  [Button] {btn.gameObject.name}: {BUTTON_WIDTH}x{BUTTON_HEIGHT}");
            }

            return count;
        }
    }
}
#endif
