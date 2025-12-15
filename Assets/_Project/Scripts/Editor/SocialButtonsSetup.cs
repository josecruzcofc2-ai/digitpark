using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Configura los botones de Google y Apple Sign In según las guías oficiales
    ///
    /// GUÍAS OFICIALES:
    /// - Apple: https://developer.apple.com/design/human-interface-guidelines/sign-in-with-apple
    /// - Google: https://developers.google.com/identity/branding-guidelines
    ///
    /// DESCARGAR LOGOS:
    /// - Apple: https://developer.apple.com/design/resources/ (buscar "Sign in with Apple")
    /// - Google: https://developers.google.com/identity/branding-guidelines (descargar assets)
    /// </summary>
    public class SocialButtonsSetup : EditorWindow
    {
        // Colores oficiales
        private static readonly Color APPLE_BG_COLOR = Color.black;
        private static readonly Color APPLE_TEXT_COLOR = Color.white;
        private static readonly Color GOOGLE_BG_COLOR = Color.white;
        private static readonly Color GOOGLE_TEXT_COLOR = new Color(0.26f, 0.26f, 0.26f, 1f); // #434343
        private static readonly Color GOOGLE_BORDER_COLOR = new Color(0.85f, 0.85f, 0.85f, 1f); // Borde sutil

        [MenuItem("DigitPark/Setup Social Buttons (Google + Apple)")]
        public static void ShowWindow()
        {
            GetWindow<SocialButtonsSetup>("Social Buttons Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Configuración de Botones Sociales", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Este tool configura los botones de Google y Apple Sign In\n" +
                "según las guías oficiales de diseño.\n\n" +
                "IMPORTANTE: Después de ejecutar, debes:\n" +
                "1. Descargar los logos oficiales\n" +
                "2. Importarlos a Assets/_Project/Art/Icons/\n" +
                "3. Asignarlos manualmente a los Image de los iconos",
                MessageType.Info);

            GUILayout.Space(10);

            // Links de descarga
            EditorGUILayout.LabelField("Links para descargar logos:", EditorStyles.boldLabel);

            if (GUILayout.Button("Abrir: Apple Design Resources"))
            {
                Application.OpenURL("https://developer.apple.com/design/resources/");
            }

            if (GUILayout.Button("Abrir: Google Identity Branding"))
            {
                Application.OpenURL("https://developers.google.com/identity/branding-guidelines");
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Configurar Ambos Botones en Login", GUILayout.Height(40)))
            {
                SetupBothButtons();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Solo Configurar Botón de Apple", GUILayout.Height(30)))
            {
                SetupAppleButton();
            }

            if (GUILayout.Button("Solo Configurar Botón de Google", GUILayout.Height(30)))
            {
                SetupGoogleButton();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Conectar Referencias en LoginManager", GUILayout.Height(30)))
            {
                ConnectReferences();
            }

            GUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "ESTRUCTURA DE CADA BOTÓN:\n" +
                "├── Button (Image de fondo)\n" +
                "│   ├── IconContainer (para el logo)\n" +
                "│   │   └── Icon (Image - asignar sprite aquí)\n" +
                "│   └── Text (TextMeshProUGUI)\n",
                MessageType.None);
        }

        private static void SetupBothButtons()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);

            SetupAppleButtonInternal();
            SetupGoogleButtonInternal();
            ConnectReferencesInternal();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Completado",
                "Botones configurados correctamente.\n\n" +
                "Ahora debes:\n" +
                "1. Descargar los logos oficiales de Apple y Google\n" +
                "2. Importarlos a Assets/_Project/Art/Icons/\n" +
                "3. En la escena, buscar GoogleButton/IconContainer/Icon y AppleButton/IconContainer/Icon\n" +
                "4. Asignar los sprites correspondientes",
                "OK");
        }

        private static void SetupAppleButton()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);
            SetupAppleButtonInternal();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SocialButtons] Botón de Apple configurado");
        }

        private static void SetupGoogleButton()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);
            SetupGoogleButtonInternal();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SocialButtons] Botón de Google configurado");
        }

        private static void SetupAppleButtonInternal()
        {
            GameObject appleButton = FindInScene("AppleButton");

            if (appleButton == null)
            {
                // Crear el botón si no existe
                GameObject googleButton = FindInScene("GoogleButton");
                if (googleButton == null)
                {
                    Debug.LogError("[SocialButtons] No se encontró GoogleButton como referencia");
                    return;
                }

                appleButton = Object.Instantiate(googleButton, googleButton.transform.parent);
                appleButton.name = "AppleButton";

                // Posicionar debajo de Google
                RectTransform googleRT = googleButton.GetComponent<RectTransform>();
                RectTransform appleRT = appleButton.GetComponent<RectTransform>();
                appleRT.anchoredPosition = new Vector2(googleRT.anchoredPosition.x,
                    googleRT.anchoredPosition.y - googleRT.sizeDelta.y - 15f);

                // Mover después de Google en la jerarquía
                int googleIndex = googleButton.transform.GetSiblingIndex();
                appleButton.transform.SetSiblingIndex(googleIndex + 1);
            }

            // Configurar fondo negro (requisito de Apple)
            Image buttonImage = appleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = APPLE_BG_COLOR;
            }

            // Limpiar hijos existentes que no necesitamos
            CleanupButtonChildren(appleButton);

            // Crear estructura correcta
            CreateButtonStructure(appleButton, "Apple", APPLE_TEXT_COLOR, true);

            Debug.Log("[SocialButtons] Botón de Apple configurado con éxito");
        }

        private static void SetupGoogleButtonInternal()
        {
            GameObject googleButton = FindInScene("GoogleButton");

            if (googleButton == null)
            {
                Debug.LogError("[SocialButtons] No se encontró GoogleButton en la escena");
                return;
            }

            // Configurar fondo blanco (requisito de Google)
            Image buttonImage = googleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = GOOGLE_BG_COLOR;
            }

            // Agregar borde sutil (recomendado por Google)
            Outline outline = googleButton.GetComponent<Outline>();
            if (outline == null)
            {
                outline = googleButton.AddComponent<Outline>();
            }
            outline.effectColor = GOOGLE_BORDER_COLOR;
            outline.effectDistance = new Vector2(1, 1);

            // Limpiar hijos existentes
            CleanupButtonChildren(googleButton);

            // Crear estructura correcta
            CreateButtonStructure(googleButton, "Google", GOOGLE_TEXT_COLOR, false);

            Debug.Log("[SocialButtons] Botón de Google configurado con éxito");
        }

        private static void CleanupButtonChildren(GameObject button)
        {
            // Eliminar todos los hijos para empezar limpio
            for (int i = button.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(button.transform.GetChild(i).gameObject);
            }
        }

        private static void CreateButtonStructure(GameObject button, string provider, Color textColor, bool isApple)
        {
            RectTransform buttonRT = button.GetComponent<RectTransform>();
            float buttonWidth = buttonRT.sizeDelta.x;
            float buttonHeight = buttonRT.sizeDelta.y;

            // === ICON CONTAINER (izquierda) ===
            GameObject iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(button.transform, false);

            RectTransform iconContainerRT = iconContainer.AddComponent<RectTransform>();
            iconContainerRT.anchorMin = new Vector2(0, 0);
            iconContainerRT.anchorMax = new Vector2(0, 1);
            iconContainerRT.pivot = new Vector2(0, 0.5f);
            iconContainerRT.anchoredPosition = new Vector2(20, 0);
            iconContainerRT.sizeDelta = new Vector2(40, 40);

            // === ICON IMAGE ===
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(iconContainer.transform, false);

            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = isApple ? Color.white : Color.white; // El icono tendrá su propio color
            iconImage.preserveAspect = true;

            // Placeholder - mostrar que falta el icono
            // El usuario debe asignar el sprite manualmente

            // === TEXT ===
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(70, 0); // Dejar espacio para el icono
            textRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            // Texto según el proveedor (se localizará en runtime)
            if (isApple)
            {
                tmp.text = "Sign in with Apple";
            }
            else
            {
                tmp.text = "Sign in with Google";
            }

            tmp.fontSize = 24;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Normal;
            tmp.enableWordWrapping = false;

            // Agregar AutoLocalizer para traducción
            AddAutoLocalizer(textObj, isApple ? "sign_in_apple" : "sign_in_google");

            EditorUtility.SetDirty(button);
        }

        private static void ConnectReferences()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);
            ConnectReferencesInternal();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConnectReferencesInternal()
        {
            // Buscar LoginManager
            DigitPark.Managers.LoginManager loginManager = null;
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.scene.IsValid())
                {
                    var manager = obj.GetComponent<DigitPark.Managers.LoginManager>();
                    if (manager != null)
                    {
                        loginManager = manager;
                        break;
                    }
                }
            }

            if (loginManager == null)
            {
                Debug.LogError("[SocialButtons] No se encontró LoginManager en la escena");
                return;
            }

            // Buscar botones
            GameObject appleButton = FindInScene("AppleButton");
            GameObject googleButton = FindInScene("GoogleButton");

            SerializedObject so = new SerializedObject(loginManager);

            // Conectar Apple
            if (appleButton != null)
            {
                SerializedProperty appleButtonProp = so.FindProperty("appleButton");
                if (appleButtonProp != null)
                {
                    Button btn = appleButton.GetComponent<Button>();
                    appleButtonProp.objectReferenceValue = btn;
                    Debug.Log("[SocialButtons] AppleButton conectado a LoginManager");
                }
            }

            // Conectar Google
            if (googleButton != null)
            {
                SerializedProperty googleButtonProp = so.FindProperty("googleButton");
                if (googleButtonProp != null)
                {
                    Button btn = googleButton.GetComponent<Button>();
                    googleButtonProp.objectReferenceValue = btn;
                    Debug.Log("[SocialButtons] GoogleButton conectado a LoginManager");
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(loginManager);
        }

        private static GameObject FindInScene(string name)
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

        private static void AddAutoLocalizer(GameObject textObject, string localizationKey)
        {
            System.Type autoLocalizerType = System.Type.GetType("DigitPark.Localization.AutoLocalizer, Assembly-CSharp");

            if (autoLocalizerType == null)
            {
                Debug.LogWarning("[SocialButtons] AutoLocalizer no encontrado");
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
