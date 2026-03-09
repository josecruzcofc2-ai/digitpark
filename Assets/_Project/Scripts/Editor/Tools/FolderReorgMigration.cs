#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DigitPark.Editor
{
    /// <summary>
    /// Migration tool for reorganizing Scripts/ into a feature-first structure.
    /// Run phases IN ORDER via DigitPark/Migration/ menu.
    /// Each phase creates target folders and moves files via AssetDatabase.MoveAsset() to preserve GUIDs.
    /// See docs/FOLDER_REORGANIZATION.md for the full plan.
    /// </summary>
    public static class FolderReorgMigration
    {
        const string SRC = "Assets/_Project/Scripts";
        const string DST = "Assets/_Project/Scripts/Runtime";

        static int _moved;
        static int _errors;
        static int _skipped;

        // ───────────────────── Helpers ─────────────────────

        static void EnsureFolder(string path)
        {
            path = path.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void MoveFile(string from, string to)
        {
            from = from.Replace("\\", "/");
            to = to.Replace("\\", "/");

            if (!File.Exists(from) && !File.Exists(from.Replace("/", "\\")))
            {
                Debug.LogWarning($"[Migration] Source not found, skipping: {from}");
                _skipped++;
                return;
            }

            EnsureFolder(Path.GetDirectoryName(to).Replace("\\", "/"));
            string result = AssetDatabase.MoveAsset(from, to);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.LogError($"[Migration] Failed to move {from} → {to}: {result}");
                _errors++;
            }
            else
            {
                _moved++;
            }
        }

        static void MoveBatch(string phaseName, (string from, string to)[] moves)
        {
            _moved = 0;
            _errors = 0;
            _skipped = 0;

            if (!EditorUtility.DisplayDialog($"Migration: {phaseName}",
                $"Se moveran {moves.Length} archivos.\n¿Continuar?", "Si", "Cancelar"))
                return;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var (from, to) in moves)
                    MoveFile(from, to);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[Migration] {phaseName} completado: {_moved} movidos, {_skipped} saltados, {_errors} errores");
            EditorUtility.DisplayDialog("Resultado",
                $"{phaseName}\n\nMovidos: {_moved}\nSaltados: {_skipped}\nErrores: {_errors}", "OK");
        }

        // ───────────────────── Phase 0: Create Skeleton ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 0 - Create Folders", false, 100)]
        static void Phase0_CreateFolders()
        {
            string[] folders = new[]
            {
                // Core
                $"{DST}/Core/Boot",
                $"{DST}/Core/Audio",
                $"{DST}/Core/Navigation",
                $"{DST}/Core/Popups",
                $"{DST}/Core/Network",
                $"{DST}/Core/SafeArea",
                // Data
                $"{DST}/Data/Missions",
                // Services
                $"{DST}/Services/Firebase",
                $"{DST}/Services/Interfaces",
                $"{DST}/Services/Mock",
                $"{DST}/Services/Triumph",
                // Localization
                $"{DST}/Localization",
                // UI
                $"{DST}/UI/Common",
                $"{DST}/UI/Components",
                $"{DST}/UI/Panels",
                $"{DST}/UI/Notifications",
                $"{DST}/UI/Items",
                $"{DST}/UI/Builders",
                // Animations
                $"{DST}/Animations/Animators",
                $"{DST}/Animations/Components",
                $"{DST}/Animations/Core",
                // Effects
                $"{DST}/Effects",
                // Themes
                $"{DST}/Themes",
                // Features
                $"{DST}/Features/Auth",
                $"{DST}/Features/MainMenu",
                $"{DST}/Features/Settings",
                $"{DST}/Features/Games/Core",
                $"{DST}/Features/Games/DigitRush",
                $"{DST}/Features/Games/FlashTap",
                $"{DST}/Features/Games/MemoryPairs",
                $"{DST}/Features/Games/OddOneOut",
                $"{DST}/Features/Games/QuickMath",
                $"{DST}/Features/Games/Navigation",
                $"{DST}/Features/Games/Results",
                $"{DST}/Features/Social/Friends",
                $"{DST}/Features/Social/Profile",
                $"{DST}/Features/Social/Notifications",
                $"{DST}/Features/Monetization/Currency",
                $"{DST}/Features/Monetization/Shop",
                $"{DST}/Features/Monetization/Achievements",
                $"{DST}/Features/Monetization/DailyMissions",
                $"{DST}/Features/Monetization/DailyRewards",
                $"{DST}/Features/Monetization/Premium",
                $"{DST}/Features/Monetization/Progression",
                $"{DST}/Features/Tournaments",
                $"{DST}/Features/Onboarding",
                $"{DST}/Features/CashBattle/Hub",
                $"{DST}/Features/CashBattle/History",
                $"{DST}/Features/CashBattle/Profile",
                $"{DST}/Features/CashBattle/Wallet",
                $"{DST}/Features/CashBattle/Tournaments",
                $"{DST}/Features/CashBattle/Results",
                // DevTools
                $"{DST}/DevTools",
            };

            foreach (string f in folders)
                EnsureFolder(f);

            AssetDatabase.Refresh();
            Debug.Log($"[Migration] Phase 0: {folders.Length} carpetas creadas");
            EditorUtility.DisplayDialog("Phase 0", $"{folders.Length} carpetas creadas", "OK");
        }

        // ───────────────────── Phase 1: Data ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 1 - Data", false, 101)]
        static void Phase1_Data()
        {
            MoveBatch("Phase 1 - Data", new[]
            {
                ($"{SRC}/Data/FriendData.cs",        $"{DST}/Data/FriendData.cs"),
                ($"{SRC}/Data/MatchHistoryData.cs",  $"{DST}/Data/MatchHistoryData.cs"),
                ($"{SRC}/Data/PlayerData.cs",        $"{DST}/Data/PlayerData.cs"),
                ($"{SRC}/Data/PlayerSettings.cs",    $"{DST}/Data/PlayerSettings.cs"),
                ($"{SRC}/Data/TournamentData.cs",    $"{DST}/Data/TournamentData.cs"),
                ($"{SRC}/Data/Missions/MissionDefinitionSO.cs",   $"{DST}/Data/Missions/MissionDefinitionSO.cs"),
                ($"{SRC}/Data/Missions/MissionPoolSO.cs",         $"{DST}/Data/Missions/MissionPoolSO.cs"),
                ($"{SRC}/Data/Missions/MissionProgressReporter.cs", $"{DST}/Data/Missions/MissionProgressReporter.cs"),
                // FrameData moves from Services to Data
                ($"{SRC}/Services/FrameData.cs",     $"{DST}/Data/FrameData.cs"),
            });
        }

        // ───────────────────── Phase 2: Services ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 2 - Services", false, 102)]
        static void Phase2_Services()
        {
            MoveBatch("Phase 2 - Services", new[]
            {
                // Root services (NetworkService moves to Core in Phase 5)
                ($"{SRC}/Services/AchievementService.cs",         $"{DST}/Services/AchievementService.cs"),
                ($"{SRC}/Services/ATTService.cs",                 $"{DST}/Services/ATTService.cs"),
                ($"{SRC}/Services/AvatarService.cs",              $"{DST}/Services/AvatarService.cs"),
                ($"{SRC}/Services/ChatFilterService.cs",          $"{DST}/Services/ChatFilterService.cs"),
                ($"{SRC}/Services/DailyRewardService.cs",         $"{DST}/Services/DailyRewardService.cs"),
                ($"{SRC}/Services/DeepLinkService.cs",            $"{DST}/Services/DeepLinkService.cs"),
                ($"{SRC}/Services/EmoteService.cs",               $"{DST}/Services/EmoteService.cs"),
                ($"{SRC}/Services/FriendService.cs",              $"{DST}/Services/FriendService.cs"),
                ($"{SRC}/Services/LocationRestrictionService.cs", $"{DST}/Services/LocationRestrictionService.cs"),
                ($"{SRC}/Services/MatchmakingService.cs",         $"{DST}/Services/MatchmakingService.cs"),
                ($"{SRC}/Services/NotificationStorageService.cs", $"{DST}/Services/NotificationStorageService.cs"),
                ($"{SRC}/Services/PlayerFrameService.cs",         $"{DST}/Services/PlayerFrameService.cs"),
                ($"{SRC}/Services/PlayerTitleService.cs",         $"{DST}/Services/PlayerTitleService.cs"),
                ($"{SRC}/Services/ReviewService.cs",              $"{DST}/Services/ReviewService.cs"),
                ($"{SRC}/Services/ServiceLocator.cs",             $"{DST}/Services/ServiceLocator.cs"),
                ($"{SRC}/Services/VictoryEffectService.cs",       $"{DST}/Services/VictoryEffectService.cs"),
                // Firebase
                ($"{SRC}/Services/Firebase/AnalyticsService.cs",       $"{DST}/Services/Firebase/AnalyticsService.cs"),
                ($"{SRC}/Services/Firebase/AuthenticationService.cs",  $"{DST}/Services/Firebase/AuthenticationService.cs"),
                ($"{SRC}/Services/Firebase/DatabaseService.cs",        $"{DST}/Services/Firebase/DatabaseService.cs"),
                ($"{SRC}/Services/Firebase/NotificationService.cs",    $"{DST}/Services/Firebase/NotificationService.cs"),
                // Interfaces
                ($"{SRC}/Services/Interfaces/IKYCService.cs",          $"{DST}/Services/Interfaces/IKYCService.cs"),
                ($"{SRC}/Services/Interfaces/IMatchmakingService.cs",  $"{DST}/Services/Interfaces/IMatchmakingService.cs"),
                ($"{SRC}/Services/Interfaces/ITournamentService.cs",   $"{DST}/Services/Interfaces/ITournamentService.cs"),
                ($"{SRC}/Services/Interfaces/IWalletService.cs",       $"{DST}/Services/Interfaces/IWalletService.cs"),
                // Mock
                ($"{SRC}/Services/Mock/MockKYCService.cs",             $"{DST}/Services/Mock/MockKYCService.cs"),
                ($"{SRC}/Services/Mock/MockMatchmakingService.cs",     $"{DST}/Services/Mock/MockMatchmakingService.cs"),
                ($"{SRC}/Services/Mock/MockTournamentService.cs",      $"{DST}/Services/Mock/MockTournamentService.cs"),
                ($"{SRC}/Services/Mock/MockWalletService.cs",          $"{DST}/Services/Mock/MockWalletService.cs"),
                // Triumph
                ($"{SRC}/Services/Triumph/TriumphServices.cs",         $"{DST}/Services/Triumph/TriumphServices.cs"),
                ($"{SRC}/Services/TriumphManager.cs",                  $"{DST}/Services/Triumph/TriumphManager.cs"),
            });
        }

        // ───────────────────── Phase 3: Localization ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 3 - Localization", false, 103)]
        static void Phase3_Localization()
        {
            MoveBatch("Phase 3 - Localization", new[]
            {
                ($"{SRC}/Localization/AutoLocalizer.cs",          $"{DST}/Localization/AutoLocalizer.cs"),
                ($"{SRC}/Localization/LocalizationManager.cs",    $"{DST}/Localization/LocalizationManager.cs"),
                ($"{SRC}/Localization/LocalizedTextComponent.cs", $"{DST}/Localization/LocalizedTextComponent.cs"),
            });
        }

        // ───────────────────── Phase 4: Themes ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 4 - Themes", false, 104)]
        static void Phase4_Themes()
        {
            MoveBatch("Phase 4 - Themes", new[]
            {
                ($"{SRC}/Themes/CashThemeForcer.cs",    $"{DST}/Themes/CashThemeForcer.cs"),
                ($"{SRC}/Themes/ThemeApplier.cs",       $"{DST}/Themes/ThemeApplier.cs"),
                ($"{SRC}/Themes/ThemeData.cs",          $"{DST}/Themes/ThemeData.cs"),
                ($"{SRC}/Themes/ThemeInitializer.cs",   $"{DST}/Themes/ThemeInitializer.cs"),
                ($"{SRC}/Themes/ThemeManager.cs",       $"{DST}/Themes/ThemeManager.cs"),
                // NeonThemeColors moves from UI/Theme/ to Themes/
                ($"{SRC}/UI/Theme/NeonThemeColors.cs",  $"{DST}/Themes/NeonThemeColors.cs"),
            });
        }

        // ───────────────────── Phase 5: Effects ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 5 - Effects", false, 105)]
        static void Phase5_Effects()
        {
            MoveBatch("Phase 5 - Effects", new[]
            {
                ($"{SRC}/Effects/ButtonEffects.cs",         $"{DST}/Effects/ButtonEffects.cs"),
                ($"{SRC}/Effects/CelebrationManager.cs",    $"{DST}/Effects/CelebrationManager.cs"),
                ($"{SRC}/Effects/FeedbackManager.cs",       $"{DST}/Effects/FeedbackManager.cs"),
                ($"{SRC}/Effects/FloatingText.cs",          $"{DST}/Effects/FloatingText.cs"),
                ($"{SRC}/Effects/NeonGlowEffect.cs",        $"{DST}/Effects/NeonGlowEffect.cs"),
                ($"{SRC}/Effects/ParticleSystemManager.cs", $"{DST}/Effects/ParticleSystemManager.cs"),
            });
        }

        // ───────────────────── Phase 6: Animations ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 6 - Animations", false, 106)]
        static void Phase6_Animations()
        {
            // WinCelebrationAnimator and ComboVisualController are extracted to Features/Games
            // in Phase 9. All other Animations files move here.
            MoveBatch("Phase 6 - Animations", new[]
            {
                // Root
                ($"{SRC}/Animations/AnimConstants.cs", $"{DST}/Animations/AnimConstants.cs"),
                // Animators
                ($"{SRC}/Animations/Animators/CashProfileAnimator.cs",    $"{DST}/Animations/Animators/CashProfileAnimator.cs"),
                ($"{SRC}/Animations/Animators/CurrencyAnimator.cs",       $"{DST}/Animations/Animators/CurrencyAnimator.cs"),
                ($"{SRC}/Animations/Animators/GameSelectorAnimator.cs",   $"{DST}/Animations/Animators/GameSelectorAnimator.cs"),
                ($"{SRC}/Animations/Animators/MainMenuAnimator.cs",       $"{DST}/Animations/Animators/MainMenuAnimator.cs"),
                ($"{SRC}/Animations/Animators/MatchmakingAnimator.cs",    $"{DST}/Animations/Animators/MatchmakingAnimator.cs"),
                ($"{SRC}/Animations/Animators/ParticleEffectSpawner.cs",  $"{DST}/Animations/Animators/ParticleEffectSpawner.cs"),
                ($"{SRC}/Animations/Animators/RewardClaimAnimator.cs",    $"{DST}/Animations/Animators/RewardClaimAnimator.cs"),
                ($"{SRC}/Animations/Animators/TrophyShowcaseAnimator.cs", $"{DST}/Animations/Animators/TrophyShowcaseAnimator.cs"),
                // Components (minus 2 extracted files)
                ($"{SRC}/Animations/Components/AnimatedLoadingState.cs",  $"{DST}/Animations/Components/AnimatedLoadingState.cs"),
                ($"{SRC}/Animations/Components/AnimatedPanel.cs",        $"{DST}/Animations/Components/AnimatedPanel.cs"),
                ($"{SRC}/Animations/Components/BadgeAnimator.cs",        $"{DST}/Animations/Components/BadgeAnimator.cs"),
                ($"{SRC}/Animations/Components/Button3D.cs",             $"{DST}/Animations/Components/Button3D.cs"),
                ($"{SRC}/Animations/Components/CountdownAnimator.cs",    $"{DST}/Animations/Components/CountdownAnimator.cs"),
                ($"{SRC}/Animations/Components/EmptyStateAnimator.cs",   $"{DST}/Animations/Components/EmptyStateAnimator.cs"),
                ($"{SRC}/Animations/Components/NavTransitionAnimator.cs", $"{DST}/Animations/Components/NavTransitionAnimator.cs"),
                ($"{SRC}/Animations/Components/SceneTransitionManager.cs", $"{DST}/Animations/Components/SceneTransitionManager.cs"),
                ($"{SRC}/Animations/Components/ScoreRevealAnimator.cs",  $"{DST}/Animations/Components/ScoreRevealAnimator.cs"),
                ($"{SRC}/Animations/Components/StaggeredListAnimator.cs", $"{DST}/Animations/Components/StaggeredListAnimator.cs"),
                ($"{SRC}/Animations/Components/TabTransitionAnimator.cs", $"{DST}/Animations/Components/TabTransitionAnimator.cs"),
                ($"{SRC}/Animations/Components/UIEffects.cs",            $"{DST}/Animations/Components/UIEffects.cs"),
                // Core
                ($"{SRC}/Animations/Core/UIAnimationManager.cs", $"{DST}/Animations/Core/UIAnimationManager.cs"),
                ($"{SRC}/Animations/Core/UIAnimations.cs",       $"{DST}/Animations/Core/UIAnimations.cs"),
            });
        }

        // ───────────────────── Phase 7: Core ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 7 - Core", false, 107)]
        static void Phase7_Core()
        {
            MoveBatch("Phase 7 - Core", new[]
            {
                // Boot
                ($"{SRC}/Managers/BootManager.cs",       $"{DST}/Core/Boot/BootManager.cs"),
                ($"{SRC}/UI/BootAnimator.cs",            $"{DST}/Core/Boot/BootAnimator.cs"),
                ($"{SRC}/Tools/EditorBootConfig.cs",     $"{DST}/Core/Boot/EditorBootConfig.cs"),
                // Audio
                ($"{SRC}/Managers/AudioManager.cs",      $"{DST}/Core/Audio/AudioManager.cs"),
                // Navigation
                ($"{SRC}/Monetization/Shop/SceneNavigator.cs", $"{DST}/Core/Navigation/SceneNavigator.cs"),
                ($"{SRC}/UI/BackButton.cs",              $"{DST}/Core/Navigation/BackButton.cs"),
                ($"{SRC}/UI/BackButtonGold.cs",          $"{DST}/Core/Navigation/BackButtonGold.cs"),
                // Popups
                ($"{SRC}/UI/PopupManager.cs",            $"{DST}/Core/Popups/PopupManager.cs"),
                ($"{SRC}/UI/Common/ConfirmationPopup.cs",       $"{DST}/Core/Popups/ConfirmationPopup.cs"),
                ($"{SRC}/UI/Common/LogoutConfirmationPopup.cs", $"{DST}/Core/Popups/LogoutConfirmationPopup.cs"),
                ($"{SRC}/UI/Common/UsernamePopup.cs",           $"{DST}/Core/Popups/UsernamePopup.cs"),
                ($"{SRC}/UI/Panels/ConfirmPanelUI.cs",   $"{DST}/Core/Popups/ConfirmPanelUI.cs"),
                ($"{SRC}/UI/Panels/ErrorPanelUI.cs",     $"{DST}/Core/Popups/ErrorPanelUI.cs"),
                ($"{SRC}/UI/Panels/InputPanelUI.cs",     $"{DST}/Core/Popups/InputPanelUI.cs"),
                // Network
                ($"{SRC}/Services/NetworkService.cs",        $"{DST}/Core/Network/NetworkService.cs"),
                ($"{SRC}/UI/Panels/NetworkStatusBanner.cs",  $"{DST}/Core/Network/NetworkStatusBanner.cs"),
                // SafeArea
                ($"{SRC}/UI/SafeAreaHandler.cs",         $"{DST}/Core/SafeArea/SafeAreaHandler.cs"),
                ($"{SRC}/UI/SafeAreaManager.cs",         $"{DST}/Core/SafeArea/SafeAreaManager.cs"),
            });
        }

        // ───────────────────── Phase 8: UI (shared) ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 8 - UI Shared", false, 108)]
        static void Phase8_UIShared()
        {
            MoveBatch("Phase 8 - UI Shared", new[]
            {
                // Common
                ($"{SRC}/UI/FontSizes.cs",                  $"{DST}/UI/Common/FontSizes.cs"),
                ($"{SRC}/UI/UIFactory.cs",                  $"{DST}/UI/Common/UIFactory.cs"),
                ($"{SRC}/UI/UICanvasHelper.cs",             $"{DST}/UI/Common/UICanvasHelper.cs"),
                ($"{SRC}/UI/LocalizedTextLayoutFixer.cs",   $"{DST}/UI/Common/LocalizedTextLayoutFixer.cs"),
                // Components (all 10)
                ($"{SRC}/UI/Components/AccessibilityHelper.cs",      $"{DST}/UI/Components/AccessibilityHelper.cs"),
                ($"{SRC}/UI/Components/AvatarInitialGenerator.cs",   $"{DST}/UI/Components/AvatarInitialGenerator.cs"),
                ($"{SRC}/UI/Components/AvatarUI.cs",                 $"{DST}/UI/Components/AvatarUI.cs"),
                ($"{SRC}/UI/Components/DropdownScrollFix.cs",        $"{DST}/UI/Components/DropdownScrollFix.cs"),
                ($"{SRC}/UI/Components/LanguageDropdownStyler.cs",   $"{DST}/UI/Components/LanguageDropdownStyler.cs"),
                ($"{SRC}/UI/Components/NeonButtonGlow.cs",           $"{DST}/UI/Components/NeonButtonGlow.cs"),
                ($"{SRC}/UI/Components/RoundedCorners.cs",           $"{DST}/UI/Components/RoundedCorners.cs"),
                ($"{SRC}/UI/Components/ThemeDropdownController.cs",  $"{DST}/UI/Components/ThemeDropdownController.cs"),
                ($"{SRC}/UI/Components/ThemeSelector.cs",            $"{DST}/UI/Components/ThemeSelector.cs"),
                ($"{SRC}/UI/Components/UIPolish.cs",                 $"{DST}/UI/Components/UIPolish.cs"),
                // Panels
                ($"{SRC}/UI/Panels/PremiumPanelUI.cs",         $"{DST}/UI/Panels/PremiumPanelUI.cs"),
                ($"{SRC}/UI/Panels/StylesProPromptPanel.cs",   $"{DST}/UI/Panels/StylesProPromptPanel.cs"),
                ($"{SRC}/UI/PremiumCard.cs",                   $"{DST}/UI/Panels/PremiumCard.cs"),
                // Notifications
                ($"{SRC}/UI/InAppToastUI.cs",                          $"{DST}/UI/Notifications/InAppToastUI.cs"),
                ($"{SRC}/UI/AchievementToastUI.cs",                    $"{DST}/UI/Notifications/AchievementToastUI.cs"),
                ($"{SRC}/Managers/InAppNotificationManager.cs",        $"{DST}/UI/Notifications/InAppNotificationManager.cs"),
                ($"{SRC}/Managers/AchievementNotificationManager.cs",  $"{DST}/UI/Notifications/AchievementNotificationManager.cs"),
                ($"{SRC}/Managers/InAppNotificationInitializer.cs",    $"{DST}/UI/Notifications/InAppNotificationInitializer.cs"),
                ($"{SRC}/Managers/AchievementNotificationInitializer.cs", $"{DST}/UI/Notifications/AchievementNotificationInitializer.cs"),
                // Items (shared)
                ($"{SRC}/UI/Items/LeaderboardEntryUI.cs",    $"{DST}/UI/Items/LeaderboardEntryUI.cs"),
                // Builders
                ($"{SRC}/UI/Builders/BootUIBuilder.cs",              $"{DST}/UI/Builders/BootUIBuilder.cs"),
                ($"{SRC}/UI/Builders/DailyRewardPanelBuilder.cs",    $"{DST}/UI/Builders/DailyRewardPanelBuilder.cs"),
            });
        }

        // ───────────────────── Phase 9: Features ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 9A - Features Auth", false, 109)]
        static void Phase9A_Auth()
        {
            MoveBatch("Phase 9A - Auth", new[]
            {
                ($"{SRC}/Managers/LoginManager.cs",              $"{DST}/Features/Auth/LoginManager.cs"),
                ($"{SRC}/Managers/RegisterManager.cs",           $"{DST}/Features/Auth/RegisterManager.cs"),
                ($"{SRC}/Managers/AgeVerificationManager.cs",    $"{DST}/Features/Auth/AgeVerificationManager.cs"),
                ($"{SRC}/UI/Common/PasswordToggle.cs",           $"{DST}/Features/Auth/PasswordToggle.cs"),
                ($"{SRC}/UI/Common/ForgotPasswordPopup.cs",      $"{DST}/Features/Auth/ForgotPasswordPopup.cs"),
                ($"{SRC}/UI/Common/ForgotPasswordHoverEffect.cs", $"{DST}/Features/Auth/ForgotPasswordHoverEffect.cs"),
            });
        }

        [MenuItem("DigitPark/Migration/Phase 9B - Features MainMenu + Settings", false, 110)]
        static void Phase9B_MainMenuSettings()
        {
            MoveBatch("Phase 9B - MainMenu + Settings", new[]
            {
                ($"{SRC}/Managers/MainMenuManager.cs",   $"{DST}/Features/MainMenu/MainMenuManager.cs"),
                ($"{SRC}/Managers/SettingsManager.cs",   $"{DST}/Features/Settings/SettingsManager.cs"),
                ($"{SRC}/UI/ForceShowSettingsLabels.cs",  $"{DST}/Features/Settings/ForceShowSettingsLabels.cs"),
            });
        }

        [MenuItem("DigitPark/Migration/Phase 9C - Features Games", false, 111)]
        static void Phase9C_Games()
        {
            MoveBatch("Phase 9C - Games", new[]
            {
                // Games/Core (10 files)
                ($"{SRC}/Games/Core/CognitiveSprintManager.cs", $"{DST}/Features/Games/Core/CognitiveSprintManager.cs"),
                ($"{SRC}/Games/Core/GameContext.cs",            $"{DST}/Features/Games/Core/GameContext.cs"),
                ($"{SRC}/Games/Core/GameMode.cs",               $"{DST}/Features/Games/Core/GameMode.cs"),
                ($"{SRC}/Games/Core/GameSelectorManager.cs",    $"{DST}/Features/Games/Core/GameSelectorManager.cs"),
                ($"{SRC}/Games/Core/GameSessionManager.cs",     $"{DST}/Features/Games/Core/GameSessionManager.cs"),
                ($"{SRC}/Games/Core/GameType.cs",               $"{DST}/Features/Games/Core/GameType.cs"),
                ($"{SRC}/Games/Core/IMinigame.cs",              $"{DST}/Features/Games/Core/IMinigame.cs"),
                ($"{SRC}/Games/Core/MinigameBase.cs",           $"{DST}/Features/Games/Core/MinigameBase.cs"),
                ($"{SRC}/Games/Core/MinigameConfig.cs",         $"{DST}/Features/Games/Core/MinigameConfig.cs"),
                ($"{SRC}/Games/Core/MinigameResult.cs",         $"{DST}/Features/Games/Core/MinigameResult.cs"),
                // DigitRush
                ($"{SRC}/Managers/DigitRushController.cs",      $"{DST}/Features/Games/DigitRush/DigitRushController.cs"),
                ($"{SRC}/Controllers/EffectsController.cs",     $"{DST}/Features/Games/DigitRush/EffectsController.cs"),
                ($"{SRC}/Controllers/TileController.cs",        $"{DST}/Features/Games/DigitRush/TileController.cs"),
                // FlashTap
                ($"{SRC}/Games/FlashTap/FlashTapController.cs", $"{DST}/Features/Games/FlashTap/FlashTapController.cs"),
                ($"{SRC}/UI/FlashTapButton3D.cs",               $"{DST}/Features/Games/FlashTap/FlashTapButton3D.cs"),
                ($"{SRC}/UI/TapButtonEffect.cs",                $"{DST}/Features/Games/FlashTap/TapButtonEffect.cs"),
                // MemoryPairs
                ($"{SRC}/Games/MemoryPairs/MemoryPairsController.cs", $"{DST}/Features/Games/MemoryPairs/MemoryPairsController.cs"),
                ($"{SRC}/UI/Card3DEffect.cs",                         $"{DST}/Features/Games/MemoryPairs/Card3DEffect.cs"),
                // OddOneOut
                ($"{SRC}/Games/OddOneOut/OddOneOutController.cs", $"{DST}/Features/Games/OddOneOut/OddOneOutController.cs"),
                ($"{SRC}/UI/OddOneOutCell3D.cs",                  $"{DST}/Features/Games/OddOneOut/OddOneOutCell3D.cs"),
                ($"{SRC}/UI/Cell3DButton.cs",                     $"{DST}/Features/Games/OddOneOut/Cell3DButton.cs"),
                // QuickMath
                ($"{SRC}/Games/QuickMath/QuickMathController.cs", $"{DST}/Features/Games/QuickMath/QuickMathController.cs"),
                ($"{SRC}/UI/QuickMathCell3D.cs",                  $"{DST}/Features/Games/QuickMath/QuickMathCell3D.cs"),
                // Navigation
                ($"{SRC}/Managers/PlayModeSelectionManager.cs",   $"{DST}/Features/Games/Navigation/PlayModeSelectionManager.cs"),
                ($"{SRC}/Managers/MatchmakingManager.cs",         $"{DST}/Features/Games/Navigation/MatchmakingManager.cs"),
                ($"{SRC}/Monetization/Betting/BetSelectionPanel.cs", $"{DST}/Features/Games/Navigation/BetSelectionPanel.cs"),
                ($"{SRC}/UI/CountdownUI.cs",                      $"{DST}/Features/Games/Navigation/CountdownUI.cs"),
                ($"{SRC}/UI/GameCardEffect.cs",                   $"{DST}/Features/Games/Navigation/GameCardEffect.cs"),
                ($"{SRC}/UI/GridGlowPulse.cs",                    $"{DST}/Features/Games/Navigation/GridGlowPulse.cs"),
                // Results (+ 2 extracted from Animations)
                ($"{SRC}/Managers/ResultPanelManager.cs",         $"{DST}/Features/Games/Results/ResultPanelManager.cs"),
                ($"{SRC}/Managers/OnlineResultManager.cs",        $"{DST}/Features/Games/Results/OnlineResultManager.cs"),
                ($"{SRC}/UI/WinPanels/WinPanelController.cs",    $"{DST}/Features/Games/Results/WinPanelController.cs"),
                ($"{SRC}/UI/WinPanels/OnlineResultPanelController.cs", $"{DST}/Features/Games/Results/OnlineResultPanelController.cs"),
                ($"{SRC}/UI/WinPanels/SprintSummaryPanelController.cs", $"{DST}/Features/Games/Results/SprintSummaryPanelController.cs"),
                ($"{SRC}/UI/UISparkleEffect.cs",                  $"{DST}/Features/Games/Results/UISparkleEffect.cs"),
                // Extracted from Animations (break circular dep)
                ($"{SRC}/Animations/Components/WinCelebrationAnimator.cs", $"{DST}/Features/Games/Results/WinCelebrationAnimator.cs"),
                ($"{SRC}/Animations/Components/ComboVisualController.cs",  $"{DST}/Features/Games/Results/ComboVisualController.cs"),
            });
        }

        [MenuItem("DigitPark/Migration/Phase 9D - Features Social", false, 112)]
        static void Phase9D_Social()
        {
            MoveBatch("Phase 9D - Social", new[]
            {
                // Friends
                ($"{SRC}/Managers/FriendsManager.cs",              $"{DST}/Features/Social/Friends/FriendsManager.cs"),
                ($"{SRC}/Managers/FriendRequestsSceneManager.cs",  $"{DST}/Features/Social/Friends/FriendRequestsSceneManager.cs"),
                ($"{SRC}/Managers/SearchPlayersManager.cs",        $"{DST}/Features/Social/Friends/SearchPlayersManager.cs"),
                ($"{SRC}/UI/Items/Social/PlayerSearchItemUI.cs",   $"{DST}/Features/Social/Friends/PlayerSearchItemUI.cs"),
                // Profile
                ($"{SRC}/Managers/ProfileManager.cs",              $"{DST}/Features/Social/Profile/ProfileManager.cs"),
                ($"{SRC}/Managers/MatchHistorySceneManager.cs",    $"{DST}/Features/Social/Profile/MatchHistorySceneManager.cs"),
                ($"{SRC}/Managers/LeaderboardManager.cs",          $"{DST}/Features/Social/Profile/LeaderboardManager.cs"),
                // Notifications
                ($"{SRC}/Managers/NotificationsManager.cs",        $"{DST}/Features/Social/Notifications/NotificationsManager.cs"),
            });
        }

        [MenuItem("DigitPark/Migration/Phase 9E - Features Monetization", false, 113)]
        static void Phase9E_Monetization()
        {
            MoveBatch("Phase 9E - Monetization", new[]
            {
                // Currency
                ($"{SRC}/Monetization/Currency/CurrencyManager.cs",    $"{DST}/Features/Monetization/Currency/CurrencyManager.cs"),
                ($"{SRC}/Monetization/Currency/CurrencyDisplayUI.cs",  $"{DST}/Features/Monetization/Currency/CurrencyDisplayUI.cs"),
                // Shop
                ($"{SRC}/Monetization/Shop/ShopManager.cs",    $"{DST}/Features/Monetization/Shop/ShopManager.cs"),
                ($"{SRC}/Monetization/Shop/ShopItemData.cs",   $"{DST}/Features/Monetization/Shop/ShopItemData.cs"),
                ($"{SRC}/Monetization/Shop/ShopItemUI.cs",     $"{DST}/Features/Monetization/Shop/ShopItemUI.cs"),
                // Achievements
                ($"{SRC}/Managers/Monetization/AchievementsManager.cs",      $"{DST}/Features/Monetization/Achievements/AchievementsManager.cs"),
                ($"{SRC}/UI/Items/Achievements/AchievementItemUI.cs",        $"{DST}/Features/Monetization/Achievements/AchievementItemUI.cs"),
                ($"{SRC}/UI/Items/Achievements/CategoryHeaderUI.cs",         $"{DST}/Features/Monetization/Achievements/CategoryHeaderUI.cs"),
                ($"{SRC}/UI/Items/Achievements/TrophyCardUI.cs",             $"{DST}/Features/Monetization/Achievements/TrophyCardUI.cs"),
                // DailyMissions
                ($"{SRC}/Managers/Monetization/DailyMissionsManager.cs",     $"{DST}/Features/Monetization/DailyMissions/DailyMissionsManager.cs"),
                ($"{SRC}/UI/Items/DailyMissions/MissionCardUI.cs",           $"{DST}/Features/Monetization/DailyMissions/MissionCardUI.cs"),
                // DailyRewards
                ($"{SRC}/Managers/Monetization/DailyRewardsManager.cs",      $"{DST}/Features/Monetization/DailyRewards/DailyRewardsManager.cs"),
                ($"{SRC}/UI/Items/DailyRewards/RewardDayItemUI.cs",          $"{DST}/Features/Monetization/DailyRewards/RewardDayItemUI.cs"),
                // Premium
                ($"{SRC}/Managers/PremiumManager.cs",                        $"{DST}/Features/Monetization/Premium/PremiumManager.cs"),
                // Progression
                ($"{SRC}/Managers/Progression/MissionsManager.cs",           $"{DST}/Features/Monetization/Progression/MissionsManager.cs"),
                ($"{SRC}/Managers/Progression/PlayerProgressionSystem.cs",   $"{DST}/Features/Monetization/Progression/PlayerProgressionSystem.cs"),
            });
        }

        [MenuItem("DigitPark/Migration/Phase 9F - Features Tournaments", false, 114)]
        static void Phase9F_Tournaments()
        {
            MoveBatch("Phase 9F - Tournaments", new[]
            {
                ($"{SRC}/Managers/TournamentManager.cs",                     $"{DST}/Features/Tournaments/TournamentManager.cs"),
                ($"{SRC}/Managers/Tournaments/TournamentCreateManager.cs",   $"{DST}/Features/Tournaments/TournamentCreateManager.cs"),
                ($"{SRC}/Managers/Tournaments/TournamentLobbyManager.cs",   $"{DST}/Features/Tournaments/TournamentLobbyManager.cs"),
                ($"{SRC}/Managers/Tournaments/TournamentsBrowserManager.cs", $"{DST}/Features/Tournaments/TournamentsBrowserManager.cs"),
                ($"{SRC}/UI/Items/TournamentItemUI.cs",                      $"{DST}/Features/Tournaments/TournamentItemUI.cs"),
                ($"{SRC}/UI/Items/TournamentMyItemUI.cs",                    $"{DST}/Features/Tournaments/TournamentMyItemUI.cs"),
                ($"{SRC}/UI/Items/TournamentSearchItemUI.cs",                $"{DST}/Features/Tournaments/TournamentSearchItemUI.cs"),
                ($"{SRC}/UI/Items/Tournaments/ParticipantItemUI.cs",         $"{DST}/Features/Tournaments/ParticipantItemUI.cs"),
                ($"{SRC}/UI/Items/Tournaments/PrizeRowItemUI.cs",            $"{DST}/Features/Tournaments/PrizeRowItemUI.cs"),
                ($"{SRC}/UI/WinPanels/TournamentResultPanelController.cs",   $"{DST}/Features/Tournaments/TournamentResultPanelController.cs"),
            });
        }

        [MenuItem("DigitPark/Migration/Phase 9G - Features Onboarding", false, 115)]
        static void Phase9G_Onboarding()
        {
            MoveBatch("Phase 9G - Onboarding", new[]
            {
                ($"{SRC}/Managers/OnboardingManager.cs",              $"{DST}/Features/Onboarding/OnboardingManager.cs"),
                ($"{SRC}/Managers/CashBattleOnboardingManager.cs",    $"{DST}/Features/Onboarding/CashBattleOnboardingManager.cs"),
                ($"{SRC}/UI/Items/Onboarding/AvatarOptionItemUI.cs",  $"{DST}/Features/Onboarding/AvatarOptionItemUI.cs"),
                ($"{SRC}/UI/Items/Onboarding/StepDotItemUI.cs",       $"{DST}/Features/Onboarding/StepDotItemUI.cs"),
            });
        }

        [MenuItem("DigitPark/Migration/Phase 9H - Features CashBattle", false, 116)]
        static void Phase9H_CashBattle()
        {
            MoveBatch("Phase 9H - CashBattle", new[]
            {
                // Hub
                ($"{SRC}/Managers/CashBattleManager.cs",              $"{DST}/Features/CashBattle/Hub/CashBattleManager.cs"),
                ($"{SRC}/UI/CashBattle/CashBattle1v1Manager.cs",      $"{DST}/Features/CashBattle/Hub/CashBattle1v1Manager.cs"),
                ($"{SRC}/Managers/CashBattle/CashMatchmakingManager.cs", $"{DST}/Features/CashBattle/Hub/CashMatchmakingManager.cs"),
                ($"{SRC}/UI/CashBattle/LocationRestrictionOverlay.cs", $"{DST}/Features/CashBattle/Hub/LocationRestrictionOverlay.cs"),
                // History
                ($"{SRC}/CashBattle/History/CashHistorySceneController.cs", $"{DST}/Features/CashBattle/History/CashHistorySceneController.cs"),
                ($"{SRC}/CashBattle/History/HistoryData.cs",                $"{DST}/Features/CashBattle/History/HistoryData.cs"),
                ($"{SRC}/CashBattle/History/HistoryEntryItemUI.cs",         $"{DST}/Features/CashBattle/History/HistoryEntryItemUI.cs"),
                ($"{SRC}/CashBattle/History/HistoryManager.cs",             $"{DST}/Features/CashBattle/History/HistoryManager.cs"),
                // Profile
                ($"{SRC}/CashBattle/Profile/CashProfileSceneController.cs", $"{DST}/Features/CashBattle/Profile/CashProfileSceneController.cs"),
                // Wallet
                ($"{SRC}/CashBattle/Wallet/CashWalletSceneController.cs",  $"{DST}/Features/CashBattle/Wallet/CashWalletSceneController.cs"),
                ($"{SRC}/CashBattle/Wallet/DepositOptionUI.cs",            $"{DST}/Features/CashBattle/Wallet/DepositOptionUI.cs"),
                ($"{SRC}/CashBattle/Wallet/TransactionItemUI.cs",          $"{DST}/Features/CashBattle/Wallet/TransactionItemUI.cs"),
                ($"{SRC}/CashBattle/Wallet/WalletData.cs",                 $"{DST}/Features/CashBattle/Wallet/WalletData.cs"),
                ($"{SRC}/CashBattle/Wallet/WalletManager.cs",              $"{DST}/Features/CashBattle/Wallet/WalletManager.cs"),
                // Tournaments
                ($"{SRC}/Managers/CashBattle/CashTournamentCreateManager.cs",  $"{DST}/Features/CashBattle/Tournaments/CashTournamentCreateManager.cs"),
                ($"{SRC}/Managers/CashBattle/CashTournamentLobbyManager.cs",   $"{DST}/Features/CashBattle/Tournaments/CashTournamentLobbyManager.cs"),
                ($"{SRC}/Managers/CashBattle/CashTournamentsManager.cs",       $"{DST}/Features/CashBattle/Tournaments/CashTournamentsManager.cs"),
                ($"{SRC}/UI/CashBattle/CashTournamentResultsPanelController.cs", $"{DST}/Features/CashBattle/Tournaments/CashTournamentResultsPanelController.cs"),
                // Results
                ($"{SRC}/UI/WinPanels/CashBattleResultPanelController.cs", $"{DST}/Features/CashBattle/Results/CashBattleResultPanelController.cs"),
            });
        }

        // ───────────────────── Phase 10: DevTools ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 10 - DevTools", false, 117)]
        static void Phase10_DevTools()
        {
            MoveBatch("Phase 10 - DevTools", new[]
            {
                ($"{SRC}/Debug/DebugManager.cs",                $"{DST}/DevTools/DebugManager.cs"),
                ($"{SRC}/Debug/AchievementDebugPanel.cs",       $"{DST}/DevTools/AchievementDebugPanel.cs"),
                ($"{SRC}/Debug/SettingsTextRuntimeDebug.cs",    $"{DST}/DevTools/SettingsTextRuntimeDebug.cs"),
                ($"{SRC}/Tools/PremiumDebugController.cs",      $"{DST}/DevTools/PremiumDebugController.cs"),
            });
        }

        // ───────────────────── Phase 11: Cleanup Empty Folders ─────────────────────

        [MenuItem("DigitPark/Migration/Phase 11 - Cleanup Empty Folders", false, 118)]
        static void Phase11_Cleanup()
        {
            int deleted = 0;
            // Known empty folders + folders emptied by migration
            // We do multiple passes since deleting a subfolder may make parent empty
            for (int pass = 0; pass < 5; pass++)
            {
                bool anyDeleted = false;
                string[] allFolders = AssetDatabase.GetSubFolders(SRC);
                anyDeleted |= DeleteEmptyRecursive(SRC);
                if (!anyDeleted) break;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Migration] Phase 11: Limpieza de carpetas vacias completada");
            EditorUtility.DisplayDialog("Phase 11", "Limpieza de carpetas vacias completada", "OK");
        }

        static bool DeleteEmptyRecursive(string folder)
        {
            bool anyDeleted = false;
            string[] subfolders = AssetDatabase.GetSubFolders(folder);

            foreach (string sub in subfolders)
            {
                // Skip Runtime (our target) and Editor (untouched)
                string name = Path.GetFileName(sub);
                if (folder == SRC && (name == "Runtime" || name == "Editor"))
                    continue;

                anyDeleted |= DeleteEmptyRecursive(sub);
            }

            // Re-check after potentially deleting subfolders
            subfolders = AssetDatabase.GetSubFolders(folder);
            string[] assets = AssetDatabase.FindAssets("", new[] { folder });

            // Only delete if truly empty (no subfolders, no assets) and not root
            if (folder != SRC && subfolders.Length == 0)
            {
                // Check for any remaining assets in this exact folder
                string[] directAssets = Directory.GetFiles(folder.Replace("/", "\\"), "*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".meta")).ToArray();

                if (directAssets.Length == 0)
                {
                    AssetDatabase.DeleteAsset(folder);
                    Debug.Log($"[Migration] Deleted empty folder: {folder}");
                    anyDeleted = true;
                }
            }

            return anyDeleted;
        }

        // ───────────────────── Verification ─────────────────────

        [MenuItem("DigitPark/Migration/Verify - Count Files", false, 200)]
        static void Verify_CountFiles()
        {
            string[] runtimeFiles = AssetDatabase.FindAssets("t:MonoScript", new[] { DST });
            string[] editorFiles = AssetDatabase.FindAssets("t:MonoScript", new[] { $"{SRC}/Editor" });

            // Check for leftover files in old locations
            string[] allScripts = AssetDatabase.FindAssets("t:MonoScript", new[] { SRC });
            int leftovers = allScripts.Length - runtimeFiles.Length - editorFiles.Length;

            string msg = $"Runtime: {runtimeFiles.Length} scripts\n" +
                         $"Editor: {editorFiles.Length} scripts\n" +
                         $"Sobrantes en carpetas viejas: {leftovers}\n\n";

            if (leftovers > 0)
            {
                msg += "Archivos no migrados:\n";
                var runtimeGuids = new HashSet<string>(runtimeFiles);
                var editorGuids = new HashSet<string>(editorFiles);
                foreach (string guid in allScripts)
                {
                    if (!runtimeGuids.Contains(guid) && !editorGuids.Contains(guid))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        msg += $"  - {path}\n";
                    }
                }
            }

            Debug.Log($"[Migration] Verification:\n{msg}");
            EditorUtility.DisplayDialog("Verification", msg, "OK");
        }
    }
}
#endif
