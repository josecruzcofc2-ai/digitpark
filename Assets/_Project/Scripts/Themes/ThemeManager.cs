using UnityEngine;
using System;
using System.Collections.Generic;

namespace DigitPark.Themes
{
    /// <summary>
    /// Manager singleton que controla el sistema de temas de la aplicación
    /// Permite cambiar entre temas y notifica a todos los componentes suscritos
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        private static ThemeManager _instance;
        public static ThemeManager Instance => _instance;

        /// <summary>
        /// Evento que se dispara cuando cambia el tema
        /// </summary>
        public static event Action<ThemeData> OnThemeChanged;

        [Header("Configuration")]
        [SerializeField] private List<ThemeData> availableThemes = new List<ThemeData>();
        [SerializeField] private ThemeData defaultTheme;
        [SerializeField] private bool persistThemeChoice = true;

        private const string THEME_PREF_KEY = "SelectedTheme";
        private ThemeData _currentTheme;

        /// <summary>
        /// Tema actual activo
        /// </summary>
        public ThemeData CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnThemeChanged?.Invoke(_currentTheme);
                    Debug.Log($"[ThemeManager] Tema cambiado a: {_currentTheme?.themeName}");
                }
            }
        }

        /// <summary>
        /// Lista de temas disponibles
        /// </summary>
        public List<ThemeData> AvailableThemes => availableThemes;

        /// <summary>
        /// Índice del tema actual
        /// </summary>
        public int CurrentThemeIndex
        {
            get
            {
                if (_currentTheme == null) return 0;
                return availableThemes.IndexOf(_currentTheme);
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Inicializa el sistema de temas
        /// </summary>
        private void Initialize()
        {
            // Cargar temas desde Resources si no hay ninguno asignado
            if (availableThemes.Count == 0)
            {
                LoadThemesFromResources();
            }

            // Cargar tema guardado o usar el default
            LoadSavedTheme();

            Debug.Log($"[ThemeManager] Inicializado con {availableThemes.Count} temas disponibles");
        }

        /// <summary>
        /// Carga todos los temas desde la carpeta Resources/Themes
        /// </summary>
        private void LoadThemesFromResources()
        {
            ThemeData[] themes = Resources.LoadAll<ThemeData>("Themes");
            availableThemes.AddRange(themes);

            if (themes.Length > 0)
            {
                Debug.Log($"[ThemeManager] Cargados {themes.Length} temas desde Resources");
            }
        }

        /// <summary>
        /// Carga el tema guardado en PlayerPrefs
        /// </summary>
        private void LoadSavedTheme()
        {
            if (persistThemeChoice && PlayerPrefs.HasKey(THEME_PREF_KEY))
            {
                string savedThemeId = PlayerPrefs.GetString(THEME_PREF_KEY);
                ThemeData savedTheme = GetThemeById(savedThemeId);

                if (savedTheme != null)
                {
                    _currentTheme = savedTheme;
                    Debug.Log($"[ThemeManager] Tema cargado: {savedTheme.themeName}");
                    return;
                }
            }

            // Usar tema default si no hay guardado
            if (defaultTheme != null)
            {
                _currentTheme = defaultTheme;
            }
            else if (availableThemes.Count > 0)
            {
                _currentTheme = availableThemes[0];
            }

            // Notificar el tema inicial
            OnThemeChanged?.Invoke(_currentTheme);
        }

        /// <summary>
        /// Cambia al tema especificado
        /// </summary>
        public void SetTheme(ThemeData theme)
        {
            if (theme == null)
            {
                Debug.LogWarning("[ThemeManager] Intento de establecer tema null");
                return;
            }

            if (!availableThemes.Contains(theme))
            {
                Debug.LogWarning($"[ThemeManager] Tema '{theme.themeName}' no está en la lista de temas disponibles");
                availableThemes.Add(theme);
            }

            CurrentTheme = theme;

            if (persistThemeChoice)
            {
                PlayerPrefs.SetString(THEME_PREF_KEY, theme.themeId);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Cambia al tema por su ID
        /// </summary>
        public void SetTheme(string themeId)
        {
            ThemeData theme = GetThemeById(themeId);
            if (theme != null)
            {
                SetTheme(theme);
            }
            else
            {
                Debug.LogWarning($"[ThemeManager] Tema con ID '{themeId}' no encontrado");
            }
        }

        /// <summary>
        /// Cambia al tema por índice
        /// </summary>
        public void SetTheme(int index)
        {
            if (index >= 0 && index < availableThemes.Count)
            {
                SetTheme(availableThemes[index]);
            }
            else
            {
                Debug.LogWarning($"[ThemeManager] Índice de tema {index} fuera de rango");
            }
        }

        /// <summary>
        /// Cambia al siguiente tema
        /// </summary>
        public void NextTheme()
        {
            int nextIndex = (CurrentThemeIndex + 1) % availableThemes.Count;
            SetTheme(nextIndex);
        }

        /// <summary>
        /// Cambia al tema anterior
        /// </summary>
        public void PreviousTheme()
        {
            int prevIndex = CurrentThemeIndex - 1;
            if (prevIndex < 0) prevIndex = availableThemes.Count - 1;
            SetTheme(prevIndex);
        }

        /// <summary>
        /// Obtiene un tema por su ID
        /// </summary>
        public ThemeData GetThemeById(string themeId)
        {
            return availableThemes.Find(t => t.themeId == themeId);
        }

        /// <summary>
        /// Obtiene un tema por su nombre
        /// </summary>
        public ThemeData GetThemeByName(string themeName)
        {
            return availableThemes.Find(t => t.themeName == themeName);
        }

        /// <summary>
        /// Verifica si un tema está disponible (no premium o ya comprado)
        /// </summary>
        public bool IsThemeAvailable(ThemeData theme)
        {
            if (theme == null) return false;
            if (!theme.isPremium) return true;

            // TODO: Verificar si el usuario tiene premium
            // Por ahora, todos los temas están disponibles
            return true;
        }

        /// <summary>
        /// Obtiene solo los temas gratuitos
        /// </summary>
        public List<ThemeData> GetFreeThemes()
        {
            return availableThemes.FindAll(t => !t.isPremium);
        }

        /// <summary>
        /// Obtiene solo los temas premium
        /// </summary>
        public List<ThemeData> GetPremiumThemes()
        {
            return availableThemes.FindAll(t => t.isPremium);
        }

        /// <summary>
        /// Registra un nuevo tema en tiempo de ejecución
        /// </summary>
        public void RegisterTheme(ThemeData theme)
        {
            if (theme != null && !availableThemes.Contains(theme))
            {
                availableThemes.Add(theme);
                Debug.Log($"[ThemeManager] Tema registrado: {theme.themeName}");
            }
        }

        /// <summary>
        /// Fuerza la actualización del tema actual (re-aplica a todos los componentes)
        /// </summary>
        public void RefreshTheme()
        {
            OnThemeChanged?.Invoke(_currentTheme);
        }

        /// <summary>
        /// Obtiene los nombres de todos los temas disponibles
        /// </summary>
        public string[] GetThemeNames()
        {
            string[] names = new string[availableThemes.Count];
            for (int i = 0; i < availableThemes.Count; i++)
            {
                names[i] = availableThemes[i].themeName;
            }
            return names;
        }
    }
}
