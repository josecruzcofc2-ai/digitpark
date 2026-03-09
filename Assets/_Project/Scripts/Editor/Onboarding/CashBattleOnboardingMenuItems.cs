using UnityEditor;
using UnityEngine;

namespace DigitPark.Editor
{
    /// <summary>
    /// Utility menu items for Cash Battle Onboarding testing
    /// </summary>
    public static class CashBattleOnboardingMenuItems
    {
        [MenuItem("DigitPark/Debug/Onboarding/Reset CashBattle Onboarding", false, 173)]
        public static void ResetCashBattleOnboarding()
        {
            if (EditorUtility.DisplayDialog(
                "Reset Cash Battle Onboarding",
                "This will reset the Cash Battle Onboarding completion status. Continue?",
                "Yes", "Cancel"))
            {
                DigitPark.Managers.CashBattleOnboardingManager.ResetOnboarding();
                Debug.Log("✅ Cash Battle Onboarding reset successfully!");
            }
        }

        [MenuItem("DigitPark/Debug/Onboarding/Check Status", false, 174)]
        public static void CheckOnboardingStatus()
        {
            bool isComplete = DigitPark.Managers.CashBattleOnboardingManager.IsOnboardingComplete();
            bool isVerified = DigitPark.Managers.AgeVerificationManager.IsVerified();

            string message = $"Cash Battle Onboarding Status:\n\n" +
                           $"Onboarding Complete: {(isComplete ? "✅ Yes" : "❌ No")}\n" +
                           $"Age Verified: {(isVerified ? "✅ Yes" : "❌ No")}\n\n";

            if (!isComplete)
            {
                message += "User needs to complete Cash Battle Onboarding.";
            }
            else if (!isVerified)
            {
                message += "User needs Age Verification to access Cash Battle.";
            }
            else
            {
                message += "User can access Cash Battle Hub!";
            }

            EditorUtility.DisplayDialog("Onboarding Status", message, "OK");
            Debug.Log(message);
        }

        [MenuItem("DigitPark/Debug/Quick Scene Access/CashBattle Onboarding", false, 500)]
        public static void OpenCashBattleOnboardingScene()
        {
            string scenePath = "Assets/_Project/Scenes/Onboarding/CashBattleOnboarding.unity";

            if (System.IO.File.Exists(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                Debug.Log($"✅ Opened scene: {scenePath}");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Scene Not Found",
                    $"Could not find scene at:\n{scenePath}\n\n" +
                    "Please create the scene first or check the path.",
                    "OK");
            }
        }
    }
}
