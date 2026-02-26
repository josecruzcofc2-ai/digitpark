#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DigitPark.Editor
{
    /// <summary>
    /// Organizador central del menú DigitPark.
    /// Define la estructura jerárquica de todas las herramientas.
    ///
    /// ═══════════════════════════════════════════════════════════════════════════
    ///                        ESTRUCTURA DEL MENÚ DIGITPARK
    /// ═══════════════════════════════════════════════════════════════════════════
    ///
    /// DigitPark/
    /// │
    /// ├── 🔧 Tools/                              [0-99]
    /// │   ├── UI Setup Tool (Ctrl+Shift+U)       - Conectar referencias automáticamente
    /// │   ├── Auto Reference Assigner            - Asignar referencias por nombre
    /// │   ├── Icon Manager                       - Gestionar iconos del proyecto
    /// │   ├── Icons/                             - Operaciones con iconos
    /// │   └── Force Recompile                    - Forzar recompilación
    /// │
    /// ├── 🎨 UI Builders/                        [100-299]
    /// │   ├── Games/                             [110-149]
    /// │   │   ├── DigitRush
    /// │   │   ├── FlashTap
    /// │   │   ├── MemoryPairs
    /// │   │   ├── OddOneOut
    /// │   │   ├── QuickMath
    /// │   │   └── GameSelector
    /// │   │
    /// │   ├── Core/                              [150-179]
    /// │   │   ├── MainMenu
    /// │   │   ├── Matchmaking
    /// │   │   └── PlayModeSelection
    /// │   │
    /// │   ├── Monetization/                      [180-219]
    /// │   │   ├── Shop
    /// │   │   ├── DailyRewards
    /// │   │   ├── DailyMissions
    /// │   │   ├── Achievements
    /// │   │   └── Onboarding
    /// │   │
    /// │   ├── Social/                            [220-229]
    /// │   │   ├── Scores
    /// │   │   └── SearchPlayers
    /// │   │
    /// │   ├── Tournaments/                       [230-249]
    /// │   │   ├── Rebuild Complete UI
    /// │   │   ├── Open Builder Window
    /// │   │   ├── Quick Fix - Colors Only
    /// │   │   ├── TournamentCreate
    /// │   │   ├── TournamentLobby
    /// │   │   └── TournamentsBrowser
    /// │   │
    /// │   └── CashBattle/                        [250-299]
    /// │       ├── CashBattle Hub
    /// │       ├── Build Wallet Panel
    /// │       ├── Build Deposit Option Prefab
    /// │       └── Build Transaction Item Prefab
    /// │
    /// ├── ✨ Effects/                            [300-399]
    /// │   ├── Setup FeedbackManager
    /// │   ├── Add ButtonEffects to All/Selected
    /// │   ├── Add NeonGlow to Selected
    /// │   ├── Setup All Scenes
    /// │   ├── Create Background Particles
    /// │   └── [AUTO] Setup Complete System
    /// │
    /// ├── 🎬 Animation/                          [400-499]
    /// │   ├── Setup Animation System
    /// │   ├── Create Button3D Prefab
    /// │   ├── Create Transition Canvas Prefab
    /// │   ├── Create UI Animation Manager Prefab
    /// │   ├── Create All Prefabs
    /// │   ├── Create UI Materials
    /// │   ├── Add to Current Scene
    /// │   └── Batch/                             - Operaciones batch
    /// │
    /// ├── 🎨 Themes/                             [500-599]
    /// │   ├── Add ThemeApplier to Current Scene
    /// │   ├── Apply Theme Colors in Editor
    /// │   ├── Apply Theme Colors to ALL Scenes
    /// │   ├── Add ThemeApplier to Prefabs
    /// │   └── Add ThemeApplier to ALL Scenes
    /// │
    /// ├── 📦 Prefabs/                            [600-699]
    /// │   ├── Create Item Prefabs
    /// │   ├── Monetization/                      - Prefabs de monetización
    /// │   └── CashBattle/                        - Prefabs de CashBattle
    /// │
    /// ├── 🖼️ UI/                                 [350-399]
    /// │   ├── Background/                        - Configuración de fondos
    /// │   ├── Neon Glow/                         - Efectos neon
    /// │   └── Setup Premium UI
    /// │
    /// ├── 💰 Shop/                               [700-799]
    /// │   ├── Connect Shop References
    /// │   ├── Setup Shop Manager
    /// │   ├── Create Shop Item Data Assets
    /// │   ├── Add ShopItemUI to Items
    /// │   └── Entry Points/                      - Puntos de entrada a tienda
    /// │
    /// └── About DigitPark Tools                  [1000]
    ///
    /// ═══════════════════════════════════════════════════════════════════════════
    /// </summary>
    public static class DigitParkMenuOrganizer
    {
        [MenuItem("DigitPark/About DigitPark Tools", false, 1000)]
        public static void ShowAbout()
        {
            EditorUtility.DisplayDialog("DigitPark Editor Tools",
                "═══════════════════════════════════════\n" +
                "        DIGITPARK EDITOR TOOLS\n" +
                "═══════════════════════════════════════\n\n" +
                "🔧 TOOLS\n" +
                "   • UI Setup Tool (Ctrl+Shift+U)\n" +
                "   • Auto Reference Assigner (Ctrl+Shift+R)\n" +
                "   • Icon Manager\n\n" +
                "🎨 UI BUILDERS\n" +
                "   • Games (DigitRush, FlashTap, etc.)\n" +
                "   • Core (MainMenu, Matchmaking)\n" +
                "   • Monetization (Shop, DailyRewards)\n" +
                "   • Social (Scores, SearchPlayers)\n" +
                "   • Tournaments\n" +
                "   • CashBattle\n\n" +
                "✨ EFFECTS - Efectos visuales y feedback\n" +
                "🎬 ANIMATION - Sistema de animaciones\n" +
                "🎨 THEMES - Temas y colores\n" +
                "📦 PREFABS - Creación de prefabs\n" +
                "🖼️ UI - Configuración general de UI\n" +
                "💰 SHOP - Tienda y entry points\n" +
                "═══════════════════════════════════════",
                "OK");
        }
    }
}
#endif
