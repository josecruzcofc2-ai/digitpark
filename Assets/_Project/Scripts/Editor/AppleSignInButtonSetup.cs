using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

namespace DigitPark.Editor
{
    /// <summary>
    /// Editor tool para agregar el botón de Apple Sign In a la escena Login
    /// Menú: DigitPark → Add Apple Sign In Button
    /// </summary>
    public class AppleSignInButtonSetup : EditorWindow
    {
        [MenuItem("DigitPark/Add Apple Sign In Button")]
        public static void ShowWindow()
        {
            GetWindow<AppleSignInButtonSetup>("Apple Sign In Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Apple Sign In Button Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este tool agregará un botón de Apple Sign In a la escena Login, " +
                "con el mismo estilo que el botón de Google.",
                MessageType.Info);

            GUILayout.Space(20);

            if (GUILayout.Button("Agregar Botón de Apple a Login", GUILayout.Height(40)))
            {
                AddAppleButtonToLoginScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Conectar Referencias en LoginManager", GUILayout.Height(30)))
            {
                ConnectReferences();
            }
        }

        private void AddAppleButtonToLoginScene()
        {
            // Abrir escena Login
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);

            // Buscar el botón de Google como referencia
            GameObject googleButton = FindInScene("GoogleButton");
            if (googleButton == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró GoogleButton en la escena Login.", "OK");
                return;
            }

            // Verificar si ya existe AppleButton
            GameObject existingApple = FindInScene("AppleButton");
            if (existingApple != null)
            {
                if (!EditorUtility.DisplayDialog("Botón Existente",
                    "Ya existe un AppleButton. ¿Deseas reemplazarlo?", "Sí", "No"))
                {
                    return;
                }
                DestroyImmediate(existingApple);
            }

            // Duplicar el botón de Google
            GameObject appleButton = Instantiate(googleButton, googleButton.transform.parent);
            appleButton.name = "AppleButton";

            // Posicionar debajo del botón de Google
            RectTransform googleRT = googleButton.GetComponent<RectTransform>();
            RectTransform appleRT = appleButton.GetComponent<RectTransform>();

            Vector2 googlePos = googleRT.anchoredPosition;
            float buttonHeight = googleRT.sizeDelta.y;
            float spacing = 15f;

            appleRT.anchoredPosition = new Vector2(googlePos.x, googlePos.y - buttonHeight - spacing);

            // Cambiar colores - Apple usa negro/blanco
            Image buttonImage = appleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Fondo negro para Apple
                buttonImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            }

            // Buscar y modificar el texto
            TextMeshProUGUI[] texts = appleButton.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.text.Contains("Google") || text.text.Contains("google") ||
                    text.text.Contains("Iniciar") || text.text.Contains("Sign"))
                {
                    text.text = " Sign in with Apple"; // Se localizará en runtime con AutoLocalizer
                    text.color = Color.white;

                    // Agregar AutoLocalizer si existe
                    AddAutoLocalizer(text.gameObject, "sign_in_apple");
                }
            }

            // Buscar el icono y cambiarlo
            Transform iconTransform = appleButton.transform.Find("Icon");
            if (iconTransform == null)
            {
                // Buscar en hijos
                foreach (Transform child in appleButton.transform)
                {
                    Image childImage = child.GetComponent<Image>();
                    if (childImage != null && child.name.ToLower().Contains("icon"))
                    {
                        iconTransform = child;
                        break;
                    }
                }
            }

            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    // Cambiar a color blanco (el icono de Apple será blanco sobre negro)
                    iconImage.color = Color.white;

                    // Nota: El sprite del icono de Apple se debe asignar manualmente
                    // o crear uno procedural
                }
            }

            // Agregar el logo de Apple como texto (usando símbolo de Apple si está disponible)
            // Crear un objeto para el logo de Apple
            CreateAppleLogo(appleButton);

            // Mover el botón en la jerarquía (después de Google)
            int googleIndex = googleButton.transform.GetSiblingIndex();
            appleButton.transform.SetSiblingIndex(googleIndex + 1);

            // Aplicar RoundedCorners si existe
            ApplyRoundedCorners(appleButton);

            // Marcar como dirty y guardar
            EditorUtility.SetDirty(appleButton);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[AppleSignIn] Botón de Apple agregado exitosamente!");
            EditorUtility.DisplayDialog("Completado",
                "Botón de Apple Sign In agregado.\n\n" +
                "Ahora haz clic en 'Conectar Referencias en LoginManager' para vincular el botón.",
                "OK");
        }

        private void CreateAppleLogo(GameObject appleButton)
        {
            // Buscar si ya hay un contenedor de icono
            Transform iconContainer = null;
            foreach (Transform child in appleButton.transform)
            {
                if (child.name.ToLower().Contains("icon") || child.name.ToLower().Contains("logo"))
                {
                    iconContainer = child;
                    break;
                }
            }

            if (iconContainer != null)
            {
                // Limpiar el icono existente y agregar texto con símbolo de Apple
                TextMeshProUGUI iconText = iconContainer.GetComponent<TextMeshProUGUI>();
                if (iconText == null)
                {
                    // Si hay una imagen, desactivarla y agregar texto
                    Image img = iconContainer.GetComponent<Image>();
                    if (img != null)
                    {
                        img.enabled = false;
                    }

                    // Agregar objeto de texto para el logo
                    GameObject logoTextObj = new GameObject("AppleLogo");
                    logoTextObj.transform.SetParent(iconContainer, false);

                    RectTransform rt = logoTextObj.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    iconText = logoTextObj.AddComponent<TextMeshProUGUI>();
                }

                if (iconText != null)
                {
                    iconText.text = ""; // Símbolo de Apple (requiere fuente que lo soporte)
                    iconText.fontSize = 28;
                    iconText.color = Color.white;
                    iconText.alignment = TextAlignmentOptions.Center;
                }
            }
        }

        private void ConnectReferences()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);

            // Buscar LoginManager
            GameObject loginManagerObj = FindInScene("LoginManager");
            if (loginManagerObj == null)
            {
                // Buscar en cualquier objeto con el componente
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.scene.IsValid())
                    {
                        var manager = obj.GetComponent<DigitPark.Managers.LoginManager>();
                        if (manager != null)
                        {
                            loginManagerObj = obj;
                            break;
                        }
                    }
                }
            }

            if (loginManagerObj == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró LoginManager en la escena.", "OK");
                return;
            }

            var loginManager = loginManagerObj.GetComponent<DigitPark.Managers.LoginManager>();
            if (loginManager == null)
            {
                EditorUtility.DisplayDialog("Error", "El objeto no tiene el componente LoginManager.", "OK");
                return;
            }

            // Buscar AppleButton
            GameObject appleButton = FindInScene("AppleButton");
            if (appleButton == null)
            {
                EditorUtility.DisplayDialog("Error", "No se encontró AppleButton. Primero agrégalo.", "OK");
                return;
            }

            // Conectar referencia
            SerializedObject so = new SerializedObject(loginManager);
            SerializedProperty appleButtonProp = so.FindProperty("appleButton");

            if (appleButtonProp != null)
            {
                Button btn = appleButton.GetComponent<Button>();
                appleButtonProp.objectReferenceValue = btn;
                so.ApplyModifiedProperties();

                EditorUtility.SetDirty(loginManager);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                Debug.Log("[AppleSignIn] Referencia conectada exitosamente!");
                EditorUtility.DisplayDialog("Completado", "AppleButton conectado a LoginManager.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error",
                    "No se encontró la propiedad 'appleButton' en LoginManager.\n" +
                    "Asegúrate de que el código esté actualizado.", "OK");
            }
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
            Type roundedCornersType = Type.GetType("DigitPark.UI.RoundedCorners, Assembly-CSharp");

            if (roundedCornersType == null)
            {
                Debug.LogWarning("[AppleSignIn] No se encontró RoundedCorners, saltando...");
                return;
            }

            var rounded = button.GetComponent(roundedCornersType);
            if (rounded == null)
            {
                rounded = button.AddComponent(roundedCornersType);
            }

            var so = new SerializedObject(rounded);
            so.FindProperty("cornerRadius").floatValue = 25f;
            so.FindProperty("showBottomBar").boolValue = true;
            so.FindProperty("bottomBarHeight").floatValue = 6f;
            so.FindProperty("bottomBarColor").colorValue = new Color(0, 0, 0, 0.8f);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(button);
        }

        private void AddAutoLocalizer(GameObject textObject, string localizationKey)
        {
            Type autoLocalizerType = Type.GetType("DigitPark.Localization.AutoLocalizer, Assembly-CSharp");

            if (autoLocalizerType == null)
            {
                Debug.LogWarning("[AppleSignIn] No se encontró AutoLocalizer, el texto no se localizará automáticamente.");
                return;
            }

            var localizer = textObject.GetComponent(autoLocalizerType);
            if (localizer == null)
            {
                localizer = textObject.AddComponent(autoLocalizerType);
            }

            var so = new SerializedObject(localizer);
            var keyProp = so.FindProperty("localizationKey");
            if (keyProp != null)
            {
                keyProp.stringValue = localizationKey;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(textObject);
        }
    }
}
