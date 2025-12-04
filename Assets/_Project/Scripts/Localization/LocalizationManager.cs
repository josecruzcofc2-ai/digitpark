using UnityEngine;
using System.Collections.Generic;
using System;

namespace DigitPark.Localization
{
    public enum Language
    {
        English = 0,
        Spanish = 1,
        French = 2,
        Portuguese = 3,
        German = 4
    }

    [Serializable]
    public class LocalizedText
    {
        public string key;
        public string english;
        public string spanish;
        public string french;
        public string portuguese;
        public string german;
    }

    /// <summary>
    /// Sistema de localización para múltiples idiomas
    /// Soporta: English, Español, Français, Português, Deutsch
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
        private Language currentLanguage = Language.English;

        // Evento para notificar cambios de idioma
        public static event Action OnLanguageChanged;

        private const string LANGUAGE_KEY = "Language";

        // Nombres de idiomas para mostrar en UI
        public static readonly string[] LanguageNames = { "English", "Español", "Français", "Português", "Deutsch" };
        public static readonly string[] LanguageNativeCodes = { "en", "es", "fr", "pt", "de" };

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
            var existingAutoLocalizer = FindObjectOfType<AutoLocalizer>();

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
            // ==================== LOGIN ====================
            AddTranslation("login_title",
                "Login", "Iniciar Sesión", "Connexion", "Entrar", "Anmelden");
            AddTranslation("email_placeholder",
                "Email", "Correo Electrónico", "E-mail", "E-mail", "E-Mail");
            AddTranslation("password_placeholder",
                "Password", "Contraseña", "Mot de passe", "Senha", "Passwort");
            AddTranslation("login_button",
                "Sign In", "Iniciar Sesión", "Se connecter", "Entrar", "Anmelden");
            AddTranslation("register_button",
                "Create Account", "Crear Cuenta", "Créer un compte", "Criar Conta", "Konto erstellen");
            AddTranslation("remember_me",
                "Remember Me", "Recordarme", "Se souvenir de moi", "Lembrar-me", "Angemeldet bleiben");
            AddTranslation("forgot_password",
                "Forgot Password?", "¿Olvidaste tu contraseña?", "Mot de passe oublié?", "Esqueceu a senha?", "Passwort vergessen?");
            AddTranslation("or_continue_with",
                "Or continue with", "O continúa con", "Ou continuer avec", "Ou continue com", "Oder fortfahren mit");

            // ==================== MAIN MENU ====================
            AddTranslation("play_button",
                "Play", "Jugar", "Jouer", "Jogar", "Spielen");
            AddTranslation("scores_button",
                "Scores", "Puntuaciones", "Scores", "Pontuações", "Punkte");
            AddTranslation("tournament_button",
                "Tournaments", "Torneos", "Tournois", "Torneios", "Turniere");
            AddTranslation("settings_button",
                "Settings", "Configuración", "Paramètres", "Configurações", "Einstellungen");
            AddTranslation("no_username",
                "No Username", "Sin Usuario", "Sans nom", "Sem nome", "Kein Name");

            // ==================== SETTINGS ====================
            AddTranslation("settings_title",
                "Settings", "Configuración", "Paramètres", "Configurações", "Einstellungen");
            AddTranslation("volume_sound",
                "Sound Volume", "Volumen de Sonido", "Volume du son", "Volume do Som", "Lautstärke");
            AddTranslation("volume_effects",
                "Effects Volume", "Volumen de Efectos", "Volume des effets", "Volume dos Efeitos", "Effektlautstärke");
            AddTranslation("change_name",
                "Change Username", "Cambiar Nombre", "Changer le nom", "Mudar Nome", "Namen ändern");
            AddTranslation("logout_button",
                "Logout", "Cerrar Sesión", "Déconnexion", "Sair", "Abmelden");
            AddTranslation("delete_account",
                "Delete Account", "Eliminar Cuenta", "Supprimer le compte", "Excluir Conta", "Konto löschen");
            AddTranslation("language",
                "Language", "Idioma", "Langue", "Idioma", "Sprache");
            AddTranslation("back_button",
                "Back", "Volver", "Retour", "Voltar", "Zurück");

            // ==================== CHANGE NAME PANEL ====================
            AddTranslation("change_name_title",
                "Change Username", "Cambiar Nombre de Usuario", "Changer le nom d'utilisateur", "Mudar Nome de Usuário", "Benutzernamen ändern");
            AddTranslation("new_name_placeholder",
                "New username", "Nuevo nombre", "Nouveau nom", "Novo nome", "Neuer Name");
            AddTranslation("confirm_button",
                "Confirm", "Confirmar", "Confirmer", "Confirmar", "Bestätigen");
            AddTranslation("cancel_button",
                "Cancel", "Cancelar", "Annuler", "Cancelar", "Abbrechen");

            // ==================== DELETE ACCOUNT ====================
            AddTranslation("delete_confirm_title",
                "Delete Account?", "¿Eliminar Cuenta?", "Supprimer le compte?", "Excluir Conta?", "Konto löschen?");
            AddTranslation("delete_confirm_message",
                "This action cannot be undone", "Esta acción no se puede deshacer", "Cette action est irréversible", "Esta ação não pode ser desfeita", "Diese Aktion kann nicht rückgängig gemacht werden");
            AddTranslation("delete_button",
                "Delete", "Eliminar", "Supprimer", "Excluir", "Löschen");

            // ==================== GAME ====================
            AddTranslation("timer_label",
                "Time", "Tiempo", "Temps", "Tempo", "Zeit");
            AddTranslation("best_time",
                "Best Time", "Mejor Tiempo", "Meilleur temps", "Melhor Tempo", "Bestzeit");
            AddTranslation("best_label",
                "Best:", "Mejor:", "Meilleur:", "Melhor:", "Beste:");
            AddTranslation("no_best_time",
                "Best: --", "Mejor: --", "Meilleur: --", "Melhor: --", "Beste: --");
            AddTranslation("play_again",
                "Play Again", "Jugar de Nuevo", "Rejouer", "Jogar Novamente", "Nochmal spielen");
            AddTranslation("new_record",
                "New Record!", "¡Nuevo Récord!", "Nouveau record!", "Novo Recorde!", "Neuer Rekord!");

            // ==================== SUCCESS MESSAGES - Level 1 (Basic) ====================
            AddTranslation("msg_good_job",
                "Good job!", "¡Buen trabajo!", "Bon travail!", "Bom trabalho!", "Gut gemacht!");
            AddTranslation("msg_complete",
                "Complete!", "¡Completado!", "Terminé!", "Completo!", "Fertig!");
            AddTranslation("msg_nice_try",
                "Nice try!", "¡Buen intento!", "Bel essai!", "Boa tentativa!", "Guter Versuch!");
            AddTranslation("msg_well_done",
                "Well done!", "¡Bien hecho!", "Bien joué!", "Muito bem!", "Gut gemacht!");
            AddTranslation("msg_task_complete",
                "Task complete!", "¡Tarea completada!", "Tâche terminée!", "Tarefa completa!", "Aufgabe erledigt!");

            // ==================== SUCCESS MESSAGES - Level 2 (Decent) ====================
            AddTranslation("msg_great_work",
                "Great work!", "¡Gran trabajo!", "Super travail!", "Ótimo trabalho!", "Tolle Arbeit!");
            AddTranslation("msg_good_timing",
                "Good timing!", "¡Buen tiempo!", "Bon timing!", "Bom tempo!", "Gutes Timing!");
            AddTranslation("msg_not_bad",
                "Not bad!", "¡Nada mal!", "Pas mal!", "Nada mal!", "Nicht schlecht!");
            AddTranslation("msg_solid",
                "Solid performance!", "¡Sólido rendimiento!", "Performance solide!", "Desempenho sólido!", "Solide Leistung!");
            AddTranslation("msg_keep_it_up",
                "Keep it up!", "¡Sigue así!", "Continue comme ça!", "Continue assim!", "Weiter so!");

            // ==================== SUCCESS MESSAGES - Level 3 (Good) ====================
            AddTranslation("msg_excellent",
                "Excellent!", "¡Excelente!", "Excellent!", "Excelente!", "Ausgezeichnet!");
            AddTranslation("msg_impressive",
                "Impressive!", "¡Impresionante!", "Impressionnant!", "Impressionante!", "Beeindruckend!");
            AddTranslation("msg_great_speed",
                "Great speed!", "¡Gran velocidad!", "Super vitesse!", "Ótima velocidade!", "Tolle Geschwindigkeit!");
            AddTranslation("msg_well_played",
                "Well played!", "¡Bien jugado!", "Bien joué!", "Bem jogado!", "Gut gespielt!");
            AddTranslation("msg_awesome",
                "Awesome job!", "¡Increíble!", "Super boulot!", "Incrível!", "Fantastisch!");

            // ==================== SUCCESS MESSAGES - Level 4 (Very Good) ====================
            AddTranslation("msg_amazing",
                "Amazing!", "¡Asombroso!", "Incroyable!", "Incrível!", "Erstaunlich!");
            AddTranslation("msg_outstanding",
                "Outstanding!", "¡Sobresaliente!", "Remarquable!", "Excelente!", "Hervorragend!");
            AddTranslation("msg_superb",
                "Superb timing!", "¡Tiempo soberbio!", "Timing superbe!", "Tempo soberbo!", "Hervorragendes Timing!");
            AddTranslation("msg_incredible",
                "Incredible speed!", "¡Velocidad increíble!", "Vitesse incroyable!", "Velocidade incrível!", "Unglaubliche Geschwindigkeit!");
            AddTranslation("msg_spectacular",
                "Spectacular!", "¡Espectacular!", "Spectaculaire!", "Espetacular!", "Spektakulär!");
            AddTranslation("msg_on_fire",
                "You're on fire!", "¡Estás en llamas!", "Tu es en feu!", "Você está pegando fogo!", "Du bist on fire!");

            // ==================== SUCCESS MESSAGES - Level 5 (Perfect) ====================
            AddTranslation("msg_perfect",
                "PERFECT!", "¡PERFECTO!", "PARFAIT!", "PERFEITO!", "PERFEKT!");
            AddTranslation("msg_legendary",
                "LEGENDARY!", "¡LEGENDARIO!", "LÉGENDAIRE!", "LENDÁRIO!", "LEGENDÄR!");
            AddTranslation("msg_mind_blowing",
                "MIND BLOWING!", "¡ALUCINANTE!", "ÉPOUSTOUFLANT!", "INACREDITÁVEL!", "UNGLAUBLICH!");
            AddTranslation("msg_master",
                "ABSOLUTE MASTER!", "¡MAESTRO ABSOLUTO!", "MAÎTRE ABSOLU!", "MESTRE ABSOLUTO!", "ABSOLUTER MEISTER!");
            AddTranslation("msg_unstoppable",
                "UNSTOPPABLE!", "¡IMPARABLE!", "INARRÊTABLE!", "IMPARÁVEL!", "UNAUFHALTSAM!");
            AddTranslation("msg_world_class",
                "WORLD CLASS!", "¡CLASE MUNDIAL!", "CLASSE MONDIALE!", "CLASSE MUNDIAL!", "WELTKLASSE!");
            AddTranslation("msg_godlike",
                "GODLIKE!", "¡DIVINO!", "DIVIN!", "DIVINO!", "GÖTTLICH!");
            AddTranslation("msg_flawless",
                "FLAWLESS VICTORY!", "¡VICTORIA PERFECTA!", "VICTOIRE PARFAITE!", "VITÓRIA PERFEITA!", "MAKELLOSER SIEG!");

            // ==================== LEADERBOARD / SCORES ====================
            AddTranslation("leaderboard_title",
                "Leaderboard", "Tabla de Posiciones", "Classement", "Classificação", "Rangliste");
            AddTranslation("global_tab",
                "Global", "Global", "Global", "Global", "Global");
            AddTranslation("country_tab",
                "Country", "País", "Pays", "País", "Land");
            AddTranslation("position",
                "Position", "Posición", "Position", "Posição", "Position");
            AddTranslation("player",
                "Player", "Jugador", "Joueur", "Jogador", "Spieler");
            AddTranslation("time",
                "Time", "Tiempo", "Temps", "Tempo", "Zeit");
            AddTranslation("loading_rankings",
                "Loading rankings...", "Cargando rankings...", "Chargement du classement...", "Carregando ranking...", "Rangliste wird geladen...");
            AddTranslation("error_loading_rankings",
                "Error loading rankings", "Error al cargar rankings", "Erreur de chargement", "Erro ao carregar ranking", "Fehler beim Laden");
            AddTranslation("your_position",
                "Your position:", "Tu posición:", "Votre position:", "Sua posição:", "Deine Position:");
            AddTranslation("your_best_time",
                "Best time:", "Mejor tiempo:", "Meilleur temps:", "Melhor tempo:", "Bestzeit:");
            AddTranslation("no_best_time_yet",
                "No best time", "Sin mejor tiempo", "Pas de meilleur temps", "Sem melhor tempo", "Keine Bestzeit");
            AddTranslation("history_games",
                "History: {0} games", "Historial: {0} partidas", "Historique: {0} parties", "Histórico: {0} partidas", "Verlauf: {0} Spiele");
            AddTranslation("no_scores_yet",
                "No scores yet\n\nPlay some games to see your scores here",
                "No hay puntuaciones aún\n\nJuega para ver tus scores aquí",
                "Pas encore de scores\n\nJouez pour voir vos scores ici",
                "Sem pontuações ainda\n\nJogue para ver seus scores aqui",
                "Noch keine Punkte\n\nSpiele um deine Punkte hier zu sehen");
            AddTranslation("no_date",
                "No date", "Sin fecha", "Pas de date", "Sem data", "Kein Datum");
            AddTranslation("invalid_date",
                "Invalid date", "Fecha inválida", "Date invalide", "Data inválida", "Ungültiges Datum");

            // ==================== TOURNAMENTS ====================
            AddTranslation("tournaments_title",
                "Tournaments", "Torneos", "Tournois", "Torneios", "Turniere");
            AddTranslation("search_tab",
                "Search", "Buscar", "Rechercher", "Buscar", "Suchen");
            AddTranslation("my_tournaments_tab",
                "My Tournaments", "Mis Torneos", "Mes Tournois", "Meus Torneios", "Meine Turniere");
            AddTranslation("create_tab",
                "Create", "Crear", "Créer", "Criar", "Erstellen");
            AddTranslation("join_tournament",
                "Join", "Unirse", "Rejoindre", "Entrar", "Beitreten");
            AddTranslation("exit_tournament",
                "Exit Tournament", "Salir del Torneo", "Quitter le tournoi", "Sair do Torneio", "Turnier verlassen");
            AddTranslation("entry_fee",
                "Entry Fee", "Cuota de Entrada", "Frais d'entrée", "Taxa de Entrada", "Eintrittsgebühr");
            AddTranslation("prize_pool",
                "Prize Pool", "Pozo de Premios", "Cagnotte", "Prêmio Total", "Preispool");
            AddTranslation("participants",
                "Participants", "Participantes", "Participants", "Participantes", "Teilnehmer");
            AddTranslation("join_confirm_message",
                "Do you want to join this tournament?", "¿Deseas unirte a este torneo?", "Voulez-vous rejoindre ce tournoi?", "Deseja entrar neste torneio?", "Möchtest du diesem Turnier beitreten?");
            AddTranslation("creator_label",
                "Creator:", "Creador:", "Créateur:", "Criador:", "Ersteller:");
            AddTranslation("time_remaining",
                "Time remaining:", "Tiempo restante:", "Temps restant:", "Tempo restante:", "Verbleibende Zeit:");
            AddTranslation("tournament_of",
                "Tournament of", "Torneo de", "Tournoi de", "Torneio de", "Turnier von");
            AddTranslation("no_active_tournaments",
                "No active tournaments", "No hay torneos activos", "Pas de tournois actifs", "Sem torneios ativos", "Keine aktiven Turniere");
            AddTranslation("not_in_tournament",
                "You're not in any tournament", "No participas en ningún torneo", "Vous n'êtes dans aucun tournoi", "Você não está em nenhum torneio", "Du bist in keinem Turnier");
            AddTranslation("create_error",
                "Could not create tournament. Try again.", "No se pudo crear el torneo. Intenta nuevamente.", "Impossible de créer le tournoi. Réessayez.", "Não foi possível criar o torneio. Tente novamente.", "Turnier konnte nicht erstellt werden. Versuche es erneut.");
            AddTranslation("join_error",
                "Could not join tournament. Try again.", "No se pudo unir al torneo. Intenta nuevamente.", "Impossible de rejoindre le tournoi. Réessayez.", "Não foi possível entrar no torneio. Tente novamente.", "Konnte dem Turnier nicht beitreten. Versuche es erneut.");
            AddTranslation("join_success",
                "You've joined the tournament!", "¡Te has unido al torneo exitosamente!", "Vous avez rejoint le tournoi!", "Você entrou no torneio!", "Du bist dem Turnier beigetreten!");
            AddTranslation("create_success",
                "Tournament created! You've been added automatically.", "¡Torneo creado exitosamente! Te has unido automáticamente.", "Tournoi créé! Vous avez été ajouté automatiquement.", "Torneio criado! Você foi adicionado automaticamente.", "Turnier erstellt! Du wurdest automatisch hinzugefügt.");
            AddTranslation("exit_success",
                "You left the tournament", "Has abandonado el torneo exitosamente", "Vous avez quitté le tournoi", "Você saiu do torneio", "Du hast das Turnier verlassen");
            AddTranslation("exit_error",
                "Could not leave tournament. Try again.", "No se pudo salir del torneo. Intenta nuevamente.", "Impossible de quitter le tournoi. Réessayez.", "Não foi possível sair do torneio. Tente novamente.", "Konnte das Turnier nicht verlassen. Versuche es erneut.");
            AddTranslation("exit_confirm_title",
                "Exit Tournament?", "¿Salir del Torneo?", "Quitter le tournoi?", "Sair do Torneio?", "Turnier verlassen?");
            AddTranslation("exit_confirm_message",
                "Are you sure you want to leave this tournament?", "¿Estás seguro de que quieres abandonar este torneo?", "Êtes-vous sûr de vouloir quitter ce tournoi?", "Tem certeza de que deseja sair deste torneio?", "Bist du sicher, dass du dieses Turnier verlassen möchtest?");
            AddTranslation("no_time",
                "No time", "Sin tiempo", "Pas de temps", "Sem tempo", "Keine Zeit");
            AddTranslation("finished",
                "Finished", "Finalizado", "Terminé", "Finalizado", "Beendet");
            AddTranslation("attempts",
                "attempts", "intentos", "essais", "tentativas", "Versuche");
            AddTranslation("try_again",
                "Try again", "Intenta nuevamente", "Réessayez", "Tente novamente", "Versuche es erneut");
            AddTranslation("max_players",
                "Max Players", "Máx. Jugadores", "Joueurs max", "Máx. Jogadores", "Max. Spieler");
            AddTranslation("duration",
                "Duration", "Duración", "Durée", "Duração", "Dauer");
            AddTranslation("public",
                "Public", "Público", "Public", "Público", "Öffentlich");
            AddTranslation("private",
                "Private", "Privado", "Privé", "Privado", "Privat");
            AddTranslation("create_tournament",
                "Create Tournament", "Crear Torneo", "Créer un tournoi", "Criar Torneio", "Turnier erstellen");

            // ==================== BOOT / LOADING ====================
            AddTranslation("boot_initializing_config",
                "Initializing settings...", "Inicializando configuración...", "Initialisation des paramètres...", "Inicializando configurações...", "Einstellungen werden initialisiert...");
            AddTranslation("boot_connecting_services",
                "Connecting to services...", "Conectando a servicios...", "Connexion aux services...", "Conectando aos serviços...", "Verbindung zu Diensten...");
            AddTranslation("boot_loading_resources",
                "Loading resources...", "Cargando recursos...", "Chargement des ressources...", "Carregando recursos...", "Ressourcen werden geladen...");
            AddTranslation("boot_verifying_user",
                "Verifying user...", "Verificando usuario...", "Vérification de l'utilisateur...", "Verificando usuário...", "Benutzer wird überprüft...");
            AddTranslation("boot_completed",
                "Completed!", "¡Completado!", "Terminé!", "Concluído!", "Fertig!");
            AddTranslation("boot_error",
                "Error initializing. Please restart.", "Error al inicializar. Por favor reinicia.", "Erreur d'initialisation. Veuillez redémarrer.", "Erro ao inicializar. Por favor reinicie.", "Initialisierungsfehler. Bitte neu starten.");

            // ==================== USERNAME POPUP ====================
            AddTranslation("username_popup_title",
                "Choose a username!", "¡Elige un nombre de usuario!", "Choisissez un nom d'utilisateur!", "Escolha um nome de usuário!", "Wähle einen Benutzernamen!");
            AddTranslation("username_placeholder",
                "Username", "Nombre de usuario", "Nom d'utilisateur", "Nome de usuário", "Benutzername");

            // ==================== CONFIRMATION POPUP ====================
            AddTranslation("current_value",
                "Current:", "Actual:", "Actuel:", "Atual:", "Aktuell:");
            AddTranslation("new_value",
                "New:", "Nuevo:", "Nouveau:", "Novo:", "Neu:");

            // ==================== GENERAL ====================
            AddTranslation("loading",
                "Loading...", "Cargando...", "Chargement...", "Carregando...", "Laden...");
            AddTranslation("error",
                "Error", "Error", "Erreur", "Erro", "Fehler");
            AddTranslation("success",
                "Success", "Éxito", "Succès", "Sucesso", "Erfolg");
            AddTranslation("yes",
                "Yes", "Sí", "Oui", "Sim", "Ja");
            AddTranslation("no",
                "No", "No", "Non", "Não", "Nein");
            AddTranslation("ok",
                "OK", "OK", "OK", "OK", "OK");
            AddTranslation("close",
                "Close", "Cerrar", "Fermer", "Fechar", "Schließen");
            AddTranslation("save",
                "Save", "Guardar", "Sauvegarder", "Salvar", "Speichern");
            AddTranslation("apply",
                "Apply", "Aplicar", "Appliquer", "Aplicar", "Anwenden");
            AddTranslation("clear",
                "Clear", "Limpiar", "Effacer", "Limpar", "Löschen");
            AddTranslation("search",
                "Search", "Buscar", "Rechercher", "Buscar", "Suchen");
            AddTranslation("filter",
                "Filter", "Filtrar", "Filtrer", "Filtrar", "Filtern");
            AddTranslation("options",
                "Options", "Opciones", "Options", "Opções", "Optionen");

            // ==================== TIME FORMATS ====================
            AddTranslation("time_days_hours",
                "{0}d {1}h", "{0}d {1}h", "{0}j {1}h", "{0}d {1}h", "{0}T {1}h");
            AddTranslation("time_hours_minutes",
                "{0}h {1}m", "{0}h {1}m", "{0}h {1}m", "{0}h {1}m", "{0}h {1}m");
            AddTranslation("time_minutes_seconds",
                "{0}m {1}s", "{0}m {1}s", "{0}m {1}s", "{0}m {1}s", "{0}m {1}s");

            // ==================== LEADERBOARD DISPLAY ====================
            AddTranslation("leaderboard_header",
                "LEADERBOARD", "CLASIFICACIÓN", "CLASSEMENT", "CLASSIFICAÇÃO", "RANGLISTE");
        }

        private void AddTranslation(string key, string english, string spanish, string french, string portuguese, string german)
        {
            textDictionary[key] = new LocalizedText
            {
                key = key,
                english = english,
                spanish = spanish,
                french = french,
                portuguese = portuguese,
                german = german
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
                case Language.French: return text.french;
                case Language.Portuguese: return text.portuguese;
                case Language.German: return text.german;
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
