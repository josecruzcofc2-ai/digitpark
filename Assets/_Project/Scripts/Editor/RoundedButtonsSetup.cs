using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool para aplicar bordes redondeados a botones específicos
    /// Menú: DigitPark → Setup Rounded Buttons
    /// </summary>
    public class RoundedButtonsSetup : EditorWindow
    {
        [MenuItem("DigitPark/Setup Rounded Buttons")]
        public static void ShowWindow()
        {
            GetWindow<RoundedButtonsSetup>("Rounded Buttons Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Aplicar Bordes Redondeados", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Aplicar a TODAS las Escenas", GUILayout.Height(40)))
            {
                ApplyToAllScenes();
            }

            GUILayout.Space(20);
            GUILayout.Label("Aplicar por Escena Individual:", EditorStyles.boldLabel);
            GUILayout.Space(5);

            if (GUILayout.Button("Game Scene - PlayAgainButton"))
            {
                ApplyToScene("Assets/_Project/Scenes/Game.unity", GetGameSceneButtons());
            }

            if (GUILayout.Button("Login Scene - Botones"))
            {
                ApplyToScene("Assets/_Project/Scenes/Login.unity", GetLoginSceneButtons());
            }

            if (GUILayout.Button("MainMenu Scene - 4 Botones"))
            {
                ApplyToScene("Assets/_Project/Scenes/MainMenu.unity", GetMainMenuSceneButtons());
            }

            if (GUILayout.Button("Register Scene - CreateAccountButton"))
            {
                ApplyToScene("Assets/_Project/Scenes/Register.unity", GetRegisterSceneButtons());
            }

            if (GUILayout.Button("Settings Scene - 3 Botones"))
            {
                ApplyToScene("Assets/_Project/Scenes/Settings.unity", GetSettingsSceneButtons());
            }
        }

        private void ApplyToAllScenes()
        {
            ApplyToScene("Assets/_Project/Scenes/Game.unity", GetGameSceneButtons());
            ApplyToScene("Assets/_Project/Scenes/Login.unity", GetLoginSceneButtons());
            ApplyToScene("Assets/_Project/Scenes/MainMenu.unity", GetMainMenuSceneButtons());
            ApplyToScene("Assets/_Project/Scenes/Register.unity", GetRegisterSceneButtons());
            ApplyToScene("Assets/_Project/Scenes/Settings.unity", GetSettingsSceneButtons());

            Debug.Log("[RoundedButtons] Todas las escenas actualizadas!");
            EditorUtility.DisplayDialog("Completado", "Bordes redondeados aplicados a todas las escenas.", "OK");
        }

        private List<string> GetGameSceneButtons()
        {
            return new List<string> { "PlayAgainButton" };
        }

        private List<string> GetLoginSceneButtons()
        {
            return new List<string> { "GoogleButton", "LoginButton", "RegisterButton" };
        }

        private List<string> GetMainMenuSceneButtons()
        {
            return new List<string> { "PlayButton", "ScoresButton", "TournamentsButton", "SettingsButton" };
        }

        private List<string> GetRegisterSceneButtons()
        {
            return new List<string> { "CreateAccountButton" };
        }

        private List<string> GetSettingsSceneButtons()
        {
            return new List<string> { "ChangeNameButton", "LogoutButton", "DeleteAccountButton" };
        }

        private void ApplyToScene(string scenePath, List<string> buttonNames)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            int applied = 0;
            foreach (string buttonName in buttonNames)
            {
                GameObject button = FindInScene(buttonName);
                if (button != null)
                {
                    ApplyRoundedCorners(button);
                    applied++;
                    Debug.Log($"[RoundedButtons] Aplicado a: {buttonName}");
                }
                else
                {
                    Debug.LogWarning($"[RoundedButtons] No encontrado: {buttonName}");
                }
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[RoundedButtons] {scenePath}: {applied}/{buttonNames.Count} botones actualizados");
        }

        private GameObject FindInScene(string name)
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name == name && obj.scene.IsValid())
                {
                    return obj;
                }
            }
            return null;
        }

        private void ApplyRoundedCorners(GameObject button)
        {
            // Buscar el tipo RoundedCorners dinámicamente
            Type roundedCornersType = Type.GetType("DigitPark.UI.RoundedCorners, Assembly-CSharp");

            if (roundedCornersType == null)
            {
                Debug.LogError("[RoundedButtons] No se encontró el tipo RoundedCorners. Asegúrate de que el script compile correctamente.");
                return;
            }

            // Agregar o actualizar RoundedCorners
            var rounded = button.GetComponent(roundedCornersType);
            if (rounded == null)
            {
                rounded = button.AddComponent(roundedCornersType);
            }

            // Configurar usando SerializedObject para que se guarde
            var so = new SerializedObject(rounded);
            so.FindProperty("cornerRadius").floatValue = 25f;
            so.FindProperty("showBottomBar").boolValue = true;
            so.FindProperty("bottomBarHeight").floatValue = 6f;
            so.FindProperty("bottomBarColor").colorValue = new Color(0, 0, 0, 0.8f);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(button);
        }
    }
}
