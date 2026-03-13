using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor Window — Theme Preview for DigitPark (20 themes).
    /// Opens via: DigitPark / Themes / Theme Preview
    ///
    /// HOW IT WORKS:
    ///   1. Finds every ThemeApplier component in the current scene (including inactive).
    ///   2. Reads each applier's elementType + applyTo* flags exactly as the runtime does.
    ///   3. Applies the SAME color mapping used by ThemeApplier.GetColorForElement().
    ///   4. Only touches components that ThemeApplier would touch — nothing else.
    ///   5. Original colors are stored on first preview and restored on demand.
    ///   6. Every change is registered with Unity Undo (Ctrl+Z works).
    ///
    /// SYNC GUARANTEE:
    ///   GetColorForElement() below is a verbatim copy of the private method inside
    ///   ThemeApplier.cs. If ThemeApplier.cs changes, update this file to match.
    ///   The *** SYNC REQUIREMENT *** comment marks the sync point.
    ///
    /// For CashBattle Gold preview on minigame scenes use:
    ///   DigitPark / Themes / CashBattle Gold Preview
    /// </summary>
    public class ThemeEditorPreview : EditorWindow
    {
        // ── Constants ─────────────────────────────────────────────────────────────
        private const float CARD_W       = 196f;
        private const float CARD_H       = 80f;
        private const float CARD_SPACING = 6f;
        private const float SWATCH_W     = 22f;
        private const float SWATCH_H     = 22f;
        private const int   COLS         = 3;

        // ── State ─────────────────────────────────────────────────────────────────
        private List<ThemeData> _themes          = new List<ThemeData>();
        private ThemeData       _previewTheme;
        private Vector2         _scroll;
        private string          _status          = "Click a theme card to preview it in the Scene View.";
        private int             _applierCount;
        private bool            _originalsStored;

        private readonly Dictionary<int, Color>      _origImage   = new Dictionary<int, Color>();
        private readonly Dictionary<int, Color>      _origText    = new Dictionary<int, Color>();
        private readonly Dictionary<int, Color>      _origOutline = new Dictionary<int, Color>();
        private readonly Dictionary<int, Color>      _origShadow  = new Dictionary<int, Color>();
        private readonly Dictionary<int, ColorBlock> _origButton  = new Dictionary<int, ColorBlock>();

        // ── Lazy styles ───────────────────────────────────────────────────────────
        private GUIStyle _titleStyle;
        private GUIStyle _tagStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _sectionLabel;
        private bool     _stylesBuilt;

        // ── Menu Item ─────────────────────────────────────────────────────────────
        [MenuItem("DigitPark/Themes/Theme Preview", false, 200)]
        public static void Open()
        {
            var win = GetWindow<ThemeEditorPreview>("Theme Preview");
            win.minSize = new Vector2(660f, 520f);
            win.LoadThemes();
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────────
        private void OnEnable() { LoadThemes(); RefreshCount(); }
        private void OnFocus()  { RefreshCount(); }

        // ── Theme Loading ─────────────────────────────────────────────────────────
        private void LoadThemes()
        {
            _themes.Clear();
            var found = Resources.LoadAll<ThemeData>("Themes");
            if (found == null || found.Length == 0)
            {
                _status = "WARNING: No ThemeData assets found in Resources/Themes/";
                return;
            }

            // Mirror sort order of ThemeManager.LoadThemesFromResources()
            System.Array.Sort(found, (a, b) =>
            {
                if (a.themeId == "neon_dark") return -1;
                if (b.themeId == "neon_dark") return  1;
                int ea = a.isPremium ? (a.isEarnable ? 1 : 2) : 0;
                int eb = b.isPremium ? (b.isEarnable ? 1 : 2) : 0;
                if (ea != eb) return ea.CompareTo(eb);
                return string.Compare(a.themeName, b.themeName, System.StringComparison.Ordinal);
            });
            _themes.AddRange(found);
        }

        private void RefreshCount()
        {
            var all = FindObjectsOfType<ThemeApplier>(true);
            _applierCount = all != null ? all.Length : 0;
        }

        // ── GUI ───────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            BuildStyles();
            DrawToolbar();
            DrawStatusBar();
            EditorGUILayout.Space(4f);
            DrawGrid();
            DrawFooter();
        }

        private void BuildStyles()
        {
            if (_stylesBuilt) return;
            _stylesBuilt = true;

            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                { fontSize = 12, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };

            _tagStyle = new GUIStyle(EditorStyles.miniLabel)
                { alignment = TextAnchor.MiddleLeft,
                  normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } };

            _statusStyle = new GUIStyle(EditorStyles.helpBox)
                { fontSize = 11, alignment = TextAnchor.MiddleLeft, richText = true };

            _sectionLabel = new GUIStyle(EditorStyles.boldLabel)
                { fontSize = 13, alignment = TextAnchor.MiddleLeft };
        }

        // ── Toolbar ───────────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("DigitPark  ·  Theme Preview Editor", _sectionLabel);
            GUILayout.FlexibleSpace();

            GUI.color = _applierCount > 0
                ? new Color(0.55f, 1f, 0.55f)
                : new Color(1f, 0.7f, 0.4f);
            GUILayout.Label($"  {_applierCount} ThemeApplier components  ", EditorStyles.toolbarButton);
            GUI.color = Color.white;

            if (GUILayout.Button("⟳ Refresh", EditorStyles.toolbarButton, GUILayout.Width(72)))
            { LoadThemes(); RefreshCount(); }

            EditorGUILayout.EndHorizontal();
        }

        // ── Status Bar ────────────────────────────────────────────────────────────
        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_status, _statusStyle, GUILayout.Height(26f));

            bool hasPrev = _previewTheme != null;
            GUI.enabled = hasPrev;
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = hasPrev ? new Color(1f, 0.65f, 0.2f) : Color.gray;
            if (GUILayout.Button("⟲  Restore Originals", GUILayout.Width(160f), GUILayout.Height(26f)))
                RestoreOriginals();
            GUI.backgroundColor = prevBg;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        // ── Theme Grid ────────────────────────────────────────────────────────────
        private void DrawGrid()
        {
            if (_themes.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No ThemeData assets found in Resources/Themes/\n" +
                    "Make sure the theme ScriptableObjects are inside Assets/Resources/Themes/",
                    MessageType.Warning);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            int   rows  = Mathf.CeilToInt(_themes.Count / (float)COLS);
            float total = rows * (CARD_H + CARD_SPACING);

            Rect area = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none,
                GUILayout.ExpandWidth(true), GUILayout.Height(total + 8f));

            float x0   = area.x + 4f;
            float y0   = area.y;
            int   col  = 0;
            float xCur = x0;
            float yCur = y0;

            for (int i = 0; i < _themes.Count; i++)
            {
                var theme = _themes[i];
                if (theme == null) continue;

                DrawCard(new Rect(xCur, yCur, CARD_W, CARD_H), theme, _previewTheme == theme);

                col++;
                if (col >= COLS) { col = 0; xCur = x0; yCur += CARD_H + CARD_SPACING; }
                else             { xCur += CARD_W + CARD_SPACING; }
            }

            EditorGUILayout.EndScrollView();
        }

        // ── Individual Card ───────────────────────────────────────────────────────
        private void DrawCard(Rect rect, ThemeData theme, bool isActive)
        {
            EditorGUI.DrawRect(rect, isActive
                ? new Color(0.13f, 0.28f, 0.13f)
                : new Color(0.16f, 0.16f, 0.20f));
            DrawBorderRect(rect,
                isActive ? new Color(0.3f, 0.95f, 0.3f) : new Color(0.30f, 0.30f, 0.38f),
                isActive ? 2 : 1);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                if (isActive) RestoreOriginals();
                else          ApplyPreview(theme);
            }

            float pad    = 10f;
            float innerX = rect.x + pad;
            float innerW = rect.width - pad * 2f;

            // Swatches
            Color[] sw = { theme.primaryBackground, theme.primaryAccent,
                           theme.secondaryAccent, theme.buttonPrimary, theme.glowColor };
            for (int s = 0; s < sw.Length; s++)
            {
                Rect sr = new Rect(innerX + s * (SWATCH_W + 4f), rect.y + 10f, SWATCH_W, SWATCH_H);
                EditorGUI.DrawRect(sr, sw[s]);
                DrawBorderRect(sr, new Color(0f, 0f, 0f, 0.45f), 1);
            }

            // Name
            _titleStyle.normal.textColor = isActive ? new Color(0.35f, 1f, 0.35f) : Color.white;
            GUI.Label(new Rect(innerX, rect.y + 10f + SWATCH_H + 6f, innerW, 18f),
                (isActive ? "✓  " : "    ") + theme.themeName, _titleStyle);

            // Tag
            string tag = theme.themeId == "neon_dark" ? "FREE  ·  BASE THEME"
                       : theme.isEarnable             ? "EARNABLE  ·  DG (or achievement)"
                       : theme.isPremium              ? "PREMIUM  ·  DG"
                       :                               "FREE";
            GUI.Label(new Rect(innerX + 4f, rect.y + 10f + SWATCH_H + 24f, innerW, 14f),
                tag, _tagStyle);
        }

        // ── Footer ────────────────────────────────────────────────────────────────
        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            GUILayout.Label(
                "Non-destructive  ·  Ctrl+Z to undo  ·  Only ThemeApplier targets modified",
                EditorStyles.toolbarButton);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        // ── Apply Preview ─────────────────────────────────────────────────────────
        private void ApplyPreview(ThemeData theme)
        {
            if (theme == null) return;

            var appliers = FindObjectsOfType<ThemeApplier>(true);
            if (appliers == null || appliers.Length == 0)
            {
                _status = "No ThemeApplier components found in scene. " +
                          "Use  DigitPark/Polish/Themes/Add to Current Scene.";
                Repaint();
                return;
            }

            if (!_originalsStored) StoreOriginals(appliers);

            Undo.SetCurrentGroupName($"Preview Theme: {theme.themeName}");
            int group = Undo.GetCurrentGroup();

            int changed = 0;
            foreach (var applier in appliers)
                if (ApplyToApplier(applier, theme)) changed++;

            Undo.CollapseUndoOperations(group);

            _previewTheme = theme;
            _status = $"Previewing  <b>{theme.themeName}</b>  —  {changed} / {appliers.Length} components updated.";
            Repaint();
        }

        // ── Per-Applier Apply ─────────────────────────────────────────────────────
        private bool ApplyToApplier(ThemeApplier applier, ThemeData theme)
        {
            if (applier == null || theme == null) return false;

            var so = new SerializedObject(applier);
            so.Update();

            var eProp = so.FindProperty("elementType");
            if (eProp == null) return false;

            var elementType = (ThemeApplier.ElementType)eProp.intValue;
            if (elementType == ThemeApplier.ElementType.None) return false;

            bool applyImg = so.FindProperty("applyToImage")?.boolValue   ?? true;
            bool applyTxt = so.FindProperty("applyToText")?.boolValue    ?? true;
            bool applyOut = so.FindProperty("applyToOutline")?.boolValue ?? false;
            bool applySha = so.FindProperty("applyToShadow")?.boolValue  ?? false;

            Image           img    = RefOrDetect<Image>(so,           "targetImage",      applier);
            TextMeshProUGUI tmp    = RefOrDetect<TextMeshProUGUI>(so, "targetTMPText",    applier);
            Text            legTxt = RefOrDetect<Text>(so,            "targetLegacyText", applier);
            Outline         outline= RefOrDetect<Outline>(so,         "targetOutline",    applier);
            Shadow          shadow = RefOrDetect<Shadow>(so,          "targetShadow",     applier);
            Button          button = RefOrDetect<Button>(so,          "targetButton",     applier);

            Color color   = GetColorForElement(theme, elementType);
            bool  changed = false;

            if (applyImg && img != null)
            { Undo.RecordObject(img, "Theme Preview"); img.color = color; EditorUtility.SetDirty(img); changed = true; }

            if (applyTxt && tmp != null)
            { Undo.RecordObject(tmp, "Theme Preview"); tmp.color = color; EditorUtility.SetDirty(tmp); changed = true; }

            if (applyTxt && legTxt != null)
            { Undo.RecordObject(legTxt, "Theme Preview"); legTxt.color = color; EditorUtility.SetDirty(legTxt); changed = true; }

            if (applyOut && outline != null)
            {
                Undo.RecordObject(outline, "Theme Preview");
                outline.effectColor = new Color(theme.glowColor.r, theme.glowColor.g,
                                                theme.glowColor.b, theme.glowIntensity);
                EditorUtility.SetDirty(outline); changed = true;
            }

            if (applySha && shadow != null && theme.useShadows)
            {
                Undo.RecordObject(shadow, "Theme Preview");
                shadow.effectColor    = theme.shadowColor;
                shadow.effectDistance = theme.shadowDistance;
                EditorUtility.SetDirty(shadow); changed = true;
            }

            if (button != null)
            {
                Undo.RecordObject(button, "Theme Preview");
                ColorBlock cb = button.colors;
                switch (elementType)
                {
                    case ThemeApplier.ElementType.ButtonPrimary:
                        cb.normalColor = theme.buttonPrimary; cb.highlightedColor = theme.buttonPrimaryHover;
                        cb.pressedColor = theme.buttonPrimaryPressed; cb.selectedColor = theme.buttonPrimaryHover;
                        cb.disabledColor = theme.textDisabled; break;
                    case ThemeApplier.ElementType.ButtonSecondary:
                        cb.normalColor = theme.buttonSecondary; cb.highlightedColor = theme.buttonSecondaryHover;
                        cb.pressedColor = theme.secondaryBackground; cb.selectedColor = theme.buttonSecondaryHover;
                        cb.disabledColor = theme.textDisabled; break;
                    case ThemeApplier.ElementType.ButtonDanger:
                        cb.normalColor = theme.buttonDanger;
                        cb.highlightedColor = new Color(theme.buttonDanger.r*1.2f, theme.buttonDanger.g*1.2f, theme.buttonDanger.b*1.2f);
                        cb.pressedColor     = new Color(theme.buttonDanger.r*0.8f, theme.buttonDanger.g*0.8f, theme.buttonDanger.b*0.8f); break;
                    case ThemeApplier.ElementType.ButtonSuccess:
                        cb.normalColor = theme.buttonSuccess;
                        cb.highlightedColor = new Color(theme.buttonSuccess.r*1.2f, theme.buttonSuccess.g*1.2f, theme.buttonSuccess.b*1.2f);
                        cb.pressedColor     = new Color(theme.buttonSuccess.r*0.8f, theme.buttonSuccess.g*0.8f, theme.buttonSuccess.b*0.8f); break;
                }
                cb.colorMultiplier = 1f; cb.fadeDuration = theme.colorTransitionDuration;
                button.colors = cb; EditorUtility.SetDirty(button); changed = true;
            }

            return changed;
        }

        // ── Store Originals ───────────────────────────────────────────────────────
        private void StoreOriginals(ThemeApplier[] appliers)
        {
            _origImage.Clear(); _origText.Clear();
            _origOutline.Clear(); _origShadow.Clear(); _origButton.Clear();

            foreach (var applier in appliers)
            {
                if (applier == null) continue;
                var so = new SerializedObject(applier);
                so.Update();

                var img    = RefOrDetect<Image>(so,           "targetImage",      applier);
                var tmp    = RefOrDetect<TextMeshProUGUI>(so, "targetTMPText",    applier);
                var legTxt = RefOrDetect<Text>(so,            "targetLegacyText", applier);
                var outline= RefOrDetect<Outline>(so,         "targetOutline",    applier);
                var shadow = RefOrDetect<Shadow>(so,          "targetShadow",     applier);
                var button = RefOrDetect<Button>(so,          "targetButton",     applier);

                if (img    != null) _origImage  [img.GetInstanceID()]    = img.color;
                if (tmp    != null) _origText   [tmp.GetInstanceID()]    = tmp.color;
                if (legTxt != null) _origText   [legTxt.GetInstanceID()] = legTxt.color;
                if (outline!= null) _origOutline[outline.GetInstanceID()]= outline.effectColor;
                if (shadow != null) _origShadow [shadow.GetInstanceID()] = shadow.effectColor;
                if (button != null) _origButton [button.GetInstanceID()] = button.colors;
            }
            _originalsStored = true;
        }

        // ── Restore Originals ─────────────────────────────────────────────────────
        private void RestoreOriginals()
        {
            if (!_originalsStored)
            { _previewTheme = null; _status = "Nothing to restore."; Repaint(); return; }

            var appliers = FindObjectsOfType<ThemeApplier>(true);
            if (appliers == null) return;

            Undo.SetCurrentGroupName("Restore Theme Originals");
            int group = Undo.GetCurrentGroup();

            foreach (var applier in appliers)
            {
                if (applier == null) continue;
                var so = new SerializedObject(applier);
                so.Update();

                var img    = RefOrDetect<Image>(so,           "targetImage",      applier);
                var tmp    = RefOrDetect<TextMeshProUGUI>(so, "targetTMPText",    applier);
                var legTxt = RefOrDetect<Text>(so,            "targetLegacyText", applier);
                var outline= RefOrDetect<Outline>(so,         "targetOutline",    applier);
                var shadow = RefOrDetect<Shadow>(so,          "targetShadow",     applier);
                var button = RefOrDetect<Button>(so,          "targetButton",     applier);

                if (img    != null && _origImage.TryGetValue(img.GetInstanceID(), out Color ic))
                { Undo.RecordObject(img, "Restore"); img.color = ic; EditorUtility.SetDirty(img); }
                if (tmp    != null && _origText.TryGetValue(tmp.GetInstanceID(), out Color tc))
                { Undo.RecordObject(tmp, "Restore"); tmp.color = tc; EditorUtility.SetDirty(tmp); }
                if (legTxt != null && _origText.TryGetValue(legTxt.GetInstanceID(), out Color lc))
                { Undo.RecordObject(legTxt, "Restore"); legTxt.color = lc; EditorUtility.SetDirty(legTxt); }
                if (outline!= null && _origOutline.TryGetValue(outline.GetInstanceID(), out Color oc))
                { Undo.RecordObject(outline, "Restore"); outline.effectColor = oc; EditorUtility.SetDirty(outline); }
                if (shadow != null && _origShadow.TryGetValue(shadow.GetInstanceID(), out Color sc))
                { Undo.RecordObject(shadow, "Restore"); shadow.effectColor = sc; EditorUtility.SetDirty(shadow); }
                if (button != null && _origButton.TryGetValue(button.GetInstanceID(), out ColorBlock cb))
                { Undo.RecordObject(button, "Restore"); button.colors = cb; EditorUtility.SetDirty(button); }
            }

            Undo.CollapseUndoOperations(group);

            _previewTheme = null; _originalsStored = false;
            _origImage.Clear(); _origText.Clear();
            _origOutline.Clear(); _origShadow.Clear(); _origButton.Clear();
            _status = "Originals restored.";
            Repaint();
        }

        // ── Color Mapping — EXACT MIRROR of ThemeApplier.GetColorForElement() ─────
        // *** SYNC REQUIREMENT *** Keep identical to ThemeApplier.cs private method.
        private static Color GetColorForElement(ThemeData t, ThemeApplier.ElementType type)
        {
            switch (type)
            {
                case ThemeApplier.ElementType.PrimaryBackground:   return t.primaryBackground;
                case ThemeApplier.ElementType.SecondaryBackground: return t.secondaryBackground;
                case ThemeApplier.ElementType.TertiaryBackground:  return t.tertiaryBackground;
                case ThemeApplier.ElementType.CardBackground:      return t.cardBackground;
                case ThemeApplier.ElementType.Overlay:             return t.overlayColor;
                case ThemeApplier.ElementType.ButtonPrimary:       return t.buttonPrimary;
                case ThemeApplier.ElementType.ButtonSecondary:     return t.buttonSecondary;
                case ThemeApplier.ElementType.ButtonDanger:        return t.buttonDanger;
                case ThemeApplier.ElementType.ButtonSuccess:       return t.buttonSuccess;
                case ThemeApplier.ElementType.TextPrimary:         return t.textPrimary;
                case ThemeApplier.ElementType.TextSecondary:       return t.textSecondary;
                case ThemeApplier.ElementType.TextDisabled:        return t.textDisabled;
                case ThemeApplier.ElementType.TextTitle:           return t.textTitle;
                case ThemeApplier.ElementType.TextOnPrimary:       return t.textOnPrimary;
                case ThemeApplier.ElementType.TextOnDanger:        return t.textOnDanger;
                case ThemeApplier.ElementType.TextOnSuccess:       return t.textOnSuccess;
                case ThemeApplier.ElementType.InputBackground:     return t.inputBackground;
                case ThemeApplier.ElementType.InputBorder:         return t.inputBorder;
                case ThemeApplier.ElementType.InputPlaceholder:    return t.inputPlaceholder;
                case ThemeApplier.ElementType.Accent:              return t.primaryAccent;
                case ThemeApplier.ElementType.AccentSecondary:     return t.secondaryAccent;
                case ThemeApplier.ElementType.AccentTertiary:      return t.tertiaryAccent;
                case ThemeApplier.ElementType.Premium:             return t.premiumColor;
                case ThemeApplier.ElementType.Glow:                return t.glowColor;
                case ThemeApplier.ElementType.TabActive:           return t.tabActive;
                case ThemeApplier.ElementType.TabInactive:         return t.tabInactive;
                case ThemeApplier.ElementType.ToggleBackground:    return t.toggleOff;
                case ThemeApplier.ElementType.ToggleCheckmark:     return t.toggleCheckmark;
                case ThemeApplier.ElementType.SliderTrack:         return t.sliderTrack;
                case ThemeApplier.ElementType.SliderFill:          return t.sliderFill;
                case ThemeApplier.ElementType.SliderHandle:        return t.sliderHandle;
                case ThemeApplier.ElementType.ScrollbarTrack:      return t.scrollbarTrack;
                case ThemeApplier.ElementType.ScrollbarHandle:     return t.scrollbarHandle;
                case ThemeApplier.ElementType.Error:               return t.errorColor;
                case ThemeApplier.ElementType.Warning:             return t.warningColor;
                case ThemeApplier.ElementType.Success:             return t.successColor;
                case ThemeApplier.ElementType.Info:                return t.infoColor;
                case ThemeApplier.ElementType.Rank1:               return t.rank1Color;
                case ThemeApplier.ElementType.Rank2:               return t.rank2Color;
                case ThemeApplier.ElementType.Rank3:               return t.rank3Color;
                case ThemeApplier.ElementType.RowEven:             return t.rowEven;
                case ThemeApplier.ElementType.RowOdd:              return t.rowOdd;
                case ThemeApplier.ElementType.HeaderPurple:        return t.headerPurple;
                case ThemeApplier.ElementType.HeaderNavy:          return t.headerNavy;
                case ThemeApplier.ElementType.BackgroundNavy:      return t.backgroundNavy;
                case ThemeApplier.ElementType.BackgroundPurple:    return t.backgroundPurple;
                case ThemeApplier.ElementType.ButtonGlowPrimary:   return t.glowColor;
                case ThemeApplier.ElementType.ButtonGlowPremium:   return t.premiumColor;
                case ThemeApplier.ElementType.ButtonGlowSuccess:   return t.successColor;
                case ThemeApplier.ElementType.ButtonGlowDanger:    return t.errorColor;
                case ThemeApplier.ElementType.ButtonGlowNavy:      return t.primaryAccent;
                default: return Color.white;
            }
        }

        // ── Utilities ─────────────────────────────────────────────────────────────
        private static T RefOrDetect<T>(SerializedObject so, string propName, Component owner)
            where T : Component
        {
            var prop = so.FindProperty(propName);
            if (prop?.objectReferenceValue is T direct) return direct;
            return owner.GetComponent<T>();
        }

        private static void DrawBorderRect(Rect r, Color c, int t)
        {
            EditorGUI.DrawRect(new Rect(r.x,        r.y,        r.width, t),  c);
            EditorGUI.DrawRect(new Rect(r.x,        r.yMax - t, r.width, t),  c);
            EditorGUI.DrawRect(new Rect(r.x,        r.y,        t, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - t, r.y,        t, r.height), c);
        }
    }
}
