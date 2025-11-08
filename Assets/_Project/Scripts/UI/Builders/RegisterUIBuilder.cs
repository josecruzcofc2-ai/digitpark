using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Register programáticamente
    /// Formulario de creación de cuenta con validaciones
    /// </summary>
    public class RegisterUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private RegisterManager registerManager;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // IMPORTANTE: Crear RegisterManager PRIMERO
            CreateRegisterManager();

            canvas = UIFactory.CreateCanvas("MainCanvas");

            CreateBackground();
            CreateTitle();
            CreateRegisterForm();
            CreateButtons();
        }

        private void CreateRegisterManager()
        {
            GameObject managerObj = new GameObject("RegisterManager");
            registerManager = managerObj.AddComponent<RegisterManager>();
        }

        private void CreateBackground()
        {
            UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);
        }

        private void CreateTitle()
        {
            TextMeshProUGUI title = UIFactory.CreateTitle(canvas.transform, "Title", "¡CREA UNA CUENTA!");

            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, -120);
            titleRT.sizeDelta = new Vector2(700, 100);

            title.outlineWidth = 0.3f;
            title.outlineColor = UIFactory.ElectricBlue;

            registerManager.titleText = title;
        }

        private void CreateRegisterForm()
        {
            GameObject formPanel = new GameObject("RegisterFormPanel");
            formPanel.transform.SetParent(canvas.transform, false);

            RectTransform formRT = formPanel.AddComponent<RectTransform>();
            formRT.anchorMin = new Vector2(0.5f, 0.5f);
            formRT.anchorMax = new Vector2(0.5f, 0.5f);
            formRT.pivot = new Vector2(0.5f, 0.5f);
            formRT.anchoredPosition = new Vector2(0, -50);
            formRT.sizeDelta = new Vector2(550, 600);

            float yPos = 250;
            float spacing = 85;

            // Campo Nombre (Opcional)
            CreateInputField(formPanel.transform, "UsernameInput", "Nombre (Opcional)",
                ref yPos, spacing, out TMP_InputField usernameInput);
            usernameInput.characterLimit = 20;
            registerManager.usernameInput = usernameInput;

            // Campo Email
            CreateInputField(formPanel.transform, "EmailInput", "Correo Electrónico",
                ref yPos, spacing, out TMP_InputField emailInput);
            emailInput.contentType = TMP_InputField.ContentType.EmailAddress;
            emailInput.characterLimit = 50;
            registerManager.emailInput = emailInput;

            // Campo Contraseña
            CreateInputField(formPanel.transform, "PasswordInput", "Contraseña (mínimo 8 caracteres)",
                ref yPos, spacing, out TMP_InputField passwordInput);
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            passwordInput.characterLimit = 30;
            registerManager.passwordInput = passwordInput;

            // Campo Confirmar Contraseña
            CreateInputField(formPanel.transform, "ConfirmPasswordInput", "Confirmar Contraseña",
                ref yPos, spacing, out TMP_InputField confirmPasswordInput);
            confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
            confirmPasswordInput.characterLimit = 30;
            registerManager.confirmPasswordInput = confirmPasswordInput;

            // Toggle "Mantener Sesión Iniciada"
            yPos -= 20; // Espacio extra antes del toggle
            CreateRememberMeToggle(formPanel.transform, yPos);
        }

        private void CreateInputField(Transform parent, string name, string placeholder,
            ref float yPos, float spacing, out TMP_InputField inputField)
        {
            // Contenedor del input
            GameObject inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);

            RectTransform inputRT = inputObj.AddComponent<RectTransform>();
            inputRT.anchoredPosition = new Vector2(0, yPos);
            inputRT.sizeDelta = new Vector2(500, 55);

            // Imagen de fondo
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.15f, 0.25f);

            // Outline
            Outline outline = inputObj.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.3f);
            outline.effectDistance = new Vector2(2, -2);

            // TMP_InputField
            inputField = inputObj.AddComponent<TMP_InputField>();

            // Crear texto placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);

            RectTransform placeholderRT = placeholderObj.AddComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.sizeDelta = Vector2.zero;
            placeholderRT.offsetMin = new Vector2(15, 0);
            placeholderRT.offsetMax = new Vector2(-15, 0);

            TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 22;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            placeholderText.alignment = TMPro.TextAlignmentOptions.Left;

            // Crear texto del input
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.offsetMin = new Vector2(15, 0);
            textRT.offsetMax = new Vector2(-15, 0);

            TextMeshProUGUI inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 22;
            inputText.color = Color.white;
            inputText.alignment = TMPro.TextAlignmentOptions.Left;

            inputField.textViewport = inputRT;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;

            yPos -= spacing;
        }

        private void CreateRememberMeToggle(Transform parent, float yPos)
        {
            GameObject toggleObj = new GameObject("RememberMeToggle");
            toggleObj.transform.SetParent(parent, false);

            RectTransform toggleRT = toggleObj.AddComponent<RectTransform>();
            toggleRT.anchoredPosition = new Vector2(0, yPos);
            toggleRT.sizeDelta = new Vector2(400, 40);

            // Toggle component
            Toggle toggle = toggleObj.AddComponent<Toggle>();

            // Background del toggle
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);

            RectTransform bgRT = bgObj.AddComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.5f);
            bgRT.anchorMax = new Vector2(0, 0.5f);
            bgRT.pivot = new Vector2(0, 0.5f);
            bgRT.anchoredPosition = new Vector2(0, 0);
            bgRT.sizeDelta = new Vector2(30, 30);

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.25f);

            // Checkmark
            GameObject checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(bgObj.transform, false);

            RectTransform checkRT = checkmarkObj.AddComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.sizeDelta = Vector2.zero;
            checkRT.offsetMin = new Vector2(5, 5);
            checkRT.offsetMax = new Vector2(-5, -5);

            Image checkmarkImage = checkmarkObj.AddComponent<Image>();
            checkmarkImage.color = UIFactory.ElectricBlue;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);

            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.5f);
            labelRT.anchorMax = new Vector2(1, 0.5f);
            labelRT.pivot = new Vector2(0, 0.5f);
            labelRT.anchoredPosition = new Vector2(40, 0);
            labelRT.sizeDelta = new Vector2(-40, 30);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Mantener Sesión Iniciada";
            labelText.fontSize = 20;
            labelText.color = Color.white;
            labelText.alignment = TMPro.TextAlignmentOptions.Left;

            // Configurar toggle
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkmarkImage;
            toggle.isOn = true; // Por defecto activado

            registerManager.rememberMeToggle = toggle;
        }

        private void CreateButtons()
        {
            // Botón CONFIRMAR (crear cuenta)
            Button confirmButton = UIFactory.CreateButton(
                canvas.transform,
                "ConfirmButton",
                "CONFIRMAR",
                new Vector2(500, 70),
                UIFactory.BrightGreen
            );

            RectTransform confirmRT = confirmButton.GetComponent<RectTransform>();
            confirmRT.anchoredPosition = new Vector2(0, -450); // Subido de -510 a -450
            AddRoundedCorners(confirmButton.gameObject, 15f);

            registerManager.confirmButton = confirmButton;

            // Texto de error (oculto por defecto)
            TextMeshProUGUI errorText = UIFactory.CreateText(
                canvas.transform,
                "ErrorText",
                "",
                24,
                Color.red,
                TMPro.TextAlignmentOptions.Center
            );

            RectTransform errorRT = errorText.GetComponent<RectTransform>();
            errorRT.anchoredPosition = new Vector2(0, -390); // Encima del botón CONFIRMAR
            errorRT.sizeDelta = new Vector2(600, 50);
            errorText.gameObject.SetActive(false);

            registerManager.errorMessageText = errorText;

            // Panel de carga (oculto por defecto)
            GameObject loadingPanel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "LoadingPanel",
                new Vector2(200, 200),
                new Color(0.1f, 0.1f, 0.2f, 0.9f)
            );

            RectTransform loadingRT = loadingPanel.GetComponent<RectTransform>();
            loadingRT.anchoredPosition = Vector2.zero;
            loadingPanel.SetActive(false);

            // Texto "Cargando..."
            TextMeshProUGUI loadingText = UIFactory.CreateText(
                loadingPanel.transform,
                "LoadingText",
                "Creando cuenta...",
                28,
                Color.white,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform loadingTextRT = loadingText.GetComponent<RectTransform>();
            loadingTextRT.anchoredPosition = Vector2.zero;
            loadingTextRT.sizeDelta = new Vector2(180, 50);

            registerManager.loadingPanel = loadingPanel;

            Debug.Log("[RegisterUI] UI construida completamente");
        }

        private void AddRoundedCorners(GameObject target, float radius)
        {
            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                Outline outline = target.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = target.AddComponent<Outline>();
                }
                outline.effectColor = new Color(0, 0, 0, 0.2f);
                outline.effectDistance = new Vector2(1, -1);
            }
        }
    }
}
