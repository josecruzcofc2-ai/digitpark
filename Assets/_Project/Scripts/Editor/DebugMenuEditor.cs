using UnityEngine;
using UnityEditor;
using DigitPark.Managers;
using DigitPark.Themes;

namespace DigitPark.Editor
{
    /// <summary>
    /// Menu de herramientas de debug en el Editor de Unity
    /// Accesible desde: Tools > DigitPark Debug
    /// </summary>
    public static class DebugMenuEditor
    {
        #region Premium

        [MenuItem("Tools/DigitPark Debug/Premium/Unlock Create Tournaments")]
        private static void UnlockCreateTournaments()
        {
            if (PremiumManager.Instance != null)
            {
                PremiumManager.Instance.UnlockProduct(PremiumProduct.CreateTournaments);
                Debug.Log("[Debug] Create Tournaments desbloqueado");
            }
            else
            {
                SetPremiumPlayerPref("Premium_CreateTournaments", true);
            }
        }

        [MenuItem("Tools/DigitPark Debug/Premium/Unlock Cash Battle Create")]
        private static void UnlockCashBattleCreate()
        {
            if (PremiumManager.Instance != null)
            {
                PremiumManager.Instance.UnlockProduct(PremiumProduct.CashBattleCreate);
                Debug.Log("[Debug] Cash Battle Create desbloqueado");
            }
            else
            {
                SetPremiumPlayerPref("Premium_CashBattleCreate", true);
            }
        }

        [MenuItem("Tools/DigitPark Debug/Premium/Unlock Tournament Bundle")]
        private static void UnlockTournamentBundle()
        {
            if (PremiumManager.Instance != null)
            {
                PremiumManager.Instance.UnlockProduct(PremiumProduct.TournamentBundle);
                Debug.Log("[Debug] Tournament Bundle desbloqueado");
            }
            else
            {
                SetPremiumPlayerPref("Premium_CreateTournaments", true);
                SetPremiumPlayerPref("Premium_CashBattleCreate", true);
            }
        }

        [MenuItem("Tools/DigitPark Debug/Premium/Unlock Styles PRO")]
        private static void UnlockStylesPro()
        {
            if (PremiumManager.Instance != null)
            {
                PremiumManager.Instance.UnlockProduct(PremiumProduct.StylesPro);
                Debug.Log("[Debug] Styles PRO desbloqueado");
            }
            else
            {
                SetPremiumPlayerPref("Premium_StylesPro", true);
            }
        }

        [MenuItem("Tools/DigitPark Debug/Premium/Unlock ALL Premium")]
        private static void UnlockAllPremium()
        {
            UnlockTournamentBundle();
            UnlockStylesPro();
            Debug.Log("[Debug] Todos los productos premium desbloqueados");
        }

        [MenuItem("Tools/DigitPark Debug/Premium/Reset All Premium")]
        private static void ResetAllPremium()
        {
            PlayerPrefs.DeleteKey("Premium_CreateTournaments");
            PlayerPrefs.DeleteKey("Premium_CashBattleCreate");
            PlayerPrefs.DeleteKey("Premium_StylesPro");
            PlayerPrefs.Save();
            Debug.Log("[Debug] Estado premium reseteado");
        }

        private static void SetPremiumPlayerPref(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
            Debug.Log($"[Debug] PlayerPref '{key}' = {value}");
        }

        #endregion

        #region Themes

        [MenuItem("Tools/DigitPark Debug/Themes/Refresh Current Theme")]
        private static void RefreshTheme()
        {
            if (ThemeManager.Instance != null)
            {
                ThemeManager.Instance.RefreshTheme();
                Debug.Log("[Debug] Tema refrescado");
            }
            else
            {
                Debug.LogWarning("[Debug] ThemeManager no disponible (ejecutar en Play Mode)");
            }
        }

        [MenuItem("Tools/DigitPark Debug/Themes/Reset Theme to Default")]
        private static void ResetThemeToDefault()
        {
            PlayerPrefs.DeleteKey("SelectedTheme");
            PlayerPrefs.Save();
            Debug.Log("[Debug] Tema reseteado a default (reinicia la app)");
        }

        #endregion

        #region Player Data

        [MenuItem("Tools/DigitPark Debug/Player Data/Add 10 Wins")]
        private static void AddWins()
        {
            var playerData = Services.Firebase.AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData != null)
            {
                playerData.totalGamesWon += 10;
                playerData.totalGamesPlayed += 10;
                Debug.Log($"[Debug] Agregadas 10 victorias. Total: {playerData.totalGamesWon}");
            }
            else
            {
                Debug.LogWarning("[Debug] No hay usuario logueado");
            }
        }

        [MenuItem("Tools/DigitPark Debug/Player Data/Reset Best Time")]
        private static void ResetBestTime()
        {
            var playerData = Services.Firebase.AuthenticationService.Instance?.GetCurrentPlayerData();
            if (playerData != null)
            {
                playerData.bestTime = float.MaxValue;
                Debug.Log("[Debug] Best time reseteado");
            }
        }

        [MenuItem("Tools/DigitPark Debug/Player Data/Clear All Local Data")]
        private static void ClearAllLocalData()
        {
            if (EditorUtility.DisplayDialog("Limpiar Datos",
                "Esto eliminara TODOS los PlayerPrefs. Continuar?", "Si", "No"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[Debug] Todos los datos locales eliminados");
            }
        }

        #endregion

        #region Scenes

        [MenuItem("Tools/DigitPark Debug/Scenes/Go to MainMenu")]
        private static void GoToMainMenu()
        {
            if (Application.isPlaying)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.LogWarning("[Debug] Solo disponible en Play Mode");
            }
        }

        [MenuItem("Tools/DigitPark Debug/Scenes/Go to DigitRush")]
        private static void GoToDigitRush()
        {
            if (Application.isPlaying)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("DigitRush");
            }
        }

        [MenuItem("Tools/DigitPark Debug/Scenes/Reload Current Scene")]
        private static void ReloadScene()
        {
            if (Application.isPlaying)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
            }
        }

        #endregion

        #region Firebase

        [MenuItem("Tools/DigitPark Debug/Firebase/Log Test Analytics Event")]
        private static void LogTestAnalytics()
        {
            Services.Firebase.AnalyticsService.Instance?.LogScreenView("debug_test_screen", "DebugMenu");
            Debug.Log("[Debug] Evento de prueba enviado a Analytics");
        }

        [MenuItem("Tools/DigitPark Debug/Firebase/Show FCM Token")]
        private static void ShowFCMToken()
        {
            if (Services.Firebase.NotificationService.Instance != null)
            {
                string token = Services.Firebase.NotificationService.Instance.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    Debug.Log($"[Debug] FCM Token: {token}");
                    EditorGUIUtility.systemCopyBuffer = token;
                    Debug.Log("[Debug] Token copiado al clipboard");
                }
                else
                {
                    Debug.Log("[Debug] FCM Token no disponible");
                }
            }
            else
            {
                Debug.LogWarning("[Debug] NotificationService no disponible");
            }
        }

        #endregion

        #region Quick Actions

        [MenuItem("Tools/DigitPark Debug/Quick Setup for Testing")]
        private static void QuickSetupTesting()
        {
            // Desbloquear premium
            SetPremiumPlayerPref("Premium_CreateTournaments", true);
            SetPremiumPlayerPref("Premium_CashBattleCreate", true);
            SetPremiumPlayerPref("Premium_StylesPro", true);

            Debug.Log("[Debug] Configuracion rapida aplicada: Premium completo desbloqueado");
        }

        [MenuItem("Tools/DigitPark Debug/Reset Everything")]
        private static void ResetEverything()
        {
            if (EditorUtility.DisplayDialog("Reset Total",
                "Esto eliminara TODOS los datos guardados localmente, PlayerPrefs, etc. Continuar?",
                "Si, Resetear", "Cancelar"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[Debug] Todo reseteado");
            }
        }

        #endregion
    }
}
