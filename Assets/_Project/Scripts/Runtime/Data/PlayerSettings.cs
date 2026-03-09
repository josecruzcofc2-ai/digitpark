using UnityEngine;

namespace DigitPark.Data
{
    /// <summary>
    /// Configuraciones personalizables del jugador
    /// </summary>
    [System.Serializable]
    public class PlayerSettings
    {
        // Audio
        public float musicVolume;
        public float sfxVolume;
        public bool vibrationEnabled;

        // Notificaciones
        public bool pushNotificationsEnabled;
        public bool tournamentNotifications;
        public bool dailyRewardNotifications;

        // Visual
        public ThemeType theme;
        public GraphicsQuality graphicsQuality;
        public int targetFPS;

        // Idioma
        public SystemLanguage language;

        // Cuenta
        public bool rememberMe;
        public bool connectedToGoogle;
        public bool connectedToFacebook;

        public PlayerSettings()
        {
            // Valores por defecto
            musicVolume = 0.7f;
            sfxVolume = 0.8f;
            vibrationEnabled = true;

            pushNotificationsEnabled = true;
            tournamentNotifications = true;
            dailyRewardNotifications = true;

            theme = ThemeType.Auto;
            graphicsQuality = GraphicsQuality.High;
            targetFPS = 60;

            language = SystemLanguage.English; // Default seguro, se detectará después

            rememberMe = false;
            connectedToGoogle = false;
            connectedToFacebook = false;
        }

        /// <summary>
        /// Aplica las configuraciones al juego
        /// </summary>
        public void Apply()
        {
            // Aplicar configuración de FPS
            Application.targetFrameRate = targetFPS;

            // Aplicar calidad gráfica
            switch (graphicsQuality)
            {
                case GraphicsQuality.Low:
                    QualitySettings.SetQualityLevel(0);
                    break;
                case GraphicsQuality.Medium:
                    QualitySettings.SetQualityLevel(1);
                    break;
                case GraphicsQuality.High:
                    QualitySettings.SetQualityLevel(2);
                    break;
            }
        }
    }

    public enum ThemeType
    {
        Light,
        Dark,
        Auto
    }

    public enum GraphicsQuality
    {
        Low,
        Medium,
        High
    }
}
