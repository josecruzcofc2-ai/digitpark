using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Boot programáticamente
    /// </summary>
    public class BootUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private BootManager bootManager;

        private void Awake()
        {
            BuildUI();
        }

        /// <summary>
        /// Construye toda la UI de la escena Boot
        /// </summary>
        private void BuildUI()
        {
            // IMPORTANTE: Crear BootManager PRIMERO antes de construir UI
            SetupBootManager();

            // Crear Canvas principal
            canvas = UIFactory.CreateCanvas("MainCanvas");

            // Fondo degradado
            CreateBackground();

            // Logo/Título
            CreateTitle();

            // Barra de progreso
            CreateLoadingBar();

            // Texto de estado
            CreateLoadingText();

            // Versión
            CreateVersionText();
        }

        /// <summary>
        /// Crea el fondo de la escena
        /// </summary>
        private void CreateBackground()
        {
            GameObject bg = UIFactory.CreatePanel(canvas.transform, "Background", UIFactory.DarkBG1);

            // Efecto de gradiente (simulado con dos imágenes)
            GameObject gradientTop = new GameObject("GradientTop");
            gradientTop.transform.SetParent(bg.transform, false);

            RectTransform rt = gradientTop.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = gradientTop.AddComponent<Image>();
            img.color = UIFactory.DarkBG2;
        }

        /// <summary>
        /// Crea el título principal
        /// </summary>
        private void CreateTitle()
        {
            TextMeshProUGUI title = UIFactory.CreateTitle(canvas.transform, "Title", "DIGIT PARK");

            // TMP ya tiene outline incorporado
            title.outlineWidth = 0.2f;
            title.outlineColor = UIFactory.ElectricBlue;
        }

        /// <summary>
        /// Crea la barra de progreso
        /// </summary>
        private void CreateLoadingBar()
        {
            // Container de la barra
            GameObject barContainer = new GameObject("LoadingBarContainer");
            barContainer.transform.SetParent(canvas.transform, false);

            RectTransform containerRT = barContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.3f);
            containerRT.anchorMax = new Vector2(0.5f, 0.3f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(700, 40);
            containerRT.anchoredPosition = Vector2.zero;

            // Fondo de la barra
            Image barBG = barContainer.AddComponent<Image>();
            barBG.color = new Color(0.15f, 0.15f, 0.25f);

            // Barra de progreso (fill)
            GameObject fillObj = new GameObject("LoadingBarFill");
            fillObj.transform.SetParent(barContainer.transform, false);

            RectTransform fillRT = fillObj.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0, 1);
            fillRT.pivot = Vector2.zero;
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = UIFactory.ElectricBlue;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0f;

            // Guardar referencia para el BootManager
            bootManager.loadingBar = fillImage;
        }

        /// <summary>
        /// Crea el texto de estado de carga
        /// </summary>
        private void CreateLoadingText()
        {
            TextMeshProUGUI loadingText = UIFactory.CreateText(
                canvas.transform,
                "LoadingText",
                "Iniciando...",
                28,
                Color.white,
                TextAlignmentOptions.Center
            );

            RectTransform rt = loadingText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.25f);
            rt.anchorMax = new Vector2(0.5f, 0.25f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(700, 50);

            bootManager.loadingText = loadingText;
        }

        /// <summary>
        /// Crea el texto de versión
        /// </summary>
        private void CreateVersionText()
        {
            TextMeshProUGUI versionText = UIFactory.CreateText(
                canvas.transform,
                "VersionText",
                $"v{Application.version}",
                20,
                Color.gray,
                TextAlignmentOptions.BottomRight
            );

            RectTransform rt = versionText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20, 20);
            rt.sizeDelta = new Vector2(200, 40);

            bootManager.versionText = versionText;
        }

        /// <summary>
        /// Configura el BootManager con las referencias de UI
        /// </summary>
        private void SetupBootManager()
        {
            // Crear BootManager si no existe
            GameObject managerObj = new GameObject("BootManager");
            bootManager = managerObj.AddComponent<BootManager>();

            Debug.Log("[BootUI] UI construida completamente");
        }
    }
}
