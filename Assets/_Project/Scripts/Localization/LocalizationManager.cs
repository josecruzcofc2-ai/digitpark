using UnityEngine;
using System.Collections.Generic;
using System;

namespace DigitPark.Localization
{
    [Serializable]
    public class LocalizedText
    {
        public string key;
        public string english;
        public string spanish;
    }

    /// <summary>
    /// Sistema de localización para múltiples idiomas
    /// Persiste entre escenas
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LocalizationManager>();
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        [Header("Traducciones")]
        public List<LocalizedText> localizedTexts = new List<LocalizedText>();

        private Dictionary<string, LocalizedText> textDictionary;
        private string currentLanguage = "English";

        // Evento para notificar cambios de idioma
        public static event Action OnLanguageChanged;

        private const string LANGUAGE_KEY = "Language";

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDictionary();
                LoadSavedLanguage();
                Debug.Log($"[Localization] Inicializado - Idioma: {currentLanguage}");
            }
            else if (_instance != this)
            {
                Debug.Log("[Localization] Instancia duplicada destruida");
                Destroy(gameObject);
            }
        }

        private void InitializeDictionary()
        {
            textDictionary = new Dictionary<string, LocalizedText>();

            // Agregar traducciones predefinidas
            AddDefaultTranslations();

            // Agregar traducciones del Inspector
            foreach (var text in localizedTexts)
            {
                if (!string.IsNullOrEmpty(text.key))
                {
                    textDictionary[text.key] = text;
                }
            }

            Debug.Log($"[Localization] {textDictionary.Count} traducciones cargadas");
        }

        private void AddDefaultTranslations()
        {
            // Login
            AddTranslation("login_title", "Login", "Iniciar Sesión");
            AddTranslation("email_placeholder", "Email", "Correo Electrónico");
            AddTranslation("password_placeholder", "Password", "Contraseña");
            AddTranslation("login_button", "Sign In", "Iniciar Sesión");
            AddTranslation("register_button", "Create Account", "Crear Cuenta");
            AddTranslation("remember_me", "Remember Me", "Recordarme");
            AddTranslation("forgot_password", "Forgot Password?", "¿Olvidaste tu contraseña?");
            AddTranslation("or_continue_with", "Or continue with", "O continúa con");

            // Main Menu
            AddTranslation("play_button", "Play", "Jugar");
            AddTranslation("scores_button", "Scores", "Puntuaciones");
            AddTranslation("tournament_button", "Tournaments", "Torneos");
            AddTranslation("settings_button", "Settings", "Configuración");

            // Settings
            AddTranslation("settings_title", "Settings", "Configuración");
            AddTranslation("volume_sound", "Sound Volume", "Volumen de Sonido");
            AddTranslation("volume_effects", "Effects Volume", "Volumen de Efectos");
            AddTranslation("change_name", "Change Username", "Cambiar Nombre");
            AddTranslation("logout_button", "Logout", "Cerrar Sesión");
            AddTranslation("delete_account", "Delete Account", "Eliminar Cuenta");
            AddTranslation("language", "Language", "Idioma");
            AddTranslation("back_button", "Back", "Volver");

            // Change Name Panel
            AddTranslation("change_name_title", "Change Username", "Cambiar Nombre de Usuario");
            AddTranslation("new_name_placeholder", "New username", "Nuevo nombre");
            AddTranslation("confirm_button", "Confirm", "Confirmar");
            AddTranslation("cancel_button", "Cancel", "Cancelar");

            // Delete Account Panel
            AddTranslation("delete_confirm_title", "Delete Account?", "¿Eliminar Cuenta?");
            AddTranslation("delete_confirm_message", "This action cannot be undone", "Esta acción no se puede deshacer");
            AddTranslation("delete_button", "Delete", "Eliminar");

            // Game
            AddTranslation("timer_label", "Time", "Tiempo");
            AddTranslation("best_time", "Best Time", "Mejor Tiempo");
            AddTranslation("play_again", "Play Again", "Jugar de Nuevo");
            AddTranslation("new_record", "New Record!", "¡Nuevo Récord!");

            // Success Messages - Level 1 (Basic)
            AddTranslation("msg_good_job", "Good job!", "¡Buen trabajo!");
            AddTranslation("msg_complete", "Complete!", "¡Completado!");
            AddTranslation("msg_nice_try", "Nice try!", "¡Buen intento!");
            AddTranslation("msg_well_done", "Well done!", "¡Bien hecho!");
            AddTranslation("msg_task_complete", "Task complete!", "¡Tarea completada!");

            // Success Messages - Level 2 (Decent)
            AddTranslation("msg_great_work", "Great work!", "¡Gran trabajo!");
            AddTranslation("msg_good_timing", "Good timing!", "¡Buen tiempo!");
            AddTranslation("msg_not_bad", "Not bad!", "¡Nada mal!");
            AddTranslation("msg_solid", "Solid performance!", "¡Sólido rendimiento!");
            AddTranslation("msg_keep_it_up", "Keep it up!", "¡Sigue así!");

            // Success Messages - Level 3 (Good)
            AddTranslation("msg_excellent", "Excellent!", "¡Excelente!");
            AddTranslation("msg_impressive", "Impressive!", "¡Impresionante!");
            AddTranslation("msg_great_speed", "Great speed!", "¡Gran velocidad!");
            AddTranslation("msg_well_played", "Well played!", "¡Bien jugado!");
            AddTranslation("msg_awesome", "Awesome job!", "¡Increíble!");

            // Success Messages - Level 4 (Very Good)
            AddTranslation("msg_amazing", "Amazing!", "¡Asombroso!");
            AddTranslation("msg_outstanding", "Outstanding!", "¡Sobresaliente!");
            AddTranslation("msg_superb", "Superb timing!", "¡Tiempo soberbio!");
            AddTranslation("msg_incredible", "Incredible speed!", "¡Velocidad increíble!");
            AddTranslation("msg_spectacular", "Spectacular!", "¡Espectacular!");
            AddTranslation("msg_on_fire", "You're on fire!", "¡Estás en llamas!");

            // Success Messages - Level 5 (Perfect)
            AddTranslation("msg_perfect", "PERFECT!", "¡PERFECTO!");
            AddTranslation("msg_legendary", "LEGENDARY!", "¡LEGENDARIO!");
            AddTranslation("msg_mind_blowing", "MIND BLOWING!", "¡ALUCINANTE!");
            AddTranslation("msg_master", "ABSOLUTE MASTER!", "¡MAESTRO ABSOLUTO!");
            AddTranslation("msg_unstoppable", "UNSTOPPABLE!", "¡IMPARABLE!");
            AddTranslation("msg_world_class", "WORLD CLASS!", "¡CLASE MUNDIAL!");
            AddTranslation("msg_godlike", "GODLIKE!", "¡DIVINO!");
            AddTranslation("msg_flawless", "FLAWLESS VICTORY!", "¡VICTORIA PERFECTA!");

            // Leaderboard / Scores
            AddTranslation("leaderboard_title", "Leaderboard", "Tabla de Posiciones");
            AddTranslation("global_tab", "Global", "Global");
            AddTranslation("country_tab", "Country", "País");
            AddTranslation("position", "Position", "Posición");
            AddTranslation("player", "Player", "Jugador");
            AddTranslation("time", "Time", "Tiempo");

            // Tournaments
            AddTranslation("tournaments_title", "Tournaments", "Torneos");
            AddTranslation("join_tournament", "Join", "Unirse");
            AddTranslation("exit_tournament", "Exit Tournament", "Salir del Torneo");
            AddTranslation("entry_fee", "Entry Fee", "Cuota de Entrada");
            AddTranslation("prize_pool", "Prize Pool", "Pozo de Premios");
            AddTranslation("participants", "Participants", "Participantes");

            // General
            AddTranslation("loading", "Loading...", "Cargando...");
            AddTranslation("error", "Error", "Error");
            AddTranslation("success", "Success", "Éxito");
            AddTranslation("yes", "Yes", "Sí");
            AddTranslation("no", "No", "No");
        }

        private void AddTranslation(string key, string english, string spanish)
        {
            textDictionary[key] = new LocalizedText
            {
                key = key,
                english = english,
                spanish = spanish
            };
        }

        private void LoadSavedLanguage()
        {
            int savedIndex = PlayerPrefs.GetInt(LANGUAGE_KEY, 0);
            currentLanguage = savedIndex == 0 ? "English" : "Español";
        }

        /// <summary>
        /// Obtiene el texto traducido para una clave
        /// </summary>
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            if (textDictionary != null && textDictionary.ContainsKey(key))
            {
                var text = textDictionary[key];
                return currentLanguage == "English" ? text.english : text.spanish;
            }

            Debug.LogWarning($"[Localization] Clave no encontrada: {key}");
            return key;
        }

        /// <summary>
        /// Cambia el idioma (0 = English, 1 = Español)
        /// </summary>
        public void SetLanguage(int index)
        {
            string newLanguage = index == 0 ? "English" : "Español";

            if (currentLanguage != newLanguage)
            {
                currentLanguage = newLanguage;
                PlayerPrefs.SetInt(LANGUAGE_KEY, index);
                PlayerPrefs.Save();

                Debug.Log($"[Localization] Idioma cambiado a: {currentLanguage}");

                // Notificar a todos los textos
                OnLanguageChanged?.Invoke();
            }
        }

        /// <summary>
        /// Obtiene el índice del idioma actual (0 = English, 1 = Español)
        /// </summary>
        public int GetCurrentLanguageIndex()
        {
            return currentLanguage == "English" ? 0 : 1;
        }

        /// <summary>
        /// Obtiene el nombre del idioma actual
        /// </summary>
        public string GetCurrentLanguage()
        {
            return currentLanguage;
        }
    }
}
