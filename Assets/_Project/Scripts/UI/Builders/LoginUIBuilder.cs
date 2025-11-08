using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Login programáticamente
    /// </summary>
    public class LoginUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private LoginManager loginManager;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // IMPORTANTE: Crear LoginManager PRIMERO
            SetupLoginManager();

            canvas = UIFactory.CreateCanvas("MainCanvas");

            CreateBackground();
            CreateTitle();
            CreateLoginPanel();
        }

        private void SetupLoginManager()
        {
            GameObject managerObj = GameObject.Find("LoginManager");
            if (managerObj == null)
            {
                managerObj = new GameObject("LoginManager");
                loginManager = managerObj.AddComponent<LoginManager>();
            }
            else
            {
                loginManager = managerObj.GetComponent<LoginManager>();
            }
        }

        private void CreateBackground()
        {
            UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);
        }

        private void CreateTitle()
        {
            TextMeshProUGUI title = UIFactory.CreateTitle(canvas.transform, "Title", "DIGIT PARK");

            RectTransform titleRT = title.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0, -100);
            titleRT.sizeDelta = new Vector2(600, 100);

            title.outlineWidth = 0.3f;
            title.outlineColor = UIFactory.ElectricBlue;
        }

        private void CreateLoginPanel()
        {
            Vector2 buttonSize = new Vector2(480, 55);
            float buttonRadius = 15f;

            GameObject loginPanel = UIFactory.CreatePanelWithSize(
                canvas.transform,
                "LoginPanel",
                new Vector2(550, 650),
                new Color(0, 0, 0, 0)
            );

            RectTransform panelRT = loginPanel.GetComponent<RectTransform>();
            panelRT.anchoredPosition = new Vector2(0, -250);

            // Email Input
            TMP_InputField emailInput = UIFactory.CreateInputField(
                loginPanel.transform,
                "EmailInput",
                "Email",
                TMP_InputField.ContentType.EmailAddress
            );
            RectTransform emailRT = emailInput.GetComponent<RectTransform>();
            emailRT.sizeDelta = buttonSize;
            emailRT.anchoredPosition = new Vector2(0, 210);
            AddRoundedCorners(emailInput.gameObject, buttonRadius);
            loginManager.loginEmailInput = emailInput;

            // Password Input
            TMP_InputField passwordInput = UIFactory.CreateInputField(
                loginPanel.transform,
                "PasswordInput",
                "Contraseña",
                TMP_InputField.ContentType.Password
            );
            RectTransform passwordRT = passwordInput.GetComponent<RectTransform>();
            passwordRT.sizeDelta = buttonSize;
            passwordRT.anchoredPosition = new Vector2(0, 140);
            AddRoundedCorners(passwordInput.gameObject, buttonRadius);
            loginManager.loginPasswordInput = passwordInput;

            // Remember Me Toggle
            Toggle rememberToggle = UIFactory.CreateToggle(
                loginPanel.transform,
                "RememberMeToggle",
                "Recordarme"
            );
            RectTransform rememberRT = rememberToggle.GetComponent<RectTransform>();
            rememberRT.anchoredPosition = new Vector2(0, 80);
            loginManager.rememberMeToggle = rememberToggle;

            // Login Button
            Button loginBtn = UIFactory.CreateButton(
                loginPanel.transform,
                "LoginButton",
                "INICIAR SESIÓN",
                buttonSize,
                UIFactory.ElectricBlue
            );
            RectTransform loginRT = loginBtn.GetComponent<RectTransform>();
            loginRT.anchoredPosition = new Vector2(0, 20);
            AddRoundedCorners(loginBtn.gameObject, buttonRadius);
            loginManager.loginButton = loginBtn;

            // Google Login Button
            Button googleBtn = UIFactory.CreateButton(
                loginPanel.transform,
                "GoogleButton",
                "Continuar con Google",
                buttonSize,
                new Color(0.85f, 0.33f, 0.31f)
            );
            RectTransform googleRT = googleBtn.GetComponent<RectTransform>();
            googleRT.anchoredPosition = new Vector2(0, -45);
            AddRoundedCorners(googleBtn.gameObject, buttonRadius);
            loginManager.googleLoginButton = googleBtn;

            // Apple Login Button
            Button appleBtn = UIFactory.CreateButton(
                loginPanel.transform,
                "AppleButton",
                "Continuar con Apple",
                buttonSize,
                new Color(0.1f, 0.1f, 0.1f)
            );
            RectTransform appleRT = appleBtn.GetComponent<RectTransform>();
            appleRT.anchoredPosition = new Vector2(0, -110);
            AddRoundedCorners(appleBtn.gameObject, buttonRadius);
            loginManager.appleLoginButton = appleBtn;

            // Register Button
            Button registerBtn = UIFactory.CreateButton(
                loginPanel.transform,
                "RegisterButton",
                "Crear cuenta nueva",
                buttonSize,
                new Color(0.3f, 0.3f, 0.4f)
            );
            RectTransform registerRT = registerBtn.GetComponent<RectTransform>();
            registerRT.anchoredPosition = new Vector2(0, -175);
            AddRoundedCorners(registerBtn.gameObject, buttonRadius);
            loginManager.showRegisterButton = registerBtn;

            // Error Message Text
            TextMeshProUGUI errorText = UIFactory.CreateText(
                loginPanel.transform,
                "ErrorText",
                "",
                20,
                UIFactory.CoralRed,
                TMPro.TextAlignmentOptions.Center
            );
            RectTransform errorRT = errorText.GetComponent<RectTransform>();
            errorRT.anchoredPosition = new Vector2(0, -230);
            errorRT.sizeDelta = new Vector2(480, 60);
            loginManager.errorMessageText = errorText;

            // Loading Panel
            GameObject loadingPanel = UIFactory.CreateLoadingPanel(canvas.transform);
            loginManager.loadingPanel = loadingPanel;

            Debug.Log("[LoginUI] UI construida completamente");
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
