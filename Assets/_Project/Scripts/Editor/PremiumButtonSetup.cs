using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Crea el boton Pro para acceder al panel de compras premium
    /// </summary>
    public class PremiumButtonSetup
    {
        private static readonly Color NeonGold = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color NeonCyan = new Color(0f, 0.9608f, 1f, 1f);

        [MenuItem("DigitPark/Create Premium Button in Current Scene")]
        public static void CreatePremiumButton()
        {
            // Buscar el Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontro un Canvas en la escena.", "OK");
                return;
            }

            // Verificar si ya existe
            var existing = GameObject.Find("PremiumButton");
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Info", "Ya existe un PremiumButton en la escena.", "OK");
                Selection.activeGameObject = existing;
                return;
            }

            // Crear el boton
            GameObject premiumButton = CreatePremiumButtonStructure(canvas.transform);

            // Seleccionar el objeto creado
            Selection.activeGameObject = premiumButton;
            EditorGUIUtility.PingObject(premiumButton);

            Undo.RegisterCreatedObjectUndo(premiumButton, "Create Premium Button");

            EditorUtility.DisplayDialog("Exito",
                "PremiumButton creado.\n\n" +
                "Ahora debes conectarlo al SettingsManager o MainMenuManager\n" +
                "arrastrando el boton al campo correspondiente.",
                "OK");
        }

        private static GameObject CreatePremiumButtonStructure(Transform parent)
        {
            // === PREMIUM BUTTON ===
            GameObject btnObj = new GameObject("PremiumButton");
            btnObj.transform.SetParent(parent, false);
            btnObj.layer = 5; // UI layer

            // RectTransform
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(180, 60);

            // Image (fondo del boton)
            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.15f, 0.25f, 1f); // Fondo oscuro
            bg.raycastTarget = true;

            // Outline dorado
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = NeonGold;
            outline.effectDistance = new Vector2(2, 2);

            // Button component
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            colors.disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            // === CONTENIDO ===
            GameObject content = new GameObject("Content");
            content.transform.SetParent(btnObj.transform, false);

            RectTransform contentRt = content.AddComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = new Vector2(8, 5);
            contentRt.offsetMax = new Vector2(-8, -5);

            // Horizontal Layout
            HorizontalLayoutGroup hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 5;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // === ICONO CORONA (igual que MainMenu) ===
            GameObject iconObj = new GameObject("CrownIcon");
            iconObj.transform.SetParent(content.transform, false);

            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(60, 40);

            LayoutElement iconLe = iconObj.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 30;
            iconLe.preferredHeight = 30;

            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = NeonGold;
            iconImg.preserveAspect = true;

            // Buscar el sprite de corona
            string[] guids = AssetDatabase.FindAssets("corona t:Sprite");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    iconImg.sprite = sprite;
                }
            }

            // === TEXTO "PRO" (igual que MainMenu) ===
            GameObject textObj = new GameObject("PremiumButtonText");
            textObj.transform.SetParent(content.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(100, 30);

            LayoutElement textLe = textObj.AddComponent<LayoutElement>();
            textLe.preferredWidth = 100;
            textLe.preferredHeight = 30;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "PRO";
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = NeonGold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            // === PREMIUM BADGE (oculto por defecto) ===
            GameObject badge = new GameObject("PremiumBadge");
            badge.transform.SetParent(btnObj.transform, false);
            badge.SetActive(false);

            RectTransform badgeRt = badge.AddComponent<RectTransform>();
            badgeRt.anchorMin = new Vector2(1, 1);
            badgeRt.anchorMax = new Vector2(1, 1);
            badgeRt.pivot = new Vector2(0.5f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(5, 5);
            badgeRt.sizeDelta = new Vector2(24, 24);

            Image badgeImg = badge.AddComponent<Image>();
            badgeImg.color = new Color(0.2f, 1f, 0.4f, 1f); // Verde neon

            GameObject badgeText = new GameObject("BadgeText");
            badgeText.transform.SetParent(badge.transform, false);

            RectTransform badgeTextRt = badgeText.AddComponent<RectTransform>();
            badgeTextRt.anchorMin = Vector2.zero;
            badgeTextRt.anchorMax = Vector2.one;
            badgeTextRt.offsetMin = Vector2.zero;
            badgeTextRt.offsetMax = Vector2.zero;

            TextMeshProUGUI badgeTmp = badgeText.AddComponent<TextMeshProUGUI>();
            badgeTmp.text = "";
            badgeTmp.fontSize = 12;
            badgeTmp.fontStyle = FontStyles.Bold;
            badgeTmp.color = Color.white;
            badgeTmp.alignment = TextAlignmentOptions.Center;

            return btnObj;
        }
    }
}
