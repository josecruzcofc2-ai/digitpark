using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitPark.Managers;
using DigitPark.Themes;
using DigitPark.Localization;

namespace DigitPark.UI
{
    /// <summary>
    /// Construye la UI de la escena Boot programáticamente
    /// Ahora con soporte para temas y animaciones premium
    /// </summary>
    public class BootUIBuilder : MonoBehaviour
    {
        private Canvas canvas;
        private BootManager bootManager;
        private BootAnimator bootAnimator;
        private ThemeData theme;

        // Referencias para animaciones
        private RectTransform logoContainer;
        private CanvasGroup logoCanvasGroup;
        private Image loadingBarGlow;
        private ParticleSystem neonParticles;

        private void Awake()
        {
            // Obtener tema actual (o usar colores por defecto)
            LoadTheme();

            BuildUI();

            // Iniciar animaciones
            if (bootAnimator != null)
            {
                bootAnimator.StartBootAnimation();
            }
        }

        /// <summary>
        /// Carga el tema actual del ThemeManager
        /// </summary>
        private void LoadTheme()
        {
            if (ThemeManager.Instance != null && ThemeManager.Instance.CurrentTheme != null)
            {
                theme = ThemeManager.Instance.CurrentTheme;
                Debug.Log($"[BootUI] Usando tema: {theme.themeName}");
            }
            else
            {
                Debug.Log("[BootUI] ThemeManager no disponible, usando colores por defecto");
            }
        }

        /// <summary>
        /// Obtiene un color del tema o usa el valor por defecto
        /// </summary>
        private Color GetThemeColor(System.Func<ThemeData, Color> themeGetter, Color defaultColor)
        {
            return theme != null ? themeGetter(theme) : defaultColor;
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

            // Fondo con gradiente temático
            CreateBackground();

            // Sistema de partículas de neón
            CreateNeonParticles();

            // Logo/Título con animación
            CreateAnimatedTitle();

            // Subtítulo decorativo
            CreateSubtitle();

            // Barra de progreso con glow
            CreateLoadingBar();

            // Texto de estado con typewriter
            CreateLoadingText();

            // Versión
            CreateVersionText();

            // Decoraciones adicionales
            CreateDecorations();

            // Configurar BootAnimator
            SetupBootAnimator();
        }

        /// <summary>
        /// Crea el fondo de la escena con gradiente temático
        /// </summary>
        private void CreateBackground()
        {
            Color bgColor = GetThemeColor(t => t.primaryBackground, new Color(0.03f, 0.03f, 0.08f));
            Color bgColor2 = GetThemeColor(t => t.secondaryBackground, new Color(0.08f, 0.08f, 0.15f));

            GameObject bg = UIFactory.CreatePanel(canvas.transform, "Background", bgColor);

            // Efecto de gradiente superior
            GameObject gradientTop = new GameObject("GradientTop");
            gradientTop.transform.SetParent(bg.transform, false);

            RectTransform rt = gradientTop.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.6f);
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = gradientTop.AddComponent<Image>();
            img.color = bgColor2;

            // Efecto de viñeta en las esquinas
            CreateVignetteEffect(bg.transform);
        }

        /// <summary>
        /// Crea efecto de viñeta para las esquinas
        /// </summary>
        private void CreateVignetteEffect(Transform parent)
        {
            GameObject vignette = new GameObject("Vignette");
            vignette.transform.SetParent(parent, false);

            RectTransform rt = vignette.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = vignette.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.3f);
            img.raycastTarget = false;
        }

        /// <summary>
        /// Crea el sistema de partículas de neón flotantes
        /// </summary>
        private void CreateNeonParticles()
        {
            GameObject particlesObj = new GameObject("NeonParticles");
            particlesObj.transform.SetParent(canvas.transform, false);

            // Posicionar en el centro
            RectTransform rt = particlesObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            neonParticles = particlesObj.AddComponent<ParticleSystem>();

            // Configurar partículas
            var main = neonParticles.main;
            main.loop = true;
            main.startLifetime = 4f;
            main.startSpeed = 20f;
            main.startSize = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = false;

            // Colores del tema
            Color accent1 = GetThemeColor(t => t.primaryAccent, new Color(0f, 1f, 1f));
            Color accent2 = GetThemeColor(t => t.secondaryAccent, new Color(1f, 0f, 0.5f));
            main.startColor = new ParticleSystem.MinMaxGradient(accent1, accent2);

            // Emisión
            var emission = neonParticles.emission;
            emission.rateOverTime = 8f;

            // Forma (área rectangular grande)
            var shape = neonParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(800, 1600, 1);

            // Color sobre tiempo (fade in/out)
            var colorOverLifetime = neonParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.6f, 0.2f),
                    new GradientAlphaKey(0.6f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Tamaño sobre tiempo (shrink)
            var sizeOverLifetime = neonParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0.3f)
            ));

            // Velocidad sobre tiempo (desacelerar)
            // Todas las curvas deben estar en el mismo modo (TwoConstants)
            var velocityOverLifetime = neonParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-5f, 5f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-10f, 10f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            // Renderer
            var renderer = particlesObj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 1;

            // Usar material por defecto de partículas
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.SetColor("_Color", accent1);
        }

        /// <summary>
        /// Crea el título principal con soporte para animación
        /// </summary>
        private void CreateAnimatedTitle()
        {
            // Container para el logo (para animaciones)
            GameObject logoContainerObj = new GameObject("LogoContainer");
            logoContainerObj.transform.SetParent(canvas.transform, false);

            logoContainer = logoContainerObj.AddComponent<RectTransform>();
            logoContainer.anchorMin = new Vector2(0.5f, 0.58f);
            logoContainer.anchorMax = new Vector2(0.5f, 0.58f);
            logoContainer.pivot = new Vector2(0.5f, 0.5f);
            logoContainer.sizeDelta = new Vector2(700, 200);
            logoContainer.anchoredPosition = Vector2.zero;

            logoCanvasGroup = logoContainerObj.AddComponent<CanvasGroup>();
            logoCanvasGroup.alpha = 0f; // Comienza invisible para la animación

            // Título principal
            Color titleColor = GetThemeColor(t => t.textTitle, new Color(0f, 1f, 1f));
            Color accentColor = GetThemeColor(t => t.primaryAccent, new Color(0f, 1f, 1f));

            TextMeshProUGUI title = UIFactory.CreateTitle(logoContainer, "Title", "DIGIT PARK");
            title.fontSize = 72;
            title.color = titleColor;
            title.outlineWidth = 0.25f;
            title.outlineColor = accentColor;

            // Efecto glow detrás del título
            CreateTitleGlow(logoContainer, accentColor);
        }

        /// <summary>
        /// Crea efecto glow detrás del título
        /// </summary>
        private void CreateTitleGlow(RectTransform parent, Color glowColor)
        {
            GameObject glowObj = new GameObject("TitleGlow");
            glowObj.transform.SetParent(parent, false);
            glowObj.transform.SetAsFirstSibling(); // Detrás del título

            RectTransform rt = glowObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = new Vector2(100, 50);
            rt.anchoredPosition = Vector2.zero;

            Image img = glowObj.AddComponent<Image>();
            Color glowWithAlpha = glowColor;
            glowWithAlpha.a = 0.15f;
            img.color = glowWithAlpha;
            img.raycastTarget = false;
        }

        /// <summary>
        /// Crea subtítulos decorativos (localizados)
        /// </summary>
        private void CreateSubtitle()
        {
            Color textSecondary = GetThemeColor(t => t.textSecondary, new Color(0.7f, 0.7f, 0.7f));
            Color accentColor = GetThemeColor(t => t.primaryAccent, new Color(0f, 1f, 1f));

            // Obtener textos localizados
            string subtitleText = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("boot_subtitle")
                : "ARCADE EXPERIENCE";
            string subtitle2Text = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("boot_subtitle2")
                : "TRAIN YOUR MIND";

            // Subtítulo 1: ARCADE EXPERIENCE (localizado)
            TextMeshProUGUI subtitle = UIFactory.CreateText(
                canvas.transform,
                "Subtitle",
                subtitleText,
                16,
                textSecondary,
                TextAlignmentOptions.Center
            );

            subtitle.characterSpacing = 6f;
            subtitle.fontStyle = FontStyles.Bold;

            RectTransform rt = subtitle.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.40f);
            rt.anchorMax = new Vector2(0.5f, 0.40f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(500, 30);

            // Subtítulo 2: Habilidad Mental (localizado)
            TextMeshProUGUI subtitle2 = UIFactory.CreateText(
                canvas.transform,
                "Subtitle2",
                subtitle2Text,
                14,
                accentColor,
                TextAlignmentOptions.Center
            );

            subtitle2.characterSpacing = 4f;
            subtitle2.fontStyle = FontStyles.Italic;
            subtitle2.alpha = 0.7f;

            RectTransform rt2 = subtitle2.GetComponent<RectTransform>();
            rt2.anchorMin = new Vector2(0.5f, 0.36f);
            rt2.anchorMax = new Vector2(0.5f, 0.36f);
            rt2.pivot = new Vector2(0.5f, 0.5f);
            rt2.anchoredPosition = Vector2.zero;
            rt2.sizeDelta = new Vector2(500, 25);
        }

        /// <summary>
        /// Crea la barra de progreso con efecto glow
        /// </summary>
        private void CreateLoadingBar()
        {
            Color bgColor = GetThemeColor(t => t.inputBackground, new Color(0.1f, 0.1f, 0.15f));
            Color accentColor = GetThemeColor(t => t.primaryAccent, new Color(0f, 1f, 1f));
            Color glowColor = GetThemeColor(t => t.glowColor, new Color(0f, 1f, 1f, 0.5f));

            // Container de la barra
            GameObject barContainer = new GameObject("LoadingBarContainer");
            barContainer.transform.SetParent(canvas.transform, false);

            RectTransform containerRT = barContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.3f);
            containerRT.anchorMax = new Vector2(0.5f, 0.3f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(600, 12);
            containerRT.anchoredPosition = Vector2.zero;

            // Fondo de la barra (con bordes redondeados simulados)
            Image barBG = barContainer.AddComponent<Image>();
            barBG.color = bgColor;

            // Glow exterior (pulsa)
            GameObject glowOuter = new GameObject("LoadingBarGlowOuter");
            glowOuter.transform.SetParent(barContainer.transform, false);
            glowOuter.transform.SetAsFirstSibling();

            RectTransform glowOuterRT = glowOuter.AddComponent<RectTransform>();
            glowOuterRT.anchorMin = Vector2.zero;
            glowOuterRT.anchorMax = Vector2.one;
            glowOuterRT.sizeDelta = new Vector2(20, 20);
            glowOuterRT.anchoredPosition = Vector2.zero;

            loadingBarGlow = glowOuter.AddComponent<Image>();
            loadingBarGlow.color = glowColor;
            loadingBarGlow.raycastTarget = false;

            // Barra de progreso (fill)
            GameObject fillObj = new GameObject("LoadingBarFill");
            fillObj.transform.SetParent(barContainer.transform, false);

            RectTransform fillRT = fillObj.AddComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0, 0.1f);
            fillRT.anchorMax = new Vector2(0, 0.9f);
            fillRT.pivot = new Vector2(0, 0.5f);
            fillRT.anchoredPosition = new Vector2(2, 0);
            fillRT.sizeDelta = new Vector2(0, 0);

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = accentColor;
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
            Color textColor = GetThemeColor(t => t.textSecondary, new Color(0.8f, 0.8f, 0.8f));

            TextMeshProUGUI loadingText = UIFactory.CreateText(
                canvas.transform,
                "LoadingText",
                "",
                22,
                textColor,
                TextAlignmentOptions.Center
            );

            RectTransform rt = loadingText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.24f);
            rt.anchorMax = new Vector2(0.5f, 0.24f);
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
            Color textDisabled = GetThemeColor(t => t.textDisabled, new Color(0.4f, 0.4f, 0.4f));

            TextMeshProUGUI versionText = UIFactory.CreateText(
                canvas.transform,
                "VersionText",
                $"v{Application.version}",
                16,
                textDisabled,
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
        /// Crea decoraciones adicionales (líneas, patrones)
        /// </summary>
        private void CreateDecorations()
        {
            Color accentColor = GetThemeColor(t => t.primaryAccent, new Color(0f, 1f, 1f));

            // Líneas decorativas superiores
            CreateDecoLine(canvas.transform, new Vector2(0.1f, 0.75f), new Vector2(0.3f, 0.75f), accentColor, 2f);
            CreateDecoLine(canvas.transform, new Vector2(0.7f, 0.75f), new Vector2(0.9f, 0.75f), accentColor, 2f);

            // Líneas decorativas inferiores
            CreateDecoLine(canvas.transform, new Vector2(0.15f, 0.15f), new Vector2(0.35f, 0.15f), accentColor, 1f);
            CreateDecoLine(canvas.transform, new Vector2(0.65f, 0.15f), new Vector2(0.85f, 0.15f), accentColor, 1f);
        }

        /// <summary>
        /// Crea una línea decorativa
        /// </summary>
        private void CreateDecoLine(Transform parent, Vector2 start, Vector2 end, Color color, float thickness)
        {
            GameObject line = new GameObject("DecoLine");
            line.transform.SetParent(parent, false);

            RectTransform rt = line.AddComponent<RectTransform>();
            rt.anchorMin = start;
            rt.anchorMax = end;
            rt.sizeDelta = new Vector2(0, thickness);
            rt.anchoredPosition = Vector2.zero;

            Image img = line.AddComponent<Image>();
            Color lineColor = color;
            lineColor.a = 0.3f;
            img.color = lineColor;
            img.raycastTarget = false;
        }

        /// <summary>
        /// Configura el BootManager con las referencias de UI
        /// </summary>
        private void SetupBootManager()
        {
            // Crear BootManager si no existe
            GameObject managerObj = new GameObject("BootManager");
            bootManager = managerObj.AddComponent<BootManager>();

            Debug.Log("[BootUI] BootManager creado");
        }

        /// <summary>
        /// Configura el BootAnimator con todas las referencias
        /// </summary>
        private void SetupBootAnimator()
        {
            // Crear BootAnimator
            GameObject animatorObj = new GameObject("BootAnimator");
            bootAnimator = animatorObj.AddComponent<BootAnimator>();

            // Asignar referencias
            bootAnimator.SetLogoCanvasGroup(logoCanvasGroup);
            bootAnimator.SetLogoTransform(logoContainer);
            bootAnimator.SetLoadingTextReference(bootManager.loadingText);
            bootAnimator.SetLoadingBarGlow(loadingBarGlow);
            bootAnimator.SetLoadingBarFill(bootManager.loadingBar);
            bootAnimator.ConfigureParticles(neonParticles);

            Debug.Log("[BootUI] BootAnimator configurado");
        }
    }
}
