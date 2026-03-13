using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor Window — CashBattle Gold Preview.
    /// Opens via: DigitPark / Themes / CashBattle Gold Preview
    ///
    /// PURPOSE:
    ///   Minigame scenes (DigitRush, FlashTap, MemoryPairs, OddOneOut, QuickMath) are
    ///   SHARED between normal gameplay and CashBattle. At runtime, when a session is
    ///   launched from CashBattle, CashThemeForcer applies a gold color overlay — no
    ///   duplicate scenes needed.
    ///
    ///   This tool lets you preview that gold overlay in the Editor, exactly as it will
    ///   look at runtime, without entering Play Mode.
    ///
    /// WORKS ONLY ON MINIGAME SCENES:
    ///   DigitRush · FlashTap · MemoryPairs · OddOneOut · QuickMath
    ///   Any other scene will show a block screen and refuse to apply.
    ///
    /// SYNC GUARANTEE:
    ///   Color constants and tolerance are verbatim copies from CashThemeForcer.cs.
    ///   The *** CASH SYNC REQUIREMENT *** comment marks the sync point.
    ///   If CashThemeForcer.cs changes its color values, update this file to match.
    ///
    /// NON-DESTRUCTIVE:
    ///   Original colors are stored before first apply.
    ///   "Restore Neon" brings every component back to its exact pre-apply color.
    ///   Ctrl+Z (Undo) also works.
    /// </summary>
    public class CashBattleGoldPreview : EditorWindow
    {
        // ══════════════════════════════════════════════════════════════════════════
        // *** CASH SYNC REQUIREMENT ***
        // These values must stay identical to CashThemeForcer.cs.
        // ══════════════════════════════════════════════════════════════════════════
        private static readonly Color CYAN_NEON    = new Color(0f,    1f,    1f,    1f);
        private static readonly Color GREEN_NEON   = new Color(0.3f,  1f,    0.5f,  1f);
        private static readonly Color MAGENTA_NEON = new Color(1f,    0f,    0.8f,  1f);
        private static readonly Color DARK_BG      = new Color(0.02f, 0.05f, 0.1f,  1f);
        private static readonly Color PANEL_BG     = new Color(0.05f, 0.1f,  0.15f, 0.95f);

        private static readonly Color GOLD_PRIMARY  = new Color(1f,    0.84f, 0f,    1f);
        private static readonly Color GOLD_LIGHT    = new Color(1f,    0.93f, 0.4f,  1f);
        private static readonly Color GOLD_ACCENT   = new Color(0.96f, 0.64f, 0.08f, 1f);
        private static readonly Color GOLD_DARK_BG  = new Color(0.06f, 0.04f, 0.02f, 1f);
        private static readonly Color GOLD_PANEL_BG = new Color(0.1f,  0.08f, 0.04f, 0.95f);

        private const float TOLERANCE = 0.05f;
        // ══════════════════════════════════════════════════════════════════════════

        private static readonly string[] MINIGAME_SCENES =
            { "DigitRush", "FlashTap", "MemoryPairs", "OddOneOut", "QuickMath" };

        // ── State ─────────────────────────────────────────────────────────────────
        private bool   _goldActive;
        private bool   _originalsStored;
        private string _currentScene;
        private bool   _isMinigameScene;

        private int _textsChanged, _imagesChanged, _outlinesChanged;

        // Stored originals (precise restore — no reverse mapping guessing)
        private readonly Dictionary<int, Color> _origText    = new Dictionary<int, Color>();
        private readonly Dictionary<int, Color> _origImage   = new Dictionary<int, Color>();
        private readonly Dictionary<int, Color> _origOutline = new Dictionary<int, Color>();
        private Color _origCamBg;
        private bool  _camStored;

        // ── Menu Item ─────────────────────────────────────────────────────────────
        [MenuItem("DigitPark/Themes/CashBattle Gold Preview", false, 201)]
        public static void Open()
        {
            var win = GetWindow<CashBattleGoldPreview>("CashBattle Gold");
            win.minSize = new Vector2(480f, 520f);
            win.maxSize = new Vector2(600f, 700f);
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────────
        private void OnEnable()  { RefreshScene(); }
        private void OnFocus()   { RefreshScene(); }

        private void RefreshScene()
        {
            _currentScene    = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _isMinigameScene = System.Array.Exists(MINIGAME_SCENES, s => s == _currentScene);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GUI
        // ══════════════════════════════════════════════════════════════════════════
        private void OnGUI()
        {
            RefreshScene();

            if (!_isMinigameScene)
            {
                DrawBlockScreen();
                return;
            }

            DrawActiveUI();
        }

        // ── Block Screen (wrong scene) ────────────────────────────────────────────
        private void DrawBlockScreen()
        {
            // Full window dark red tint
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height),
                new Color(0.12f, 0.04f, 0.04f));

            float cx = position.width  * 0.5f;
            float cy = position.height * 0.5f;

            // Lock icon area
            var lockStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 48,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.6f, 0.15f, 0.15f) }
            };
            GUI.Label(new Rect(0, cy - 130f, position.width, 70f), "⊘", lockStyle);

            // Title
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 18,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.9f, 0.35f, 0.35f) }
            };
            GUI.Label(new Rect(0, cy - 60f, position.width, 30f),
                "Only works on Minigame Scenes", titleStyle);

            // Scene name
            var sceneStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 13,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            GUI.Label(new Rect(0, cy - 24f, position.width, 22f),
                $"Current scene:  \"{_currentScene}\"", sceneStyle);

            // Valid scenes list
            var listStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize  = 12,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };
            GUI.Label(new Rect(0, cy + 10f, position.width, 20f),
                "Open one of:", listStyle);

            var namesStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 13,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.75f, 0.75f, 0.85f) }
            };
            GUI.Label(new Rect(0, cy + 30f, position.width, 22f),
                "DigitRush  ·  FlashTap  ·  MemoryPairs  ·  OddOneOut  ·  QuickMath",
                namesStyle);
        }

        // ── Active UI (minigame scene) ────────────────────────────────────────────
        private void DrawActiveUI()
        {
            // ── Full background ───────────────────────────────────────────────────
            Color bgColor = _goldActive
                ? new Color(0.10f, 0.08f, 0.02f)
                : new Color(0.08f, 0.10f, 0.14f);
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), bgColor);

            float w = position.width;

            // ── BIG MODE BANNER ───────────────────────────────────────────────────
            float bannerH = 140f;
            Rect  banner  = new Rect(0, 0, w, bannerH);

            Color bannerColor = _goldActive
                ? new Color(0.18f, 0.13f, 0.02f)
                : new Color(0.05f, 0.09f, 0.15f);
            EditorGUI.DrawRect(banner, bannerColor);

            Color bannerBorder = _goldActive
                ? GOLD_PRIMARY
                : CYAN_NEON;
            DrawBorderRect(banner, bannerBorder, 3);

            // Mode text
            string modeText  = _goldActive ? "GOLD MODE" : "NORMAL MODE";
            string modeEmoji = _goldActive ? "★" : "◈";
            Color  modeColor = _goldActive ? GOLD_PRIMARY : CYAN_NEON;

            var bigLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 32,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = modeColor }
            };
            GUI.Label(new Rect(0, 14f, w, 52f), $"{modeEmoji}  {modeText}  {modeEmoji}", bigLabel);

            var subLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 13,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = _goldActive
                    ? new Color(1f, 0.85f, 0.4f)
                    : new Color(0.55f, 0.8f, 1f) }
            };
            string subText = _goldActive
                ? "CashTournament simulation active"
                : "Standard neon look (normal gameplay)";
            GUI.Label(new Rect(0, 64f, w, 22f), subText, subLabel);

            // Scene name badge
            var sceneStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(0.6f, 0.7f, 0.6f) }
            };
            GUI.Label(new Rect(0, 90f, w, 18f), $"Scene:  {_currentScene}", sceneStyle);

            // Stats line
            if (_textsChanged + _imagesChanged + _outlinesChanged > 0)
            {
                var statsStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize  = 10,
                    alignment = TextAnchor.MiddleCenter,
                    normal    = { textColor = new Color(0.55f, 0.55f, 0.55f) }
                };
                GUI.Label(new Rect(0, 110f, w, 16f),
                    $"{_textsChanged} texts  ·  {_imagesChanged} images  ·  {_outlinesChanged} outlines changed",
                    statsStyle);
            }

            float y = bannerH + 16f;

            // ── Color mapping table ───────────────────────────────────────────────
            DrawMappingTable(ref y);

            y += 12f;

            // ── BIG ACTION BUTTONS ────────────────────────────────────────────────
            float btnPad = 20f;
            float btnW   = (w - btnPad * 3f) * 0.5f;
            float btnH   = 56f;

            // Normal button
            Color prevBg = GUI.backgroundColor;
            bool  normalActive = !_goldActive;
            GUI.backgroundColor = normalActive
                ? new Color(0f, 0.7f, 0.75f)
                : new Color(0.2f, 0.22f, 0.26f);

            var btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            Rect normalBtn = new Rect(btnPad, y, btnW, btnH);
            if (GUI.Button(normalBtn, normalActive ? "◈  NORMAL  (active)" : "◈  Apply Normal", btnStyle))
            {
                if (_goldActive) RestoreNeon();
            }

            // Gold button
            GUI.backgroundColor = _goldActive
                ? new Color(0.85f, 0.65f, 0.05f)
                : new Color(0.38f, 0.28f, 0.04f);

            Rect goldBtn = new Rect(btnPad * 2f + btnW, y, btnW, btnH);
            if (GUI.Button(goldBtn, _goldActive ? "★  GOLD  (active)" : "★  Apply Gold", btnStyle))
            {
                if (!_goldActive) ApplyGold();
            }

            GUI.backgroundColor = prevBg;
            y += btnH + 16f;

            // ── Bottom info box ───────────────────────────────────────────────────
            DrawInfoBox(ref y);
        }

        // ── Color Mapping Table ───────────────────────────────────────────────────
        private void DrawMappingTable(ref float y)
        {
            float w    = position.width;
            float pad  = 20f;
            float rowH = 28f;

            // Header
            var hdrStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
            GUI.Label(new Rect(pad, y, w - pad * 2, 18f), "COLOR MAPPING  (verbatim from CashThemeForcer.cs)", hdrStyle);
            y += 20f;

            (Color neon, Color gold, string label)[] mappings =
            {
                (CYAN_NEON,    GOLD_PRIMARY,  "Cyan Neon    →  Gold Primary"),
                (GREEN_NEON,   GOLD_LIGHT,    "Green Neon   →  Gold Light"),
                (MAGENTA_NEON, GOLD_ACCENT,   "Magenta Neon →  Gold Accent"),
                (DARK_BG,      GOLD_DARK_BG,  "Dark BG      →  Warm Dark BG"),
                (PANEL_BG,     GOLD_PANEL_BG, "Panel BG     →  Warm Panel BG"),
            };

            float swW  = 26f;
            float swH  = 18f;
            float arW  = 28f;

            foreach (var (neon, gold, label) in mappings)
            {
                // Left swatch (source in current mode)
                Color leftC  = _goldActive ? gold : neon;
                Color rightC = _goldActive ? neon : gold;

                EditorGUI.DrawRect(new Rect(pad, y + 5f, swW, swH), leftC);
                DrawBorderRect(new Rect(pad, y + 5f, swW, swH), new Color(0, 0, 0, 0.5f), 1);

                var arStyle = new GUIStyle(EditorStyles.boldLabel)
                    { fontSize = 13, alignment = TextAnchor.MiddleCenter,
                      normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } };
                GUI.Label(new Rect(pad + swW + 2f, y, arW, rowH), "→", arStyle);

                EditorGUI.DrawRect(new Rect(pad + swW + arW + 4f, y + 5f, swW, swH), rightC);
                DrawBorderRect(new Rect(pad + swW + arW + 4f, y + 5f, swW, swH), new Color(0, 0, 0, 0.5f), 1);

                var lblStyle = new GUIStyle(EditorStyles.miniLabel)
                    { alignment = TextAnchor.MiddleLeft,
                      normal = { textColor = new Color(0.65f, 0.65f, 0.65f) } };
                GUI.Label(new Rect(pad + swW * 2f + arW + 12f, y, w * 0.55f, rowH), label, lblStyle);

                y += rowH;
            }
        }

        // ── Info Box ──────────────────────────────────────────────────────────────
        private void DrawInfoBox(ref float y)
        {
            float w   = position.width;
            float pad = 20f;

            string[] lines =
            {
                "• Mirrors CashThemeForcer.ApplyGoldTheme() — exact runtime behavior.",
                "• Scans ALL TextMeshProUGUI, Image, Outline, and Camera in scene.",
                "• Originals stored on first Apply — Restore brings back exact colors.",
                "• Ctrl+Z (Undo) also supported.",
                "• Does NOT affect any scene outside the 5 minigame scenes.",
            };

            float lineH = 17f;
            float boxH  = lines.Length * lineH + 16f;
            Rect  box   = new Rect(pad, y, w - pad * 2f, boxH);

            EditorGUI.DrawRect(box, new Color(0.1f, 0.1f, 0.14f));
            DrawBorderRect(box, new Color(0.3f, 0.3f, 0.38f), 1);

            var lineStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal    = { textColor = new Color(0.6f, 0.6f, 0.65f) },
                alignment = TextAnchor.MiddleLeft
            };

            for (int i = 0; i < lines.Length; i++)
                GUI.Label(new Rect(box.x + 10f, box.y + 8f + i * lineH, box.width - 16f, lineH),
                    lines[i], lineStyle);

            y += boxH + 8f;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Apply / Restore logic
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Exact mirror of CashThemeForcer.ApplyGoldTheme().
        /// *** CASH SYNC REQUIREMENT *** Keep in sync with CashThemeForcer.cs.
        /// </summary>
        private void ApplyGold()
        {
            _textsChanged = _imagesChanged = _outlinesChanged = 0;

            if (!_originalsStored) StoreOriginals();

            Undo.SetCurrentGroupName("CashBattle Gold Preview");
            int group = Undo.GetCurrentGroup();

            // TextMeshProUGUI — same as CashThemeForcer.ApplyToAllTexts()
            foreach (var tmp in FindObjectsOfType<TextMeshProUGUI>(true))
            {
                Color next = ToGold(tmp.color);
                if (next == tmp.color) continue;
                Undo.RecordObject(tmp, "Cash Gold");
                tmp.color = next;
                EditorUtility.SetDirty(tmp);
                _textsChanged++;
            }

            // Image — same as CashThemeForcer.ApplyToAllImages()
            foreach (var img in FindObjectsOfType<Image>(true))
            {
                Color next = ToGoldImg(img.color);
                if (next == img.color) continue;
                Undo.RecordObject(img, "Cash Gold");
                img.color = next;
                EditorUtility.SetDirty(img);
                _imagesChanged++;
            }

            // Outline — same as CashThemeForcer.ApplyToAllOutlines()
            foreach (var outline in FindObjectsOfType<Outline>(true))
            {
                Color next = ToGoldOutline(outline.effectColor);
                if (next == outline.effectColor) continue;
                Undo.RecordObject(outline, "Cash Gold");
                outline.effectColor = next;
                EditorUtility.SetDirty(outline);
                _outlinesChanged++;
            }

            // Camera — same as CashThemeForcer.ApplyToCamera()
            var cam = Camera.main;
            if (cam != null && Match(cam.backgroundColor, DARK_BG))
            {
                Undo.RecordObject(cam, "Cash Gold");
                cam.backgroundColor = GOLD_DARK_BG;
                EditorUtility.SetDirty(cam);
            }

            Undo.CollapseUndoOperations(group);

            _goldActive = true;
            SceneView.RepaintAll();
            Repaint();
        }

        private void RestoreNeon()
        {
            if (!_originalsStored) { _goldActive = false; Repaint(); return; }

            Undo.SetCurrentGroupName("Restore Neon (CashBattle)");
            int group = Undo.GetCurrentGroup();

            foreach (var tmp in FindObjectsOfType<TextMeshProUGUI>(true))
            {
                if (!_origText.TryGetValue(tmp.GetInstanceID(), out Color c)) continue;
                Undo.RecordObject(tmp, "Restore Neon");
                tmp.color = c;
                EditorUtility.SetDirty(tmp);
            }

            foreach (var img in FindObjectsOfType<Image>(true))
            {
                if (!_origImage.TryGetValue(img.GetInstanceID(), out Color c)) continue;
                Undo.RecordObject(img, "Restore Neon");
                img.color = c;
                EditorUtility.SetDirty(img);
            }

            foreach (var outline in FindObjectsOfType<Outline>(true))
            {
                if (!_origOutline.TryGetValue(outline.GetInstanceID(), out Color c)) continue;
                Undo.RecordObject(outline, "Restore Neon");
                outline.effectColor = c;
                EditorUtility.SetDirty(outline);
            }

            var cam = Camera.main;
            if (cam != null && _camStored)
            {
                Undo.RecordObject(cam, "Restore Neon");
                cam.backgroundColor = _origCamBg;
                EditorUtility.SetDirty(cam);
            }

            Undo.CollapseUndoOperations(group);

            _goldActive = _originalsStored = _camStored = false;
            _origText.Clear(); _origImage.Clear(); _origOutline.Clear();
            _textsChanged = _imagesChanged = _outlinesChanged = 0;

            SceneView.RepaintAll();
            Repaint();
        }

        private void StoreOriginals()
        {
            _origText.Clear(); _origImage.Clear(); _origOutline.Clear();

            foreach (var tmp in FindObjectsOfType<TextMeshProUGUI>(true))
                _origText[tmp.GetInstanceID()] = tmp.color;

            foreach (var img in FindObjectsOfType<Image>(true))
                _origImage[img.GetInstanceID()] = img.color;

            foreach (var outline in FindObjectsOfType<Outline>(true))
                _origOutline[outline.GetInstanceID()] = outline.effectColor;

            var cam = Camera.main;
            if (cam != null) { _origCamBg = cam.backgroundColor; _camStored = true; }

            _originalsStored = true;
        }

        // ── Color mapping — verbatim from CashThemeForcer ─────────────────────────
        private static Color ToGold(Color c)         // texts
        {
            if (Match(c, CYAN_NEON))    return GOLD_PRIMARY;
            if (Match(c, GREEN_NEON))   return GOLD_LIGHT;
            if (Match(c, MAGENTA_NEON)) return GOLD_ACCENT;
            return c;
        }

        private static Color ToGoldImg(Color c)      // images (includes BG)
        {
            if (Match(c, CYAN_NEON))    return GOLD_PRIMARY;
            if (Match(c, GREEN_NEON))   return GOLD_LIGHT;
            if (Match(c, MAGENTA_NEON)) return GOLD_ACCENT;
            if (Match(c, DARK_BG))      return GOLD_DARK_BG;
            if (Match(c, PANEL_BG))     return GOLD_PANEL_BG;
            return c;
        }

        private static Color ToGoldOutline(Color c)  // outlines
        {
            if (Match(c, CYAN_NEON))  return GOLD_PRIMARY;
            if (Match(c, GREEN_NEON)) return GOLD_LIGHT;
            return c;
        }

        private static bool Match(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < TOLERANCE &&
                   Mathf.Abs(a.g - b.g) < TOLERANCE &&
                   Mathf.Abs(a.b - b.b) < TOLERANCE;
        }

        // ── Utility ───────────────────────────────────────────────────────────────
        private static void DrawBorderRect(Rect r, Color c, int t)
        {
            EditorGUI.DrawRect(new Rect(r.x,        r.y,        r.width, t),  c);
            EditorGUI.DrawRect(new Rect(r.x,        r.yMax - t, r.width, t),  c);
            EditorGUI.DrawRect(new Rect(r.x,        r.y,        t, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - t, r.y,        t, r.height), c);
        }
    }
}
