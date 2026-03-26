using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DigitPark.UI;
using DigitPark.UI.Items;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor script para crear prefabs de items de Monetizacion
    /// Incluye: Onboarding, Social (V58)
    /// Resolucion: Portrait 9:16 (1080x1920)
    /// </summary>
    public class MonetizationPrefabBuilder : EditorWindow
    {
        // Colores del tema neon
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color CYAN_DARK = new Color(0f, 0.4f, 0.5f, 1f);
        private static readonly Color DARK_BG = new Color(0.08f, 0.12f, 0.18f, 0.98f);
        private static readonly Color CARD_BG = new Color(0.06f, 0.08f, 0.12f, 1f);
        private static readonly Color GOLD = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color GREEN = new Color(0.3f, 0.9f, 0.4f, 1f);
        private static readonly Color ORANGE = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color PURPLE = new Color(0.7f, 0.3f, 0.9f, 1f);
        private static readonly Color RED = new Color(0.9f, 0.3f, 0.3f, 1f);
        private static readonly Color PREMIUM_GOLD = new Color(1f, 0.7f, 0.2f, 1f);
        private static readonly Color PROGRESS_BG = new Color(0.1f, 0.12f, 0.15f, 1f);
        private static readonly Color PROGRESS_FILL = new Color(0f, 0.8f, 0.4f, 1f);
        private static readonly Color LOCKED_BG = new Color(0.1f, 0.1f, 0.12f, 0.9f);
        private static readonly Color COMPLETED_BG = new Color(0.1f, 0.25f, 0.15f, 0.95f);

        // Dimensiones
        private const float STEP_DOT_SIZE = 12f;
        private const float LEADERBOARD_ENTRY_H = 70f;

        [MenuItem("DigitPark/Prefabs/Monetization/Create All")]
        private static void CreateAllFromMenu()
        {
            CreateAllPrefabs();
        }

        [MenuItem("DigitPark/Prefabs/Monetization/Open Builder Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<MonetizationPrefabBuilder>("Monetization Prefabs");
            window.minSize = new Vector2(300, 500);
        }

        private void OnGUI()
        {
            GUILayout.Label("Monetization Prefab Builder", EditorStyles.boldLabel);
            GUILayout.Label("Resolucion: Portrait 9:16 (1080x1920)", EditorStyles.miniLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("CREAR TODOS LOS PREFABS", GUILayout.Height(40)))
                CreateAllPrefabs();

            GUILayout.Space(10);
            GUILayout.Label("O crear individualmente:", EditorStyles.miniBoldLabel);
            GUILayout.Space(5);

            if (GUILayout.Button("StepDotItem")) CreateStepDotItemPrefab();
            GUILayout.Space(5);
            GUILayout.Label("Social:", EditorStyles.miniBoldLabel);
            if (GUILayout.Button("LeaderboardEntry")) CreateLeaderboardEntryPrefab();
            GUILayout.Space(5);
        }

        private static void CreateAllPrefabs()
        {
            CreateStepDotItemPrefab();
            CreateLeaderboardEntryPrefab();

            Debug.Log("[MonetizationPrefabBuilder] Todos los prefabs creados!");
            EditorUtility.DisplayDialog("Completado", "Todos los prefabs han sido creados exitosamente!", "OK");
        }

        #region Onboarding - Step Dot Item Prefab

        [MenuItem("DigitPark/Prefabs/Monetization/StepDotItem")]
        private static void CreateStepDotItemPrefab()
        {
            GameObject item = new GameObject("StepDotItem");

            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(STEP_DOT_SIZE, STEP_DOT_SIZE);

            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredWidth = STEP_DOT_SIZE;
            le.preferredHeight = STEP_DOT_SIZE;

            // Dot Image
            Image dotImg = item.AddComponent<Image>();
            dotImg.color = new Color(0.3f, 0.3f, 0.35f);

            // Pulse Image (for active state)
            GameObject pulse = CreateImageElement(item.transform, "PulseImage",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-10, -10), new Vector2(10, 10));
            pulse.GetComponent<Image>().color = new Color(CYAN_NEON.r, CYAN_NEON.g, CYAN_NEON.b, 0.3f);
            pulse.SetActive(false);

            // Add UI Component
            StepDotItemUI ui = item.AddComponent<StepDotItemUI>();

            SavePrefab(item, "Assets/_Project/Prefabs/Onboarding/StepDotItem.prefab");
        }

        #endregion

        // Avatar Option Item Prefab removed — avatar system eliminated
        // Player Search Item Prefab removed — Friends system eliminated

        #region Leaderboard Entry Prefab

        [MenuItem("DigitPark/Prefabs/Social/LeaderboardEntry")]
        public static void CreateLeaderboardEntryPrefab()
        {
            GameObject item = new GameObject("LeaderboardEntry");
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, LEADERBOARD_ENTRY_H);
            LayoutElement le = item.AddComponent<LayoutElement>();
            le.preferredHeight = LEADERBOARD_ENTRY_H;
            le.flexibleWidth = 1;
            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.08f, 0.13f, 0.95f);
            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Medal glow strip (left edge, top-3 indicator)
            GameObject medal = CreateImageElement(item.transform, "MedalIndicator",
                new Vector2(0, 0.1f), new Vector2(0, 0.9f),
                new Vector2(2, 0), new Vector2(6, 0));
            medal.GetComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.4f);

            // Position
            CreateTextElement(item.transform, "PositionText", "1",
                new Vector2(0, 0.1f), new Vector2(0.13f, 0.9f),
                new Vector2(8, 0), new Vector2(0, 0),
                (int)FontSizes.Body, new Color(1f, 0.84f, 0f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Avatar placeholder
            GameObject avatar = CreateImageElement(item.transform, "AvatarImage",
                new Vector2(0.13f, 0.1f), new Vector2(0.13f, 0.9f),
                new Vector2(4, 2), new Vector2(58, -2));
            avatar.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f);
            avatar.GetComponent<Image>().raycastTarget = true;

            // Username
            CreateTextElement(item.transform, "UsernameText", "Player123",
                new Vector2(0.26f, 0.45f), new Vector2(0.7f, 0.95f),
                Vector2.zero, Vector2.zero,
                (int)FontSizes.Body, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Time / score
            CreateTextElement(item.transform, "TimeText", "1.234s",
                new Vector2(0.7f, 0.1f), new Vector2(1f, 0.9f),
                new Vector2(0, 0), new Vector2(-10, 0),
                (int)FontSizes.Body, new Color(0f, 1f, 0.53f), FontStyles.Bold, TextAlignmentOptions.Right);

            var comp = item.AddComponent<LeaderboardEntryUI>();
            comp.AutoSetupReferences();
            SavePrefab(item, "Assets/_Project/Prefabs/Social/LeaderboardEntry.prefab");
        }

        #endregion


        #region Helper Methods

        private static GameObject CreateContainer(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return obj;
        }

        private static GameObject CreateImageElement(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            Image img = obj.AddComponent<Image>();
            img.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateTextElement(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateTextElement(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = FontSizes.AutoMinBody;
            tmp.fontSizeMax = fontSize > 0 ? fontSize : FontSizes.Body;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Color bgColor, Color textColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = obj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            if (!string.IsNullOrEmpty(text))
            {
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(obj.transform, false);
                RectTransform textRt = textObj.AddComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = text;
                tmp.fontSize = (int)FontSizes.Body;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = FontSizes.AutoMinBody;
                tmp.fontSizeMax = tmp.fontSize;
                tmp.color = textColor;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }

            return obj;
        }

        private static GameObject CreateOverlay(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = obj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return obj;
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            Debug.Log($"[MonetizationPrefabBuilder] Prefab creado: {path}");
            Selection.activeObject = prefab;
        }

        private static Sprite GenerateCircleSprite()
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/UI/CircleSprite.png");
            if (s != null) return s;
            // Fallback: generate at runtime (won't survive prefab save)
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    if (dist <= radius) tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f) tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius + 1f - dist)));
                    else tex.SetPixel(x, y, Color.clear);
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        #endregion
    }
}
