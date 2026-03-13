using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Games;

namespace DigitPark.Themes
{
    /// <summary>
    /// Runtime component that forces gold theme on minigame scenes when playing in CashTournament mode.
    /// Attach to any GameObject in the scene or let MinigameBase add it automatically.
    /// Replaces cyan/green neon colors with gold palette across all UI elements.
    /// </summary>
    public class CashThemeForcer : MonoBehaviour
    {
        // Normal neon colors (from UIBuilders)
        private static readonly Color CYAN_NEON = new Color(0f, 1f, 1f, 1f);
        private static readonly Color GREEN_NEON = new Color(0.3f, 1f, 0.5f, 1f);
        private static readonly Color MAGENTA_NEON = new Color(1f, 0f, 0.8f, 1f);
        private static readonly Color DARK_BG = new Color(0.02f, 0.05f, 0.1f, 1f);
        private static readonly Color PANEL_BG = new Color(0.05f, 0.1f, 0.15f, 0.95f);

        // Gold replacement colors
        private static readonly Color GOLD_PRIMARY = new Color(1f, 0.84f, 0f, 1f);          // #FFD700 - main gold
        private static readonly Color GOLD_LIGHT = new Color(1f, 0.93f, 0.4f, 1f);           // #FFED66 - light gold (replaces green)
        private static readonly Color GOLD_ACCENT = new Color(0.96f, 0.64f, 0.08f, 1f);      // #F5A314 - warm accent (replaces magenta)
        private static readonly Color GOLD_DARK_BG = new Color(0.06f, 0.04f, 0.02f, 1f);     // Dark warm background
        private static readonly Color GOLD_PANEL_BG = new Color(0.1f, 0.08f, 0.04f, 0.95f);  // Warm panel background

        private static readonly float COLOR_TOLERANCE = 0.05f;

        private void Start()
        {
            if (!ShouldApplyGoldTheme())
            {
                Destroy(this);
                return;
            }

            ApplyGoldTheme();
        }

        private bool ShouldApplyGoldTheme()
        {
            var session = GameSessionManager.Instance;
            return session != null &&
                   session.HasActiveSession &&
                   session.CurrentContext.Mode == GameMode.CashTournament;
        }

        /// <summary>
        /// Apply gold theme to all UI elements in the scene.
        /// Can also be called from Editor scripts for preview.
        /// </summary>
        public void ApplyGoldTheme()
        {
            ApplyToAllTexts();
            ApplyToAllImages();
            ApplyToAllOutlines();
            ApplyToCamera();

            Debug.Log("[CashThemeForcer] Gold theme applied to scene");
        }

        /// <summary>
        /// Revert gold theme back to normal neon colors.
        /// Used by Editor preview tool.
        /// </summary>
        public static void RevertToNormal(GameObject root)
        {
            foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (ColorsMatch(tmp.color, GOLD_PRIMARY)) tmp.color = CYAN_NEON;
                else if (ColorsMatch(tmp.color, GOLD_LIGHT)) tmp.color = GREEN_NEON;
                else if (ColorsMatch(tmp.color, GOLD_ACCENT)) tmp.color = MAGENTA_NEON;
            }

            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (ColorsMatch(img.color, GOLD_PRIMARY)) img.color = CYAN_NEON;
                else if (ColorsMatch(img.color, GOLD_LIGHT)) img.color = GREEN_NEON;
                else if (ColorsMatch(img.color, GOLD_ACCENT)) img.color = MAGENTA_NEON;
                else if (ColorsMatch(img.color, GOLD_DARK_BG)) img.color = DARK_BG;
                else if (ColorsMatch(img.color, GOLD_PANEL_BG)) img.color = PANEL_BG;
            }

            foreach (var outline in root.GetComponentsInChildren<Outline>(true))
            {
                if (ColorsMatch(outline.effectColor, GOLD_PRIMARY)) outline.effectColor = CYAN_NEON;
                else if (ColorsMatch(outline.effectColor, GOLD_LIGHT)) outline.effectColor = GREEN_NEON;
            }
        }

        private void ApplyToAllTexts()
        {
            foreach (var tmp in FindObjectsOfType<TextMeshProUGUI>(true))
            {
                if (ColorsMatch(tmp.color, CYAN_NEON)) tmp.color = GOLD_PRIMARY;
                else if (ColorsMatch(tmp.color, GREEN_NEON)) tmp.color = GOLD_LIGHT;
                else if (ColorsMatch(tmp.color, MAGENTA_NEON)) tmp.color = GOLD_ACCENT;
            }
        }

        private void ApplyToAllImages()
        {
            foreach (var img in FindObjectsOfType<Image>(true))
            {
                if (img.CompareTag("FrameLayer")) continue; // excluir frames de perfil
                if (img.gameObject.name == "AvatarFrame") continue; // fallback si el tag no existe
                if (ColorsMatch(img.color, CYAN_NEON)) img.color = GOLD_PRIMARY;
                else if (ColorsMatch(img.color, GREEN_NEON)) img.color = GOLD_LIGHT;
                else if (ColorsMatch(img.color, MAGENTA_NEON)) img.color = GOLD_ACCENT;
                else if (ColorsMatch(img.color, DARK_BG)) img.color = GOLD_DARK_BG;
                else if (ColorsMatch(img.color, PANEL_BG)) img.color = GOLD_PANEL_BG;
            }
        }

        private void ApplyToAllOutlines()
        {
            foreach (var outline in FindObjectsOfType<Outline>(true))
            {
                if (ColorsMatch(outline.effectColor, CYAN_NEON)) outline.effectColor = GOLD_PRIMARY;
                else if (ColorsMatch(outline.effectColor, GREEN_NEON)) outline.effectColor = GOLD_LIGHT;
            }
        }

        private void ApplyToCamera()
        {
            var cam = Camera.main;
            if (cam != null && ColorsMatch(cam.backgroundColor, DARK_BG))
            {
                cam.backgroundColor = GOLD_DARK_BG;
            }
        }

        private static bool ColorsMatch(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < COLOR_TOLERANCE &&
                   Mathf.Abs(a.g - b.g) < COLOR_TOLERANCE &&
                   Mathf.Abs(a.b - b.b) < COLOR_TOLERANCE;
        }
    }
}
