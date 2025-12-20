using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace DigitPark.Editor
{
    /// <summary>
    /// Configura los botones de Google y Apple Sign In.
    ///
    /// - Google: Fondo oscuro + Logo separado + Texto (facil de modificar)
    /// - Apple: Imagen completa oficial (ApleMsg.png)
    ///
    /// ICONOS REQUERIDOS en Assets/_Project/Art/Icons/:
    /// - google_logo.png (solo el logo G multicolor)
    /// - ApleMsg.png (boton completo de Apple negro)
    /// </summary>
    public class SocialButtonsSetup : EditorWindow
    {
        // Rutas de los iconos
        private const string GOOGLE_LOGO_PATH = "Assets/_Project/Art/Icons/google_logo.png";
        private const string APPLE_BUTTON_PATH = "Assets/_Project/Art/Icons/ApleMsg.png";

        // Colores para Google (tema oscuro oficial)
        private static readonly Color GOOGLE_BG_COLOR = new Color(0.075f, 0.075f, 0.078f, 1f); // #131314
        private static readonly Color GOOGLE_TEXT_COLOR = new Color(0.89f, 0.89f, 0.89f, 1f); // #E3E3E3
        private static readonly Color GOOGLE_BORDER_COLOR = new Color(0.557f, 0.569f, 0.561f, 1f); // #8E918F

        [MenuItem("DigitPark/Setup Social Buttons (Google + Apple)")]
        public static void ShowWindow()
        {
            GetWindow<SocialButtonsSetup>("Social Buttons Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Configuracion de Botones Sociales", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "GOOGLE: Fondo oscuro + Logo + Texto separado\n" +
                "(Facil de modificar y traducir)\n\n" +
                "APPLE: Imagen completa oficial\n" +
                "(Cumple con guias de Apple)",
                MessageType.Info);

            GUILayout.Space(10);

            // Verificar iconos
            bool googleLogoExists = AssetDatabase.LoadAssetAtPath<Sprite>(GOOGLE_LOGO_PATH) != null;
            bool appleExists = AssetDatabase.LoadAssetAtPath<Sprite>(APPLE_BUTTON_PATH) != null;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Estado de Iconos:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Google Logo:", GUILayout.Width(80));
            EditorGUILayout.LabelField(googleLogoExists ? "OK" : "FALTA", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Apple Button:", GUILayout.Width(80));
            EditorGUILayout.LabelField(appleExists ? "OK" : "FALTA", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(20);

            GUI.enabled = googleLogoExists && appleExists;
            if (GUILayout.Button("Configurar Ambos Botones en Login", GUILayout.Height(40)))
            {
                SetupBothButtons();
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            GUI.enabled = appleExists;
            if (GUILayout.Button("Solo Configurar Boton de Apple", GUILayout.Height(30)))
            {
                SetupAppleButton();
            }
            GUI.enabled = true;

            GUI.enabled = googleLogoExists;
            if (GUILayout.Button("Solo Configurar Boton de Google", GUILayout.Height(30)))
            {
                SetupGoogleButton();
            }
            GUI.enabled = true;

            GUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "ESTRUCTURA FINAL:\n\n" +
                "GoogleButton (500x80, fondo #131314)\n" +
                "  - IconContainer (logo G)\n" +
                "  - Text (TextMeshPro)\n\n" +
                "AppleButton (500x80)\n" +
                "  - Sin hijos (imagen completa)",
                MessageType.None);
        }

        private static void SetupBothButtons()
        {
            // Verificar si la escena ya esta abierta
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.Scene scene;

            if (currentScene.path == "Assets/_Project/Scenes/Login.unity")
            {
                scene = currentScene;
            }
            else
            {
                scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);
            }

            SetupGoogleButtonInternal();
            SetupAppleButtonInternal();
            ConnectReferencesInternal();

            // Forzar guardado
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Completado",
                "Botones configurados y guardados:\n\n" +
                "- Google: Fondo oscuro + Logo + Texto\n" +
                "- Apple: Imagen completa oficial\n\n" +
                "La escena ha sido guardada.",
                "OK");
        }

        private static void SetupAppleButton()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.Scene scene;

            if (currentScene.path == "Assets/_Project/Scenes/Login.unity")
            {
                scene = currentScene;
            }
            else
            {
                scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);
            }

            SetupAppleButtonInternal();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[SocialButtons] Boton de Apple configurado y guardado");
        }

        private static void SetupGoogleButton()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.Scene scene;

            if (currentScene.path == "Assets/_Project/Scenes/Login.unity")
            {
                scene = currentScene;
            }
            else
            {
                scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Login.unity", OpenSceneMode.Single);
            }

            SetupGoogleButtonInternal();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[SocialButtons] Boton de Google configurado y guardado");
        }

        // ==================== GOOGLE BUTTON (Logo + Texto) ====================
        private static void SetupGoogleButtonInternal()
        {
            GameObject googleButton = FindInScene("GoogleButton");

            if (googleButton == null)
            {
                Debug.LogError("[SocialButtons] No se encontro GoogleButton en la escena");
                return;
            }

            // Limpiar hijos existentes
            CleanupButtonChildren(googleButton);

            // Configurar fondo oscuro (tema oficial de Google)
            Image buttonImage = googleButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = googleButton.AddComponent<Image>();
            }
            buttonImage.color = GOOGLE_BG_COLOR;
            buttonImage.sprite = null; // Sin sprite, solo color de fondo
            buttonImage.raycastTarget = true;

            // Agregar borde sutil (recomendado por Google para tema oscuro)
            Outline outline = googleButton.GetComponent<Outline>();
            if (outline == null)
            {
                outline = googleButton.AddComponent<Outline>();
            }
            outline.effectColor = GOOGLE_BORDER_COLOR;
            outline.effectDistance = new Vector2(1, 1);

            // Configurar boton
            Button btn = googleButton.GetComponent<Button>();
            if (btn == null)
            {
                btn = googleButton.AddComponent<Button>();
            }
            btn.targetGraphic = buttonImage;
            btn.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = btn.colors;
            colors.normalColor = GOOGLE_BG_COLOR;
            colors.highlightedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            colors.selectedColor = GOOGLE_BG_COLOR;
            btn.colors = colors;

            // Asegurar tamano 500x80
            RectTransform buttonRT = googleButton.GetComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(500, 80);

            // Crear estructura: IconContainer + Text
            CreateGoogleButtonStructure(googleButton);

            // IMPORTANTE: Remover ThemeApplier - los botones sociales tienen branding fijo
            RemoveThemeApplier(googleButton);

            EditorUtility.SetDirty(googleButton);
            Debug.Log("[SocialButtons] Boton de Google configurado con logo + texto");
        }

        private static void CreateGoogleButtonStructure(GameObject button)
        {
            // === ICON CONTAINER (izquierda) ===
            GameObject iconContainer = new GameObject("IconContainer");
            iconContainer.transform.SetParent(button.transform, false);

            RectTransform iconContainerRT = iconContainer.AddComponent<RectTransform>();
            iconContainerRT.anchorMin = new Vector2(0, 0.5f);
            iconContainerRT.anchorMax = new Vector2(0, 0.5f);
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
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            // Cargar logo de Google
            Sprite googleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GOOGLE_LOGO_PATH);
            if (googleSprite != null)
            {
                iconImage.sprite = googleSprite;
                iconImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning("[SocialButtons] No se encontro google_logo.png");
            }

            // === TEXT ===
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(70, 0); // Espacio para el icono
            textRT.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Sign in with Google";
            tmp.fontSize = 24;
            tmp.color = GOOGLE_TEXT_COLOR;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Normal;
            tmp.enableWordWrapping = false;

            // Agregar AutoLocalizer
            AddAutoLocalizer(textObj, "sign_in_google");
        }

        // ==================== APPLE BUTTON (Imagen completa) ====================
        private static void SetupAppleButtonInternal()
        {
            GameObject appleButton = FindInScene("AppleButton");

            if (appleButton == null)
            {
                // Crear el boton si no existe
                GameObject googleButton = FindInScene("GoogleButton");
                if (googleButton == null)
                {
                    Debug.LogError("[SocialButtons] No se encontro GoogleButton como referencia");
                    return;
                }

                // Crear nuevo GameObject en lugar de instanciar
                appleButton = new GameObject("AppleButton");
                appleButton.transform.SetParent(googleButton.transform.parent, false);

                // Registrar con Undo para que se guarde correctamente
                Undo.RegisterCreatedObjectUndo(appleButton, "Create AppleButton");

                // Agregar RectTransform
                RectTransform appleRT = appleButton.AddComponent<RectTransform>();
                RectTransform googleRT = googleButton.GetComponent<RectTransform>();

                // Copiar configuracion de RectTransform
                appleRT.anchorMin = googleRT.anchorMin;
                appleRT.anchorMax = googleRT.anchorMax;
                appleRT.pivot = googleRT.pivot;
                appleRT.sizeDelta = new Vector2(500, 80);
                appleRT.anchoredPosition = new Vector2(googleRT.anchoredPosition.x,
                    googleRT.anchoredPosition.y - googleRT.sizeDelta.y - 15f);

                // Posicionar despues de Google en la jerarquia
                int googleIndex = googleButton.transform.GetSiblingIndex();
                appleButton.transform.SetSiblingIndex(googleIndex + 1);

                Debug.Log("[SocialButtons] AppleButton creado correctamente");
            }

            // Limpiar hijos existentes
            CleanupButtonChildren(appleButton);

            // Cargar sprite de Apple
            Sprite appleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(APPLE_BUTTON_PATH);
            if (appleSprite == null)
            {
                Debug.LogError("[SocialButtons] No se encontro ApleMsg.png");
                return;
            }

            // Configurar imagen
            Image buttonImage = appleButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = appleButton.AddComponent<Image>();
            }
            buttonImage.sprite = appleSprite;
            buttonImage.color = Color.white;
            buttonImage.type = Image.Type.Simple;
            buttonImage.preserveAspect = false; // Llenar el boton completo
            buttonImage.raycastTarget = true;

            // Remover Outline si existe
            Outline outline = appleButton.GetComponent<Outline>();
            if (outline != null)
            {
                Object.DestroyImmediate(outline);
            }

            // IMPORTANTE: Remover ThemeApplier - los botones sociales tienen branding fijo
            RemoveThemeApplier(appleButton);

            // Configurar boton
            Button btn = appleButton.GetComponent<Button>();
            if (btn == null)
            {
                btn = appleButton.AddComponent<Button>();
            }
            btn.targetGraphic = buttonImage;
            btn.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.selectedColor = Color.white;
            btn.colors = colors;

            // Asegurar tamano 500x80
            RectTransform rt = appleButton.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 80);

            EditorUtility.SetDirty(appleButton);
            Debug.Log("[SocialButtons] Boton de Apple configurado con imagen completa");
        }

        // ==================== UTILIDADES ====================
        private static void CleanupButtonChildren(GameObject button)
        {
            for (int i = button.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(button.transform.GetChild(i).gameObject);
            }
        }

        private static void ConnectReferencesInternal()
        {
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
                Debug.LogError("[SocialButtons] No se encontro LoginManager en la escena");
                return;
            }

            GameObject appleButton = FindInScene("AppleButton");
            GameObject googleButton = FindInScene("GoogleButton");

            SerializedObject so = new SerializedObject(loginManager);

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

        private static void RemoveThemeApplier(GameObject obj)
        {
            // Buscar y remover ThemeApplier
            System.Type themeApplierType = System.Type.GetType("DigitPark.Themes.ThemeApplier, Assembly-CSharp");

            if (themeApplierType != null)
            {
                var themeApplier = obj.GetComponent(themeApplierType);
                if (themeApplier != null)
                {
                    Object.DestroyImmediate(themeApplier);
                    Debug.Log($"[SocialButtons] ThemeApplier removido de {obj.name}");
                }
            }

            // También remover RoundedCorners si existe (no compatible con imagen completa)
            System.Type roundedCornersType = System.Type.GetType("DigitPark.UI.RoundedCorners, Assembly-CSharp");

            if (roundedCornersType != null)
            {
                var roundedCorners = obj.GetComponent(roundedCornersType);
                if (roundedCorners != null)
                {
                    Object.DestroyImmediate(roundedCorners);
                    Debug.Log($"[SocialButtons] RoundedCorners removido de {obj.name}");
                }
            }
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
