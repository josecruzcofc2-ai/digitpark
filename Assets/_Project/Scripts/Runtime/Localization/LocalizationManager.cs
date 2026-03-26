using UnityEngine;
using System.Collections.Generic;
using System;

namespace DigitPark.Localization
{
    public enum Language
    {
        English = 0,
        Spanish = 1
    }

    [Serializable]
    public class LocalizedText
    {
        public string key;
        public string english;
        public string spanish;
    }

    /// <summary>
    /// Sistema de localización para múltiples idiomas
    /// Soporta: English, Español
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get { return _instance; }
            private set { _instance = value; }
        }

        [Header("Traducciones")]
        public List<LocalizedText> localizedTexts = new List<LocalizedText>();

        private Dictionary<string, LocalizedText> textDictionary;
        private Language currentLanguage = Language.English;

        // Evento para notificar cambios de idioma
        public static event Action OnLanguageChanged;

        private const string LANGUAGE_KEY = "Language";

        // Nombres de idiomas para mostrar en UI
        public static readonly string[] LanguageNames = { "English", "Español" };
        public static readonly string[] LanguageNativeCodes = { "en", "es" };

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDictionary();
                LoadSavedLanguage();
                Debug.Log($"[Localization] Inicializado - Idioma: {LanguageNames[(int)currentLanguage]}");

                // Asegurar que AutoLocalizer existe
                EnsureAutoLocalizer();

                // Asegurar que LocalizedTextLayoutFixer existe
                EnsureLayoutFixer();
            }
            else if (_instance != this)
            {
                Debug.Log("[Localization] Instancia duplicada destruida");
                Destroy(gameObject);
            }
        }

        private void EnsureAutoLocalizer()
        {
            Debug.Log("[Localization] Verificando AutoLocalizer...");

            // Buscar si ya existe en la escena
            var existingAutoLocalizer = FindFirstObjectByType<AutoLocalizer>();

            if (existingAutoLocalizer == null)
            {
                // Crear AutoLocalizer en el mismo GameObject
                var autoLocalizer = gameObject.AddComponent<AutoLocalizer>();
                Debug.Log($"[Localization] AutoLocalizer creado: {autoLocalizer != null}");
            }
            else
            {
                Debug.Log("[Localization] AutoLocalizer ya existe en la escena");
            }
        }

        private void EnsureLayoutFixer()
        {
            Debug.Log("[Localization] Verificando LocalizedTextLayoutFixer...");

            var existingFixer = FindFirstObjectByType<DigitPark.UI.LocalizedTextLayoutFixer>();
            if (existingFixer == null)
            {
                GameObject fixerObj = new GameObject("LocalizedTextLayoutFixer");
                fixerObj.AddComponent<DigitPark.UI.LocalizedTextLayoutFixer>();
                Debug.Log("[Localization] LocalizedTextLayoutFixer creado");
            }
            else
            {
                Debug.Log("[Localization] LocalizedTextLayoutFixer ya existe");
            }
        }

        private void InitializeDictionary()
        {
            textDictionary = new Dictionary<string, LocalizedText>();

            // PRIMERO: Cargar desde archivo Translations.txt (fuente centralizada)
            LoadTranslationsFromFile();

            // SEGUNDO: Agregar traducciones hardcodeadas como fallback
            AddDefaultTranslations();

            // TERCERO: Agregar traducciones del Inspector (override)
            foreach (var text in localizedTexts)
            {
                if (!string.IsNullOrEmpty(text.key))
                {
                    textDictionary[text.key] = text;
                }
            }

            Debug.Log($"[Localization] {textDictionary.Count} traducciones cargadas");
        }

        /// <summary>
        /// Carga traducciones desde el archivo Translations.txt centralizado
        /// </summary>
        private void LoadTranslationsFromFile()
        {
            try
            {
                TextAsset translationsFile = Resources.Load<TextAsset>("Translations");
                if (translationsFile == null)
                {
                    // Intentar cargar desde StreamingAssets o ruta directa
                    string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Translations.txt");
#if UNITY_EDITOR
                    if (!System.IO.File.Exists(path))
                    {
                        path = "Assets/_Project/Localization/Translations.txt";
                    }
#endif

                    if (System.IO.File.Exists(path))
                    {
                        string content = System.IO.File.ReadAllText(path);
                        ParseTranslationsFile(content);
                    }
                    else
                    {
                        Debug.LogWarning("[Localization] Archivo Translations.txt no encontrado, usando traducciones hardcodeadas");
                    }
                }
                else
                {
                    ParseTranslationsFile(translationsFile.text);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Localization] Error cargando Translations.txt: {e.Message}");
            }
        }

        /// <summary>
        /// Parsea el contenido del archivo Translations.txt
        /// Formato:
        /// key_name
        ///     EN: English text
        ///     ES: Spanish text
        /// </summary>
        private void ParseTranslationsFile(string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            string[] lines = content.Split('\n');
            string currentKey = null;
            string en = "", es = "";

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');

                // Saltar líneas vacías y comentarios
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("=") || line.StartsWith("#") || line.StartsWith("//"))
                {
                    // Si teníamos una key pendiente, guardarla
                    if (!string.IsNullOrEmpty(currentKey) && !string.IsNullOrEmpty(en))
                    {
                        SaveParsedTranslation(currentKey, en, es);
                        currentKey = null;
                        en = es = "";
                    }
                    continue;
                }

                string trimmed = line.Trim();

                // Detectar líneas de idioma (soporta "EN:" y "EN " formatos)
                string langValue = TryExtractLangValue(trimmed, "EN");
                if (langValue != null)
                {
                    en = langValue;
                }
                else if ((langValue = TryExtractLangValue(trimmed, "ES")) != null)
                {
                    es = langValue;
                }
                // Si no empieza con espacio/tab y no es línea de idioma, es una nueva key
                else if (!line.StartsWith(" ") && !line.StartsWith("\t") && !trimmed.Contains(":") && !trimmed.Contains("|"))
                {
                    // Guardar key anterior si existe
                    if (!string.IsNullOrEmpty(currentKey) && !string.IsNullOrEmpty(en))
                    {
                        SaveParsedTranslation(currentKey, en, es);
                    }

                    // Nueva key
                    currentKey = trimmed;
                    en = es = "";
                }
            }

            // Guardar última key
            if (!string.IsNullOrEmpty(currentKey) && !string.IsNullOrEmpty(en))
            {
                SaveParsedTranslation(currentKey, en, es);
            }

            Debug.Log($"[Localization] {textDictionary.Count} traducciones cargadas desde archivo");
        }

        /// <summary>
        /// Extracts the value for a language prefix. Supports both "EN:" and "EN " (space) formats.
        /// Returns null if the line doesn't match the given language prefix.
        /// </summary>
        private static string TryExtractLangValue(string trimmedLine, string lang)
        {
            // Format 1: "EN: value" or "EN:value"
            if (trimmedLine.StartsWith(lang + ":"))
                return trimmedLine.Substring(lang.Length + 1).Trim();

            // Format 2: "EN value" (space-separated, must be exactly the lang code followed by space)
            if (trimmedLine.StartsWith(lang + " ") && trimmedLine.Length > lang.Length + 1)
            {
                string rest = trimmedLine.Substring(lang.Length).Trim();
                if (!string.IsNullOrEmpty(rest))
                    return rest;
            }

            return null;
        }

        private void SaveParsedTranslation(string key, string en, string es)
        {
            if (textDictionary.ContainsKey(key)) return; // No sobrescribir

            textDictionary[key] = new LocalizedText
            {
                key = key,
                english = !string.IsNullOrEmpty(en) ? en : key,
                spanish = !string.IsNullOrEmpty(es) ? es : en
            };
        }

        private void AddDefaultTranslations()
        {
            // ==================== LOGIN ====================
            AddTranslation("login_title",
                "Login", "Iniciar Sesión");
            AddTranslation("email_placeholder",
                "Email", "Correo Electrónico");
            AddTranslation("password_placeholder",
                "Password", "Contraseña");
            AddTranslation("login_button",
                "Sign In", "Iniciar Sesión");
            AddTranslation("register_button",
                "Create Account", "Crear Cuenta");
            AddTranslation("remember_me",
                "Remember Me", "Recordarme");
            AddTranslation("forgot_password",
                "Forgot Password?", "¿Olvidaste tu contraseña?");
            AddTranslation("or_continue_with",
                "Or continue with", "O continúa con");

            // ==================== MAIN MENU ====================
            AddTranslation("play_button",
                "Play", "Jugar");
            AddTranslation("scores_button",
                "Scores", "Puntuaciones");
            AddTranslation("tournament_button",
                "Tournaments", "Torneos");
            AddTranslation("settings_button",
                "Settings", "Configuración");
            AddTranslation("no_username",
                "No Username", "Sin Usuario");

            // ==================== SETTINGS ====================
            AddTranslation("settings_title",
                "Settings", "Configuración");
            AddTranslation("volume_sound",
                "Sound Volume", "Volumen de Sonido");
            AddTranslation("volume_effects",
                "Effects Volume", "Volumen de Efectos");
            AddTranslation("change_name",
                "Change Username", "Cambiar Nombre");
            AddTranslation("logout_button",
                "Logout", "Cerrar Sesión");
            AddTranslation("delete_account",
                "Delete Account", "Eliminar Cuenta");
            AddTranslation("language",
                "Language", "Idioma");
            AddTranslation("change_language",
                "Change Language", "Cambiar Idioma");
            AddTranslation("back_button",
                "Back", "Volver");

            // ==================== PREMIUM / PURCHASES ====================
            AddTranslation("premium_title",
                "PREMIUM", "PREMIUM");
            AddTranslation("premium_section_title",
                "Premium", "Premium");
            AddTranslation("no_ads_title",
                "NO ADS", "SIN ANUNCIOS");
            AddTranslation("no_ads_description",
                "Play without interruptions", "Juega sin interrupciones");
            AddTranslation("no_ads_price",
                "$9.99", "$9.99");
            AddTranslation("remove_ads_title",
                "Remove Ads", "Quitar Anuncios");
            AddTranslation("remove_ads_description",
                "Remove all ads from the app", "Elimina todos los anuncios de la app");
            AddTranslation("premium_full_title",
                "PREMIUM", "PREMIUM");
            AddTranslation("premium_full_description",
                "No ads + Create tournaments", "Sin anuncios + Crear torneos");
            AddTranslation("premium_full_price",
                "$19.99", "$19.99");
            AddTranslation("buy_button",
                "BUY", "COMPRAR");
            AddTranslation("premium_recommended",
                "RECOMMENDED", "RECOMENDADO");
            AddTranslation("premium_feature_no_ads",
                "No advertisements", "Sin anuncios");
            AddTranslation("premium_feature_tournaments",
                "Create unlimited tournaments", "Crear torneos ilimitados");
            AddTranslation("premium_feature_badge",
                "Exclusive badge", "Insignia exclusiva");
            AddTranslation("premium_active",
                "Premium Active", "Premium Activo");
            AddTranslation("you_are_premium",
                "You are a Premium member!", "Eres miembro Premium!");
            AddTranslation("no_ads_active",
                "No Ads Active", "Sin Anuncios Activo");
            AddTranslation("already_purchased",
                "Already purchased", "Ya comprado");
            AddTranslation("acquired_text",
                "Acquired", "Adquirido");
            AddTranslation("restore_purchases",
                "Restore Purchases", "Restaurar Compras");
            AddTranslation("purchase_success",
                "Purchase successful!", "¡Compra exitosa!");
            AddTranslation("purchase_failed",
                "Purchase failed. Try again.", "Error en la compra. Intenta de nuevo.");
            AddTranslation("purchase_cancelled",
                "Purchase cancelled", "Compra cancelada");
            AddTranslation("processing_purchase",
                "Processing purchase...", "Procesando compra...");

            // ==================== PREMIUM REQUIRED PANEL ====================
            AddTranslation("premium_required_title",
                "Premium Required", "Se Requiere Premium");
            AddTranslation("premium_required_message",
                "You need Premium to create tournaments.\nGet Premium Full to unlock this feature!",
                "Necesitas Premium para crear torneos.\n¡Obtén Premium Completo para desbloquear esta función!");
            AddTranslation("get_premium",
                "Get Premium", "Obtener Premium");
            AddTranslation("maybe_later",
                "Maybe Later", "Quizás Después");

            // ==================== CHANGE NAME PANEL ====================
            AddTranslation("change_name_title",
                "Change Username", "Cambiar Nombre de Usuario");
            AddTranslation("new_name_placeholder",
                "New username", "Nuevo nombre");
            AddTranslation("confirm_button",
                "Confirm", "Confirmar");
            AddTranslation("cancel_button",
                "Cancel", "Cancelar");

            // ==================== DELETE ACCOUNT ====================
            AddTranslation("delete_confirm_title",
                "Delete Account?", "¿Eliminar Cuenta?");
            AddTranslation("delete_confirm_message",
                "This action cannot be undone", "Esta acción no se puede deshacer");
            AddTranslation("delete_button",
                "Delete", "Eliminar");

            // ==================== GAME ====================
            AddTranslation("timer_label",
                "Time", "Tiempo");
            AddTranslation("best_time",
                "Best Time", "Mejor Tiempo");
            AddTranslation("best_label",
                "Best:", "Mejor:");
            AddTranslation("no_best_time",
                "Best: --", "Mejor: --");
            AddTranslation("play_again",
                "Play Again", "Jugar de Nuevo");
            AddTranslation("new_record",
                "New Record!", "¡Nuevo Récord!");

            // ==================== SUCCESS MESSAGES - Level 1 (PERFECT < 1s) SUPER DOPAMINE ====================
            AddTranslation("msg_godlike_focus",
                "GODLIKE FOCUS!", "¡ENFOQUE DIVINO!");
            AddTranslation("msg_mind_on_fire",
                "YOUR MIND IS ON FIRE!", "¡TU MENTE ESTÁ EN LLAMAS!");
            AddTranslation("msg_exceptional_reflexes",
                "EXCEPTIONAL REFLEXES!", "¡REFLEJOS EXCEPCIONALES!");
            AddTranslation("msg_neural_perfection",
                "NEURAL PERFECTION!", "¡PERFECCIÓN NEURONAL!");
            AddTranslation("msg_time_master",
                "MASTER OF TIME!", "¡MAESTRO DEL TIEMPO!");
            AddTranslation("msg_superhuman",
                "SUPERHUMAN SPEED!", "¡VELOCIDAD SOBREHUMANA!");
            AddTranslation("msg_unstoppable_force",
                "UNSTOPPABLE FORCE!", "¡FUERZA IMPARABLE!");
            AddTranslation("msg_legendary_speed",
                "LEGENDARY SPEED!", "¡VELOCIDAD LEGENDARIA!");
            AddTranslation("msg_pure_genius",
                "PURE GENIUS!", "¡PURO GENIO!");
            AddTranslation("msg_absolute_legend",
                "ABSOLUTE LEGEND!", "¡LEYENDA ABSOLUTA!");

            // ==================== SUCCESS MESSAGES - Level 2 (VERY GOOD 1-2s) HIGH DOPAMINE ====================
            AddTranslation("msg_incredible_focus",
                "Incredible focus!", "¡Enfoque increíble!");
            AddTranslation("msg_blazing_fast",
                "Blazing fast!", "¡Velocidad ardiente!");
            AddTranslation("msg_sharp_mind",
                "Sharp mind!", "¡Mente aguda!");
            AddTranslation("msg_impressive_reflexes",
                "Impressive reflexes!", "¡Reflejos impresionantes!");
            AddTranslation("msg_excellent_timing",
                "Excellent timing!", "¡Tiempo excelente!");
            AddTranslation("msg_on_fire",
                "You're on fire!", "¡Estás en llamas!");
            AddTranslation("msg_amazing_speed",
                "Amazing speed!", "¡Velocidad asombrosa!");
            AddTranslation("msg_brilliant_play",
                "Brilliant play!", "¡Jugada brillante!");
            AddTranslation("msg_stellar_performance",
                "Stellar performance!", "¡Rendimiento estelar!");
            AddTranslation("msg_remarkable",
                "Remarkable!", "¡Notable!");

            // ==================== SUCCESS MESSAGES - Level 3 (GOOD 2-3s) POSITIVE ====================
            AddTranslation("msg_great_job",
                "Great job!", "¡Gran trabajo!");
            AddTranslation("msg_well_played",
                "Well played!", "¡Bien jugado!");
            AddTranslation("msg_nice_speed",
                "Nice speed!", "¡Buena velocidad!");
            AddTranslation("msg_good_reflexes",
                "Good reflexes!", "¡Buenos reflejos!");
            AddTranslation("msg_solid_time",
                "Solid time!", "¡Tiempo sólido!");

            // ==================== SUCCESS MESSAGES - Level 4 (DECENT 3-4s) ENCOURAGING ====================
            AddTranslation("msg_good_effort",
                "Good effort!", "¡Buen esfuerzo!");
            AddTranslation("msg_not_bad",
                "Not bad!", "¡Nada mal!");
            AddTranslation("msg_keep_going",
                "Keep going!", "¡Sigue adelante!");
            AddTranslation("msg_nice_try",
                "Nice try!", "¡Buen intento!");
            AddTranslation("msg_getting_better",
                "Getting better!", "¡Mejorando!");

            // ==================== SUCCESS MESSAGES - Level 5 (BASIC 4-5s) MOTIVATIONAL ====================
            AddTranslation("msg_completed",
                "Completed!", "¡Completado!");
            AddTranslation("msg_done",
                "Done!", "¡Hecho!");
            AddTranslation("msg_finished",
                "Finished!", "¡Terminado!");
            AddTranslation("msg_keep_practicing",
                "Keep practicing!", "¡Sigue practicando!");
            AddTranslation("msg_you_can_improve",
                "You can do better!", "¡Puedes hacerlo mejor!");

            // ==================== SUCCESS MESSAGES - Level 6 (NO CLASSIFY 5s+) EMOTIONAL SUPPORT ====================
            AddTranslation("msg_almost_there",
                "Almost there... keep trying!", "Casi lo logras... ¡sigue intentando!");
            AddTranslation("msg_breathe_continue",
                "Breathe and continue", "Respira y sigue");
            AddTranslation("msg_next_will_be_better",
                "Next one will be better!", "¡El siguiente será mejor!");
            AddTranslation("msg_dont_give_up",
                "Don't give up!", "¡No te rindas!");
            AddTranslation("msg_patience_wins",
                "Patience wins", "La paciencia gana");
            AddTranslation("msg_every_try_counts",
                "Every try counts!", "¡Cada intento cuenta!");
            AddTranslation("msg_progress_not_perfection",
                "Progress, not perfection", "Progreso, no perfección");
            AddTranslation("msg_keep_calm",
                "Stay calm and try again", "Mantén la calma e intenta de nuevo");
            AddTranslation("msg_believe_yourself",
                "Believe in yourself!", "¡Cree en ti mismo!");
            AddTranslation("msg_stay_focused",
                "Stay focused, you got this!", "¡Concéntrate, tú puedes!");

            // ==================== LEADERBOARD / SCORES ====================
            AddTranslation("leaderboard_title",
                "Leaderboard", "Tabla de Posiciones");
            AddTranslation("global_tab",
                "Global", "Global");
            AddTranslation("country_tab",
                "Country", "País");
            AddTranslation("position",
                "Position", "Posición");
            AddTranslation("player",
                "Player", "Jugador");
            AddTranslation("time",
                "Time", "Tiempo");
            AddTranslation("loading_rankings",
                "Loading rankings...", "Cargando rankings...");
            AddTranslation("error_loading_rankings",
                "Error loading rankings", "Error al cargar rankings");
            AddTranslation("your_position",
                "Your position:", "Tu posición:");
            AddTranslation("your_best_time",
                "Best time:", "Mejor tiempo:");
            AddTranslation("no_best_time_yet",
                "No best time", "Sin mejor tiempo");
            AddTranslation("history_games",
                "History: {0} games", "Historial: {0} partidas");
            AddTranslation("no_scores_yet",
                "No scores yet\n\nPlay some games to see your scores here",
                "No hay puntuaciones aún\n\nJuega para ver tus scores aquí");
            AddTranslation("no_date",
                "No date", "Sin fecha");
            AddTranslation("invalid_date",
                "Invalid date", "Fecha inválida");

            // ==================== TOURNAMENTS ====================
            AddTranslation("tournaments_title",
                "Tournaments", "Torneos");
            AddTranslation("search_tab",
                "Search", "Buscar");
            AddTranslation("my_tournaments_tab",
                "My Tournaments", "Mis Torneos");
            AddTranslation("create_tab",
                "Create", "Crear");
            AddTranslation("join_tournament",
                "Join", "Unirse");
            AddTranslation("exit_tournament",
                "Exit Tournament", "Salir del Torneo");
            AddTranslation("entry_fee",
                "Entry Fee", "Cuota de Entrada");
            AddTranslation("prize_pool",
                "Prize Pool", "Pozo de Premios");
            AddTranslation("participants",
                "Participants", "Participantes");
            AddTranslation("join_confirm_message",
                "Do you want to join this tournament?", "¿Deseas unirte a este torneo?");
            AddTranslation("creator_label",
                "Creator:", "Creador:");
            AddTranslation("time_remaining",
                "Time remaining:", "Tiempo restante:");
            AddTranslation("tournament_of",
                "Tournament of", "Torneo de");
            AddTranslation("no_active_tournaments",
                "No active tournaments", "No hay torneos activos");
            AddTranslation("not_in_tournament",
                "You're not in any tournament", "No participas en ningún torneo");
            AddTranslation("create_error",
                "Could not create tournament. Try again.", "No se pudo crear el torneo. Intenta nuevamente.");
            AddTranslation("join_error",
                "Could not join tournament. Try again.", "No se pudo unir al torneo. Intenta nuevamente.");
            AddTranslation("join_success",
                "You've joined the tournament!", "¡Te has unido al torneo exitosamente!");
            AddTranslation("create_success",
                "Tournament created! You've been added automatically.", "¡Torneo creado exitosamente! Te has unido automáticamente.");
            AddTranslation("exit_success",
                "You left the tournament", "Has abandonado el torneo exitosamente");
            AddTranslation("exit_error",
                "Could not leave tournament. Try again.", "No se pudo salir del torneo. Intenta nuevamente.");
            AddTranslation("exit_confirm_title",
                "Leave Tournament", "Abandonar Torneo");
            AddTranslation("exit_confirm_message",
                "Are you sure you want to leave? Your progress in this tournament will be lost.", "¿Seguro que quieres abandonar? Tu progreso en este torneo se perderá.");
            AddTranslation("no_time",
                "No time", "Sin tiempo");
            AddTranslation("finished",
                "Finished", "Finalizado");
            AddTranslation("attempts",
                "attempts", "intentos");
            AddTranslation("try_again",
                "Try again", "Intenta nuevamente");
            AddTranslation("max_players",
                "Max Players", "Máx. Jugadores");
            AddTranslation("duration",
                "Duration", "Duración");
            AddTranslation("public",
                "Public", "Público");
            AddTranslation("private",
                "Private", "Privado");
            AddTranslation("create_tournament",
                "Create Tournament", "Crear Torneo");

            // ==================== BOOT / LOADING ====================
            AddTranslation("boot_subtitle",
                "ARCADE EXPERIENCE", "EXPERIENCIA ARCADE");
            AddTranslation("boot_subtitle2",
                "TRAIN YOUR MIND", "ENTRENA TU MENTE");
            AddTranslation("boot_loading",
                "Loading...", "Cargando...");
            AddTranslation("boot_initializing_config",
                "Initializing settings...", "Inicializando configuración...");
            AddTranslation("boot_connecting_services",
                "Connecting to services...", "Conectando a servicios...");
            AddTranslation("boot_loading_resources",
                "Loading resources...", "Cargando recursos...");
            AddTranslation("boot_verifying_user",
                "Verifying user...", "Verificando usuario...");
            AddTranslation("boot_completed",
                "Completed!", "¡Completado!");
            AddTranslation("boot_error",
                "Error initializing. Please restart.", "Error al inicializar. Por favor reinicia.");

            // ==================== USERNAME POPUP ====================
            AddTranslation("username_popup_title",
                "Choose a username!", "¡Elige un nombre de usuario!");
            AddTranslation("username_placeholder",
                "Username", "Nombre de usuario");

            // ==================== CONFIRMATION POPUP ====================
            AddTranslation("current_value",
                "Current:", "Actual:");
            AddTranslation("new_value",
                "New:", "Nuevo:");

            // ==================== GENERAL ====================
            AddTranslation("loading",
                "Loading...", "Cargando...");
            AddTranslation("error",
                "Error", "Error");
            AddTranslation("success",
                "Success", "Éxito");
            AddTranslation("yes",
                "Yes", "Sí");
            AddTranslation("no",
                "No", "No");
            AddTranslation("ok",
                "OK", "OK");
            AddTranslation("close",
                "Close", "Cerrar");
            AddTranslation("save",
                "Save", "Guardar");
            AddTranslation("apply",
                "Apply", "Aplicar");
            AddTranslation("clear",
                "Clear", "Limpiar");
            AddTranslation("search",
                "Search", "Buscar");
            AddTranslation("filter",
                "Filter", "Filtrar");
            AddTranslation("options",
                "Options", "Opciones");

            // ==================== TIME FORMATS ====================
            AddTranslation("time_days_hours",
                "{0}d {1}h", "{0}d {1}h");
            AddTranslation("time_hours_minutes",
                "{0}h {1}m", "{0}h {1}m");
            AddTranslation("time_minutes_seconds",
                "{0}m {1}s", "{0}m {1}s");
            AddTranslation("seconds_abbr",
                "s", "s");
            AddTranslation("hours_abbr",
                "h", "h");
            AddTranslation("days_abbr",
                "d", "d");

            // ==================== LEADERBOARD DISPLAY ====================
            AddTranslation("leaderboard_header",
                "LEADERBOARD", "CLASIFICACIÓN");

            // ==================== LOGOUT CONFIRM ====================
            AddTranslation("logout_confirm_title",
                "Logout?", "¿Cerrar Sesión?");
            AddTranslation("logout_confirm_message",
                "Are you sure you want to logout?", "¿Estás seguro de que quieres cerrar sesión?");

            // ==================== SCORES TABS ====================
            AddTranslation("personal_tab",
                "Personal", "Personal");
            AddTranslation("personal_best_time",
                "Personal Best Time", "Mejor Tiempo Personal");

            // ==================== SEARCH OPTIONS ====================
            AddTranslation("search_options_title",
                "Search Options", "Opciones de Búsqueda");

            // ==================== CREATE TOURNAMENT ====================
            AddTranslation("create_tournament_title",
                "Create Tournament", "Crear Torneo");

            // ==================== BUTTONS ====================
            AddTranslation("later_button",
                "Later", "Más tarde");

            // ==================== ERROR MESSAGES - USERNAME ====================
            AddTranslation("error_username_empty",
                "You need a player name!", "¡Necesitas un nombre de jugador!");
            AddTranslation("error_username_too_short",
                "Name is too short (minimum 3 characters)", "El nombre es muy corto (mínimo 3 caracteres)");
            AddTranslation("error_username_too_long",
                "Name is too long (maximum 20 characters)", "El nombre es muy largo (máximo 20 caracteres)");
            AddTranslation("error_username_invalid_chars",
                "Only letters, numbers and underscores", "Solo letras, números y guiones bajos");
            AddTranslation("error_username_taken",
                "That name is already taken, try another", "Ese nombre ya está tomado, prueba otro");

            // ==================== ERROR MESSAGES - EMAIL ====================
            AddTranslation("error_email_empty",
                "Enter your email address", "Ingresa tu correo electrónico");
            AddTranslation("error_email_invalid",
                "Hmm... that email doesn't look valid", "Hmm... ese correo no parece válido");
            AddTranslation("error_email_already_registered",
                "This email already has an account", "Este correo ya tiene una cuenta");

            // ==================== ERROR MESSAGES - PASSWORD ====================
            AddTranslation("error_password_empty",
                "Create a password", "Crea una contraseña");
            AddTranslation("error_password_too_short",
                "Password is too short (minimum 6 characters)", "La contraseña es muy corta (mínimo 6 caracteres)");
            AddTranslation("error_password_weak",
                "Add numbers or symbols for more security", "Agrega números o símbolos para mayor seguridad");

            // ==================== ERROR MESSAGES - CONFIRM PASSWORD ====================
            AddTranslation("error_confirm_password_empty",
                "Confirm your password", "Confirma tu contraseña");
            AddTranslation("error_passwords_not_match",
                "Passwords don't match", "Las contraseñas no coinciden");

            // ==================== ERROR MESSAGES - GENERAL / NETWORK ====================
            AddTranslation("error_no_connection",
                "No internet connection. Check your network", "Sin conexión a internet. Revisa tu red");
            AddTranslation("error_server",
                "Something went wrong. Try again", "Algo salió mal. Intenta de nuevo");
            AddTranslation("error_timeout",
                "Server took too long. Try again", "El servidor tardó mucho. Intenta de nuevo");

            // ==================== ERROR MESSAGES - LOGIN SPECIFIC ====================
            AddTranslation("error_user_not_found",
                "User not found", "Usuario no encontrado");
            AddTranslation("error_wrong_password",
                "Incorrect password", "Contraseña incorrecta");
            AddTranslation("error_auth_generic",
                "Authentication error. Try again", "Error de autenticación. Intenta nuevamente");

            // ==================== ERROR MESSAGES - REGISTER SPECIFIC ====================
            AddTranslation("error_create_account",
                "Could not create account. Try again", "No se pudo crear la cuenta. Intenta de nuevo");
            AddTranslation("error_save_username",
                "Error saving username", "Error al guardar el nombre de usuario");

            // ==================== ERROR PANEL UI ====================
            AddTranslation("ErrorText",
                "Error", "Error");
            AddTranslation("ErrorButtonText",
                "Accept", "Aceptar");

            // ==================== TOURNAMENT FILTERS ====================
            AddTranslation("min_time",
                "Min Time", "Tiempo Mín");
            AddTranslation("max_time",
                "Max Time", "Tiempo Máx");
            AddTranslation("min_players",
                "Min Players", "Mín. Jugadores");
            AddTranslation("type",
                "Type", "Tipo");

            // ==================== REGISTER SCREEN ====================
            AddTranslation("register_title",
                "Create an account", "Crea una cuenta");
            AddTranslation("username_input_placeholder",
                "Username", "Nombre de Usuario");
            AddTranslation("confirm_password_placeholder",
                "Confirm Password", "Confirmar Contraseña");
            AddTranslation("create_account_button",
                "Create Account", "Crear Cuenta");

            // ==================== BOOT ERRORS ====================
            AddTranslation("boot_error_firebase",
                "Could not connect to services", "No se pudo conectar a los servicios");
            AddTranslation("boot_error_no_internet",
                "Internet connection required", "Se requiere conexión a internet");
            AddTranslation("boot_error_timeout",
                "Connection timed out. Check your internet", "Conexión agotada. Revisa tu internet");
            AddTranslation("boot_retry_button",
                "Retry", "Reintentar");
            AddTranslation("boot_exit_button",
                "Exit", "Salir");

            // ==================== ERROR MESSAGES - ADDITIONAL ====================
            AddTranslation("error_session_expired",
                "Your session expired. Please login again", "Tu sesión expiró. Inicia sesión de nuevo");
            AddTranslation("error_account_disabled",
                "Your account has been suspended", "Tu cuenta ha sido suspendida");
            AddTranslation("error_account_not_found",
                "No account exists with that email", "No existe una cuenta con ese correo");
            AddTranslation("error_wrong_credentials",
                "Incorrect email or password", "Email o contraseña incorrectos");
            AddTranslation("error_too_many_attempts",
                "Too many attempts. Wait a few minutes", "Demasiados intentos. Espera unos minutos");
            AddTranslation("error_google_auth",
                "Error signing in with Google", "Error al iniciar con Google");
            AddTranslation("error_google_auth_cancelled",
                "Google sign in cancelled", "Inicio con Google cancelado");
            AddTranslation("error_apple_auth",
                "Error signing in with Apple", "Error al iniciar con Apple");
            AddTranslation("error_apple_auth_cancelled",
                "Apple sign in cancelled", "Inicio con Apple cancelado");
            AddTranslation("sign_in_apple",
                "Sign in with Apple", "Iniciar con Apple");
            AddTranslation("sign_in_google",
                "Sign in with Google", "Iniciar con Google");
            AddTranslation("error_register_email_empty",
                "Enter your email address", "Ingresa tu correo electrónico");
            AddTranslation("error_register_email_invalid",
                "That email doesn't look valid", "Ese correo no parece válido");
            AddTranslation("error_register_password_empty",
                "Create a password", "Crea una contraseña");

            // ==================== ERROR MESSAGES - PROFILE/SETTINGS ====================
            AddTranslation("error_loading_profile",
                "Error loading your profile", "Error al cargar tu perfil");
            AddTranslation("error_name_empty",
                "Name cannot be empty", "El nombre no puede estar vacío");
            AddTranslation("error_name_taken",
                "That name is already taken", "Ese nombre ya está tomado");
            AddTranslation("error_changing_name",
                "Error changing name", "Error al cambiar nombre");
            AddTranslation("error_logout",
                "Error logging out", "Error al cerrar sesión");
            AddTranslation("error_deleting_account",
                "Error deleting account", "Error al eliminar cuenta");
            AddTranslation("confirm_delete_account",
                "Delete your account? This action cannot be undone", "¿Eliminar tu cuenta? Esta acción no se puede deshacer");

            // ==================== ERROR MESSAGES - SCORES ====================
            AddTranslation("error_saving_score",
                "Error saving your score", "Error al guardar tu puntuación");
            AddTranslation("error_loading_scores",
                "Error loading scores", "Error al cargar puntuaciones");
            AddTranslation("error_scores_need_connection",
                "Connect to internet to see rankings", "Conecta a internet para ver rankings");
            AddTranslation("error_no_personal_scores",
                "You don't have any scores yet", "Aún no tienes puntuaciones");
            AddTranslation("error_no_global_scores",
                "No scores yet", "No hay puntuaciones todavía");

            // ==================== ERROR MESSAGES - TOURNAMENTS ====================
            AddTranslation("error_tournaments_need_connection",
                "Tournaments require internet connection", "Los torneos requieren conexión a internet");
            AddTranslation("error_loading_tournaments",
                "Error loading tournaments", "Error al cargar torneos");
            AddTranslation("error_tournament_not_found",
                "Tournament not found", "Torneo no encontrado");
            AddTranslation("error_invalid_code",
                "Invalid tournament code", "Código de torneo inválido");
            AddTranslation("error_tournament_full",
                "This tournament is full", "Este torneo está lleno");
            AddTranslation("error_tournament_expired",
                "This tournament has ended", "Este torneo ya terminó");
            AddTranslation("error_already_in_tournament",
                "You're already in this tournament", "Ya estás participando en este torneo");
            AddTranslation("error_joining_tournament",
                "Error joining tournament", "Error al unirse al torneo");
            AddTranslation("error_leaving_tournament",
                "Error leaving tournament", "Error al salir del torneo");
            AddTranslation("error_not_premium",
                "You need Premium to create tournaments", "Necesitas Premium para crear torneos");
            AddTranslation("error_creating_tournament",
                "Error creating tournament", "Error al crear torneo");
            AddTranslation("error_tournament_limit",
                "You have the maximum active tournaments", "Ya tienes el máximo de torneos activos");

            // ==================== ERROR MESSAGES - ADS ====================
            AddTranslation("error_loading_ad",
                "Error loading ad", "Error al cargar anuncio");
            AddTranslation("error_no_ads_available",
                "Ad not available. Try again later", "Anuncio no disponible. Intenta más tarde");

            // ==================== REGISTER - ADDITIONAL ====================
            AddTranslation("already_have_account",
                "Already have an account?", "¿Ya tienes una cuenta?");
            AddTranslation("back_to_login",
                "Back to Login", "Volver a Iniciar Sesión");

            // ==================== PREMIUM - ADDITIONAL ====================
            AddTranslation("premium_button",
                "Premium", "Premium");
            AddTranslation("premium_banner",
                "Go Premium!", "¡Hazte Premium!");
            AddTranslation("tired_of_ads",
                "Tired of ads?", "¿Cansado de anuncios?");
            AddTranslation("remove_ads_now",
                "Remove ads now", "Quita los anuncios ahora");
            AddTranslation("no_thanks",
                "No thanks", "No gracias");
            AddTranslation("premium_unlock_tournaments",
                "Unlock tournament creation!", "¡Desbloquea la creación de torneos!");
            AddTranslation("purchase_error",
                "Purchase failed. Try again later", "Error en la compra. Intenta más tarde");
            AddTranslation("restore_success",
                "Purchases restored successfully!", "¡Compras restauradas exitosamente!");
            AddTranslation("restore_error",
                "Could not restore purchases", "No se pudieron restaurar las compras");
            AddTranslation("restore_nothing",
                "No purchases to restore", "No hay compras para restaurar");

            // ==================== SEARCH ====================
            AddTranslation("search_tournament",
                "Search Tournament", "Buscar Torneo");
            AddTranslation("search_options",
                "Search Options", "Opciones de Búsqueda");
            AddTranslation("username_search_placeholder",
                "Search by username...", "Buscar por usuario...");

            // ==================== THEMES ====================
            AddTranslation("theme_selector_title",
                "Select Theme", "Seleccionar Tema");
            AddTranslation("theme_current",
                "Current Theme", "Tema Actual");
            AddTranslation("theme_preview",
                "Preview", "Vista Previa");
            AddTranslation("theme_apply",
                "Apply Theme", "Aplicar Tema");
            AddTranslation("theme_neon_dark",
                "Neon Dark", "Neón Oscuro");
            AddTranslation("theme_clean_light",
                "Clean Light", "Luz Limpia");
            AddTranslation("theme_retro_arcade",
                "Retro Arcade", "Arcade Retro");
            AddTranslation("theme_ocean",
                "Ocean", "Océano");
            AddTranslation("theme_volcano",
                "Volcano", "Volcán");
            AddTranslation("theme_cyberpunk",
                "Cyberpunk", "Cyberpunk");
            AddTranslation("theme_premium_required",
                "Premium theme", "Tema Premium");
            AddTranslation("change_theme",
                "Change Theme", "Cambiar Tema");
            AddTranslation("change_style",
                "Change Style", "Cambiar Estilo");

            // ==================== STYLES PRO PROMPT ====================
            AddTranslation("styles_pro_title",
                "Styles PRO", "Estilos PRO");
            AddTranslation("styles_pro_unlock_themes",
                "Unlock 5 exclusive themes:", "Desbloquea 5 temas exclusivos:");
            AddTranslation("styles_pro_price",
                "$29 MXN", "$29 MXN");
            AddTranslation("close_button",
                "Close", "Cerrar");
            AddTranslation("cancel",
                "Cancel", "Cancelar");
            AddTranslation("theme_clean_light_desc",
                "Professional", "Profesional");
            AddTranslation("theme_cyberpunk_desc",
                "Futuristic", "Futurista");
            AddTranslation("theme_ocean_desc",
                "Relaxing", "Relajante");
            AddTranslation("theme_retro_desc",
                "Nostalgic", "Nostálgico");
            AddTranslation("theme_volcano_desc",
                "Intense", "Intenso");

            // ==================== INPUT HINTS ====================
            AddTranslation("hint_username",
                "3-20 characters. Letters, numbers and _ only", "3-20 caracteres. Solo letras, números y _");
            AddTranslation("hint_email",
                "Enter a valid email address", "Ingresa un correo válido");
            AddTranslation("hint_password",
                "Minimum 6 characters", "Mínimo 6 caracteres");
            AddTranslation("hint_confirm_password",
                "Repeat your password", "Repite tu contraseña");
            AddTranslation("placeholder_username",
                "Username (3-20 chars)", "Usuario (3-20 chars)");
            AddTranslation("placeholder_email",
                "email@example.com", "correo@ejemplo.com");
            AddTranslation("placeholder_password",
                "Password (min 6)", "Contraseña (mín 6)");
            AddTranslation("placeholder_confirm",
                "Confirm password", "Confirmar contraseña");

            // ==================== NETWORK STATUS ====================
            AddTranslation("net_offline",
                "No internet connection", "Sin conexion a internet");
            AddTranslation("net_reconnecting",
                "Reconnecting...", "Reconectando...");
            AddTranslation("net_restored",
                "Connection restored", "Conexion restaurada");
            AddTranslation("net_mobile_warning",
                "Using mobile data", "Usando datos moviles");

            // ==================== DEEP LINKING / SHARING ====================
            AddTranslation("share_profile",
                "Check my profile on digitPark!", "Mira mi perfil en digitPark!");
            AddTranslation("share_tournament",
                "Join this tournament!", "Unete a este torneo!");
            AddTranslation("share_match_result",
                "I scored {0} points!", "Obtuve {0} puntos!");
            AddTranslation("btn_share",
                "Share", "Compartir");
            AddTranslation("btn_copy_link",
                "Copy Link", "Copiar enlace");

            // ==================== ACCESSIBILITY LABELS ====================
            AddTranslation("acc_btn_play",
                "Play game", "Jugar");
            AddTranslation("acc_btn_settings",
                "Open settings", "Abrir configuracion");
            AddTranslation("acc_btn_shop",
                "Open shop", "Abrir tienda");
            AddTranslation("acc_btn_profile",
                "View profile", "Ver perfil");
            AddTranslation("acc_btn_login",
                "Sign in button", "Boton iniciar sesion");
            AddTranslation("acc_btn_register",
                "Create account button", "Boton crear cuenta");
            AddTranslation("acc_btn_back",
                "Go back", "Volver atras");
            AddTranslation("acc_btn_close",
                "Close", "Cerrar");
            AddTranslation("acc_input_email",
                "Email input field", "Campo de correo electronico");
            AddTranslation("acc_input_password",
                "Password input field", "Campo de contrasena");
            AddTranslation("acc_input_username",
                "Username input field", "Campo de nombre de usuario");
            AddTranslation("acc_label_coins",
                "Coins balance", "Saldo de monedas");
            AddTranslation("acc_label_gems",
                "Gems balance", "Saldo de gemas");
            AddTranslation("acc_label_trophies",
                "Trophies count", "Cantidad de trofeos");
            AddTranslation("acc_label_balance",
                "Cash balance", "Saldo en efectivo");
            AddTranslation("acc_btn_deposit",
                "Deposit money", "Depositar dinero");
            AddTranslation("acc_btn_withdraw",
                "Withdraw money", "Retirar dinero");
            AddTranslation("acc_btn_start_game",
                "Start game", "Iniciar juego");
            AddTranslation("acc_btn_find_match",
                "Find match", "Buscar partida");
            AddTranslation("acc_btn_cancel",
                "Cancel action", "Cancelar accion");
            AddTranslation("acc_btn_tournaments",
                "View tournaments", "Ver torneos");
            AddTranslation("acc_btn_scores",
                "View scores", "Ver puntuaciones");
            AddTranslation("acc_btn_friends",
                "View friends", "Ver amigos");
            AddTranslation("acc_btn_achievements",
                "View achievements", "Ver logros");
            AddTranslation("acc_btn_daily_reward",
                "Claim daily reward", "Reclamar recompensa diaria");
            AddTranslation("acc_label_game_timer",
                "Game timer", "Temporizador del juego");
            AddTranslation("acc_label_game_score",
                "Current score", "Puntuacion actual");
            AddTranslation("acc_btn_play_again",
                "Play again", "Jugar de nuevo");
            AddTranslation("acc_btn_main_menu",
                "Return to main menu", "Volver al menu principal");

            // ==================== DAILY REWARDS ====================
            AddTranslation("dr_title",
                "DAILY REWARDS", "RECOMPENSAS DIARIAS");
            AddTranslation("dr_streak",
                "Streak: {0} days", "Racha: {0} dias");
            AddTranslation("dr_bonus_day",
                "Day {0} bonus: +{1} gems", "Bonus dia {0}: +{1} gemas");
            AddTranslation("dr_available_now",
                "Available now!", "Disponible ahora!");
            AddTranslation("dr_next_in",
                "Next in: {0}", "Proximo en: {0}");
            AddTranslation("dr_claim",
                "CLAIM REWARD", "RECLAMAR RECOMPENSA");
            AddTranslation("dr_claimed",
                "Already claimed", "Ya reclamado");
            AddTranslation("dr_day",
                "DAY {0}", "DIA {0}");
            AddTranslation("dr_today",
                "TODAY!", "HOY!");
            AddTranslation("dr_milestone",
                "{0} days in a row!", "{0} dias seguidos!");
            AddTranslation("dr_milestone_bonus",
                "+{0} bonus gems", "+{0} gemas de bonus");
            AddTranslation("dr_grand_prize",
                "DAY 7 - GRAND PRIZE", "DIA 7 - GRAN PREMIO");
            AddTranslation("dr_unlocks_in",
                "Unlocks in {0} days", "Se desbloquea en {0} dias");
            AddTranslation("dr_week",
                "WEEK {0}", "SEMANA {0}");
            AddTranslation("dr_today_reward",
                "TODAY'S REWARD", "RECOMPENSA DE HOY");
            AddTranslation("dr_next_reward",
                "Next reward in:", "Proxima recompensa en:");

            // ==================== REWARD TYPES ====================
            AddTranslation("reward_coins",
                "DigitCoins", "DigitCoins");
            AddTranslation("reward_gems",
                "DigitGems", "DigitGems");
            AddTranslation("reward_xp",
                "XP", "XP");

            // ==================== DAILY MISSIONS ====================
            AddTranslation("ms_title",
                "MISSIONS", "MISIONES");
            AddTranslation("ms_refresh_in",
                "Resets in: {0}", "Reinicio en: {0}");
            AddTranslation("ms_rewards_earned",
                "{0} rewards earned", "{0} recompensas ganadas");
            AddTranslation("ms_progress",
                "{0}/{1} missions completed", "{0}/{1} misiones completadas");
            AddTranslation("ms_bonus",
                "Bonus: +{0} coins", "Bonus: +{0} monedas");
            AddTranslation("ms_completed",
                "Completed", "Completada");
            AddTranslation("ms_ready_claim",
                "Ready to claim!", "Lista para reclamar!");
            AddTranslation("ms_in_progress",
                "In Progress", "En Progreso");
            AddTranslation("ms_no_missions",
                "No missions available", "No hay misiones disponibles");
            AddTranslation("ms_tab_daily",
                "Daily", "Diarias");
            AddTranslation("ms_tab_weekly",
                "Weekly", "Semanales");
            AddTranslation("ms_tab_special",
                "Special", "Especiales");
            AddTranslation("ms_weekly_header",
                "WEEKLY MISSIONS", "MISIONES SEMANALES");
            AddTranslation("ms_special_header",
                "SPECIAL MISSIONS", "MISIONES ESPECIALES");
            AddTranslation("ms_daily_progress",
                "Daily Progress", "Progreso Diario");

            // Mission titles
            AddTranslation("ms_daily_play_3_title",
                "Active Player", "Jugador Activo");
            AddTranslation("ms_daily_play_3_desc",
                "Play 3 matches", "Juega 3 partidas");
            AddTranslation("ms_daily_win_1_title",
                "First Victory", "Primera Victoria");
            AddTranslation("ms_daily_win_1_desc",
                "Win 1 match", "Gana 1 partida");
            AddTranslation("ms_daily_score_1000_title",
                "Point Hunter", "Cazador de Puntos");
            AddTranslation("ms_daily_score_1000_desc",
                "Get 1000 total points", "Obten 1000 puntos totales");
            AddTranslation("ms_daily_complete_minigame_title",
                "Explorer", "Explorador");
            AddTranslation("ms_daily_complete_minigame_desc",
                "Complete any minigame", "Completa cualquier minijuego");
            AddTranslation("ms_daily_play_memory_title",
                "Elephant Memory", "Memoria de Elefante");
            AddTranslation("ms_daily_play_memory_desc",
                "Play 2 Memory Pairs matches", "Juega 2 partidas de Memory Pairs");
            AddTranslation("ms_daily_perfect_round_title",
                "Perfectionist", "Perfeccionista");
            AddTranslation("ms_daily_perfect_round_desc",
                "Get a perfect round", "Obten ronda perfecta");
            AddTranslation("ms_weekly_play_20_title",
                "Marathon Runner", "Maratonista");
            AddTranslation("ms_weekly_play_20_desc",
                "Play 20 matches this week", "Juega 20 partidas esta semana");
            AddTranslation("ms_weekly_win_10_title",
                "Weekly Champion", "Campeon Semanal");
            AddTranslation("ms_weekly_win_10_desc",
                "Win 10 matches", "Gana 10 partidas");
            AddTranslation("ms_weekly_all_games_title",
                "Versatile", "Versatil");
            AddTranslation("ms_weekly_all_games_desc",
                "Play all minigames", "Juega todos los minijuegos");
            AddTranslation("ms_weekly_streak_5_title",
                "On a Roll", "En Racha");
            AddTranslation("ms_weekly_streak_5_desc",
                "Keep a 5 win streak", "Manten racha de 5 victorias");
            AddTranslation("ms_weekly_tournament_title",
                "Competitor", "Competidor");
            AddTranslation("ms_weekly_tournament_desc",
                "Join a tournament", "Participa en un torneo");
            AddTranslation("ms_special_master_title",
                "Grand Master", "Gran Maestro");
            AddTranslation("ms_special_master_desc",
                "Reach level 10", "Alcanza nivel 10");
            AddTranslation("ms_special_social_title",
                "Influencer", "Influencer");
            AddTranslation("ms_special_social_desc",
                "Share the game 5 times", "Comparte el juego 5 veces");
            AddTranslation("ms_special_collector_title",
                "Collector", "Coleccionista");
            AddTranslation("ms_special_collector_desc",
                "Unlock 10 avatars", "Desbloquea 10 avatares");
        }

        private void AddTranslation(string key, string english, string spanish)
        {
            // Only add if not already loaded from Translations.txt (file is the authoritative source)
            if (textDictionary.ContainsKey(key)) return;
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
            if (savedIndex >= 0 && savedIndex < LanguageNames.Length)
            {
                currentLanguage = (Language)savedIndex;
            }
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
                return GetTextForLanguage(text, currentLanguage);
            }

            Debug.LogWarning($"[Localization] Clave no encontrada: {key}");
            return key;
        }

        /// <summary>
        /// Obtiene el texto formateado con parámetros
        /// </summary>
        public string GetText(string key, params object[] args)
        {
            string text = GetText(key);
            try
            {
                return string.Format(text, args);
            }
            catch
            {
                return text;
            }
        }

        private string GetTextForLanguage(LocalizedText text, Language language)
        {
            switch (language)
            {
                case Language.English: return text.english;
                case Language.Spanish: return text.spanish;
                default: return text.english;
            }
        }

        /// <summary>
        /// Cambia el idioma por índice
        /// </summary>
        public void SetLanguage(int index)
        {
            if (index < 0 || index >= LanguageNames.Length) return;

            Language newLanguage = (Language)index;

            if (currentLanguage != newLanguage)
            {
                currentLanguage = newLanguage;
                PlayerPrefs.SetInt(LANGUAGE_KEY, index);
                PlayerPrefs.Save();

                Debug.Log($"[Localization] Idioma cambiado a: {LanguageNames[index]}");

                // Notificar a todos los textos via evento
                int subscriberCount = OnLanguageChanged?.GetInvocationList()?.Length ?? 0;
                Debug.Log($"[Localization] Notificando a {subscriberCount} suscriptores...");
                OnLanguageChanged?.Invoke();

                // Backup: llamar directamente al AutoLocalizer si existe
                if (AutoLocalizer.Instance != null)
                {
                    Debug.Log("[Localization] Llamando AutoLocalizer directamente...");
                    AutoLocalizer.Instance.LocalizeAllTexts();
                }
            }
        }

        /// <summary>
        /// Cambia el idioma por enum
        /// </summary>
        public void SetLanguage(Language language)
        {
            SetLanguage((int)language);
        }

        /// <summary>
        /// Obtiene el índice del idioma actual
        /// </summary>
        public int GetCurrentLanguageIndex()
        {
            return (int)currentLanguage;
        }

        /// <summary>
        /// Obtiene el idioma actual como enum
        /// </summary>
        public Language GetCurrentLanguage()
        {
            return currentLanguage;
        }

        /// <summary>
        /// Obtiene el nombre del idioma actual
        /// </summary>
        public string GetCurrentLanguageName()
        {
            return LanguageNames[(int)currentLanguage];
        }

        /// <summary>
        /// Obtiene el código del idioma actual (en, es, fr, pt, de)
        /// </summary>
        public string GetCurrentLanguageCode()
        {
            return LanguageNativeCodes[(int)currentLanguage];
        }

        /// <summary>
        /// Obtiene el número total de idiomas disponibles
        /// </summary>
        public int GetLanguageCount()
        {
            return LanguageNames.Length;
        }

        /// <summary>
        /// Verifica si existe una traducción para una clave
        /// </summary>
        public bool HasTranslation(string key)
        {
            return textDictionary != null && textDictionary.ContainsKey(key);
        }
    }
}
