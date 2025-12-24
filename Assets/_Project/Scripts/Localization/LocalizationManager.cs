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
                    if (!System.IO.File.Exists(path))
                    {
                        path = "Assets/_Project/Localization/Translations.txt";
                    }

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
        ///     FR: French text
        ///     PT: Portuguese text
        ///     DE: German text
        /// </summary>
        private void ParseTranslationsFile(string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            string[] lines = content.Split('\n');
            string currentKey = null;
            string en = "", es = "", fr = "", pt = "", de = "";

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');

                // Saltar líneas vacías y comentarios
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("=") || line.StartsWith("#"))
                {
                    // Si teníamos una key pendiente, guardarla
                    if (!string.IsNullOrEmpty(currentKey) && !string.IsNullOrEmpty(en))
                    {
                        SaveParsedTranslation(currentKey, en, es, fr, pt, de);
                        currentKey = null;
                        en = es = fr = pt = de = "";
                    }
                    continue;
                }

                string trimmed = line.Trim();

                // Detectar líneas de idioma
                if (trimmed.StartsWith("EN:"))
                {
                    en = trimmed.Substring(3).Trim();
                }
                else if (trimmed.StartsWith("ES:"))
                {
                    es = trimmed.Substring(3).Trim();
                }
                else if (trimmed.StartsWith("FR:"))
                {
                    fr = trimmed.Substring(3).Trim();
                }
                else if (trimmed.StartsWith("PT:"))
                {
                    pt = trimmed.Substring(3).Trim();
                }
                else if (trimmed.StartsWith("DE:"))
                {
                    de = trimmed.Substring(3).Trim();
                }
                // Si no empieza con espacio/tab y no es línea de idioma, es una nueva key
                else if (!line.StartsWith(" ") && !line.StartsWith("\t") && !trimmed.Contains(":") && !trimmed.Contains("|"))
                {
                    // Guardar key anterior si existe
                    if (!string.IsNullOrEmpty(currentKey) && !string.IsNullOrEmpty(en))
                    {
                        SaveParsedTranslation(currentKey, en, es, fr, pt, de);
                    }

                    // Nueva key
                    currentKey = trimmed;
                    en = es = fr = pt = de = "";
                }
            }

            // Guardar última key
            if (!string.IsNullOrEmpty(currentKey) && !string.IsNullOrEmpty(en))
            {
                SaveParsedTranslation(currentKey, en, es, fr, pt, de);
            }

            Debug.Log($"[Localization] {textDictionary.Count} traducciones cargadas desde archivo");
        }

        private void SaveParsedTranslation(string key, string en, string es, string fr, string pt, string de)
        {
            if (textDictionary.ContainsKey(key)) return; // No sobrescribir

            textDictionary[key] = new LocalizedText
            {
                key = key,
                english = !string.IsNullOrEmpty(en) ? en : key,
                spanish = !string.IsNullOrEmpty(es) ? es : en,
                french = !string.IsNullOrEmpty(fr) ? fr : en,
                portuguese = !string.IsNullOrEmpty(pt) ? pt : en,
                german = !string.IsNullOrEmpty(de) ? de : en
            };
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
            AddTranslation("change_language",
                "Change Language", "Cambiar Idioma", "Changer de langue", "Mudar Idioma", "Sprache ändern");
            AddTranslation("back_button",
                "Back", "Volver", "Retour", "Voltar", "Zurück");

            // ==================== PREMIUM / PURCHASES ====================
            AddTranslation("premium_title",
                "PREMIUM", "PREMIUM", "PREMIUM", "PREMIUM", "PREMIUM");
            AddTranslation("premium_section_title",
                "Premium", "Premium", "Premium", "Premium", "Premium");
            AddTranslation("no_ads_title",
                "NO ADS", "SIN ANUNCIOS", "SANS PUBS", "SEM ANUNCIOS", "OHNE WERBUNG");
            AddTranslation("no_ads_description",
                "Play without interruptions", "Juega sin interrupciones", "Jouez sans interruptions", "Jogue sem interrupcoes", "Spiele ohne Unterbrechungen");
            AddTranslation("no_ads_price",
                "$9.99", "$9.99", "9,99€", "R$9,99", "9,99€");
            AddTranslation("remove_ads_title",
                "Remove Ads", "Quitar Anuncios", "Supprimer les pubs", "Remover Anúncios", "Werbung entfernen");
            AddTranslation("remove_ads_description",
                "Remove all ads from the app", "Elimina todos los anuncios de la app", "Supprimez toutes les publicités", "Remove todos os anúncios do app", "Entferne alle Werbung");
            AddTranslation("premium_full_title",
                "PREMIUM", "PREMIUM", "PREMIUM", "PREMIUM", "PREMIUM");
            AddTranslation("premium_full_description",
                "No ads + Create tournaments", "Sin anuncios + Crear torneos", "Sans pubs + Créer des tournois", "Sem anúncios + Criar torneios", "Keine Werbung + Turniere erstellen");
            AddTranslation("premium_full_price",
                "$19.99", "$19.99", "19,99€", "R$19,99", "19,99€");
            AddTranslation("buy_button",
                "BUY", "COMPRAR", "ACHETER", "COMPRAR", "KAUFEN");
            AddTranslation("premium_recommended",
                "RECOMMENDED", "RECOMENDADO", "RECOMMANDE", "RECOMENDADO", "EMPFOHLEN");
            AddTranslation("premium_feature_no_ads",
                "No advertisements", "Sin anuncios", "Sans publicites", "Sem anuncios", "Keine Werbung");
            AddTranslation("premium_feature_tournaments",
                "Create unlimited tournaments", "Crear torneos ilimitados", "Creer des tournois illimites", "Criar torneios ilimitados", "Unbegrenzt Turniere erstellen");
            AddTranslation("premium_feature_badge",
                "Exclusive badge", "Insignia exclusiva", "Badge exclusif", "Emblema exclusivo", "Exklusives Abzeichen");
            AddTranslation("premium_active",
                "Premium Active", "Premium Activo", "Premium Actif", "Premium Ativo", "Premium Aktiv");
            AddTranslation("you_are_premium",
                "You are a Premium member!", "Eres miembro Premium!", "Vous etes membre Premium!", "Voce e membro Premium!", "Du bist Premium-Mitglied!");
            AddTranslation("no_ads_active",
                "No Ads Active", "Sin Anuncios Activo", "Sans Pubs Actif", "Sem Anuncios Ativo", "Ohne Werbung Aktiv");
            AddTranslation("already_purchased",
                "Already purchased", "Ya comprado", "Déjà acheté", "Já comprado", "Bereits gekauft");
            AddTranslation("acquired_text",
                "Acquired", "Adquirido", "Acquis", "Adquirido", "Erworben");
            AddTranslation("restore_purchases",
                "Restore Purchases", "Restaurar Compras", "Restaurer les achats", "Restaurar Compras", "Käufe wiederherstellen");
            AddTranslation("purchase_success",
                "Purchase successful!", "¡Compra exitosa!", "Achat réussi!", "Compra realizada!", "Kauf erfolgreich!");
            AddTranslation("purchase_failed",
                "Purchase failed. Try again.", "Error en la compra. Intenta de nuevo.", "Échec de l'achat. Réessayez.", "Falha na compra. Tente novamente.", "Kauf fehlgeschlagen. Versuche es erneut.");
            AddTranslation("purchase_cancelled",
                "Purchase cancelled", "Compra cancelada", "Achat annulé", "Compra cancelada", "Kauf abgebrochen");
            AddTranslation("processing_purchase",
                "Processing purchase...", "Procesando compra...", "Traitement de l'achat...", "Processando compra...", "Kauf wird verarbeitet...");

            // ==================== PREMIUM REQUIRED PANEL ====================
            AddTranslation("premium_required_title",
                "Premium Required", "Se Requiere Premium", "Premium Requis", "Premium Necessário", "Premium Erforderlich");
            AddTranslation("premium_required_message",
                "You need Premium to create tournaments.\nGet Premium Full to unlock this feature!",
                "Necesitas Premium para crear torneos.\n¡Obtén Premium Completo para desbloquear esta función!",
                "Vous avez besoin de Premium pour créer des tournois.\nObtenez Premium Complet!",
                "Você precisa de Premium para criar torneios.\nAdquira Premium Completo!",
                "Du brauchst Premium um Turniere zu erstellen.\nHole dir Premium Vollversion!");
            AddTranslation("get_premium",
                "Get Premium", "Obtener Premium", "Obtenir Premium", "Obter Premium", "Premium holen");
            AddTranslation("maybe_later",
                "Maybe Later", "Quizás Después", "Peut-être plus tard", "Talvez Depois", "Vielleicht später");

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

            // ==================== SUCCESS MESSAGES - Level 1 (PERFECT < 1s) SUPER DOPAMINE ====================
            AddTranslation("msg_godlike_focus",
                "GODLIKE FOCUS! 👀✨", "¡ENFOQUE DIVINO! 👀✨", "CONCENTRATION DIVINE! 👀✨", "FOCO DIVINO! 👀✨", "GÖTTLICHER FOKUS! 👀✨");
            AddTranslation("msg_mind_on_fire",
                "YOUR MIND IS ON FIRE! 🔥🧠", "¡TU MENTE ESTÁ EN LLAMAS! 🔥🧠", "TON ESPRIT EST EN FEU! 🔥🧠", "SUA MENTE ESTÁ EM CHAMAS! 🔥🧠", "DEIN VERSTAND BRENNT! 🔥🧠");
            AddTranslation("msg_exceptional_reflexes",
                "EXCEPTIONAL REFLEXES! 🚀⚡", "¡REFLEJOS EXCEPCIONALES! 🚀⚡", "RÉFLEXES EXCEPTIONNELS! 🚀⚡", "REFLEXOS EXCEPCIONAIS! 🚀⚡", "AUßERGEWÖHNLICHE REFLEXE! 🚀⚡");
            AddTranslation("msg_neural_perfection",
                "NEURAL PERFECTION! 🧠💫", "¡PERFECCIÓN NEURONAL! 🧠💫", "PERFECTION NEURONALE! 🧠💫", "PERFEIÇÃO NEURAL! 🧠💫", "NEURONALE PERFEKTION! 🧠💫");
            AddTranslation("msg_time_master",
                "MASTER OF TIME! ⏱️👑", "¡MAESTRO DEL TIEMPO! ⏱️👑", "MAÎTRE DU TEMPS! ⏱️👑", "MESTRE DO TEMPO! ⏱️👑", "MEISTER DER ZEIT! ⏱️👑");
            AddTranslation("msg_superhuman",
                "SUPERHUMAN SPEED! 💨🦸", "¡VELOCIDAD SOBREHUMANA! 💨🦸", "VITESSE SURHUMAINE! 💨🦸", "VELOCIDADE SOBRE-HUMANA! 💨🦸", "ÜBERMENSCHLICHE GESCHWINDIGKEIT! 💨🦸");
            AddTranslation("msg_unstoppable_force",
                "UNSTOPPABLE FORCE! 💪🔥", "¡FUERZA IMPARABLE! 💪🔥", "FORCE IMPARABLE! 💪🔥", "FORÇA IMPARÁVEL! 💪🔥", "UNAUFHALTSAME KRAFT! 💪🔥");
            AddTranslation("msg_legendary_speed",
                "LEGENDARY SPEED! 🏆⚡", "¡VELOCIDAD LEGENDARIA! 🏆⚡", "VITESSE LÉGENDAIRE! 🏆⚡", "VELOCIDADE LENDÁRIA! 🏆⚡", "LEGENDÄRE GESCHWINDIGKEIT! 🏆⚡");
            AddTranslation("msg_pure_genius",
                "PURE GENIUS! 🧠✨", "¡PURO GENIO! 🧠✨", "PUR GÉNIE! 🧠✨", "PURO GÊNIO! 🧠✨", "REINES GENIE! 🧠✨");
            AddTranslation("msg_absolute_legend",
                "ABSOLUTE LEGEND! 👑🌟", "¡LEYENDA ABSOLUTA! 👑🌟", "LÉGENDE ABSOLUE! 👑🌟", "LENDA ABSOLUTA! 👑🌟", "ABSOLUTE LEGENDE! 👑🌟");

            // ==================== SUCCESS MESSAGES - Level 2 (VERY GOOD 1-2s) HIGH DOPAMINE ====================
            AddTranslation("msg_incredible_focus",
                "Incredible focus! 👀🔥", "¡Enfoque increíble! 👀🔥", "Concentration incroyable! 👀🔥", "Foco incrível! 👀🔥", "Unglaublicher Fokus! 👀🔥");
            AddTranslation("msg_blazing_fast",
                "Blazing fast! ⚡🔥", "¡Velocidad ardiente! ⚡🔥", "Ultra rapide! ⚡🔥", "Velocidade ardente! ⚡🔥", "Blitzschnell! ⚡🔥");
            AddTranslation("msg_sharp_mind",
                "Sharp mind! 🧠⚡", "¡Mente aguda! 🧠⚡", "Esprit vif! 🧠⚡", "Mente afiada! 🧠⚡", "Scharfer Verstand! 🧠⚡");
            AddTranslation("msg_impressive_reflexes",
                "Impressive reflexes! 🎯✨", "¡Reflejos impresionantes! 🎯✨", "Réflexes impressionnants! 🎯✨", "Reflexos impressionantes! 🎯✨", "Beeindruckende Reflexe! 🎯✨");
            AddTranslation("msg_excellent_timing",
                "Excellent timing! ⏱️🌟", "¡Tiempo excelente! ⏱️🌟", "Timing excellent! ⏱️🌟", "Tempo excelente! ⏱️🌟", "Exzellentes Timing! ⏱️🌟");
            AddTranslation("msg_on_fire",
                "You're on fire! 🔥💪", "¡Estás en llamas! 🔥💪", "Tu es en feu! 🔥💪", "Você está pegando fogo! 🔥💪", "Du bist on fire! 🔥💪");
            AddTranslation("msg_amazing_speed",
                "Amazing speed! 🚀✨", "¡Velocidad asombrosa! 🚀✨", "Vitesse incroyable! 🚀✨", "Velocidade incrível! 🚀✨", "Erstaunliche Geschwindigkeit! 🚀✨");
            AddTranslation("msg_brilliant_play",
                "Brilliant play! 💫🎮", "¡Jugada brillante! 💫🎮", "Jeu brillant! 💫🎮", "Jogada brilhante! 💫🎮", "Brillantes Spiel! 💫🎮");
            AddTranslation("msg_stellar_performance",
                "Stellar performance! ⭐🏆", "¡Rendimiento estelar! ⭐🏆", "Performance stellaire! ⭐🏆", "Desempenho estelar! ⭐🏆", "Herausragende Leistung! ⭐🏆");
            AddTranslation("msg_remarkable",
                "Remarkable! 🌟👏", "¡Notable! 🌟👏", "Remarquable! 🌟👏", "Notável! 🌟👏", "Bemerkenswert! 🌟👏");

            // ==================== SUCCESS MESSAGES - Level 3 (GOOD 2-3s) POSITIVE ====================
            AddTranslation("msg_great_job",
                "Great job! 👍", "¡Gran trabajo! 👍", "Super boulot! 👍", "Ótimo trabalho! 👍", "Tolle Arbeit! 👍");
            AddTranslation("msg_well_played",
                "Well played! 🎮", "¡Bien jugado! 🎮", "Bien joué! 🎮", "Bem jogado! 🎮", "Gut gespielt! 🎮");
            AddTranslation("msg_nice_speed",
                "Nice speed! ⚡", "¡Buena velocidad! ⚡", "Belle vitesse! ⚡", "Boa velocidade! ⚡", "Gute Geschwindigkeit! ⚡");
            AddTranslation("msg_good_reflexes",
                "Good reflexes! 👀", "¡Buenos reflejos! 👀", "Bons réflexes! 👀", "Bons reflexos! 👀", "Gute Reflexe! 👀");
            AddTranslation("msg_solid_time",
                "Solid time! ⏱️", "¡Tiempo sólido! ⏱️", "Bon temps! ⏱️", "Tempo sólido! ⏱️", "Solide Zeit! ⏱️");

            // ==================== SUCCESS MESSAGES - Level 4 (DECENT 3-4s) ENCOURAGING ====================
            AddTranslation("msg_good_effort",
                "Good effort!", "¡Buen esfuerzo!", "Bon effort!", "Bom esforço!", "Gute Mühe!");
            AddTranslation("msg_not_bad",
                "Not bad!", "¡Nada mal!", "Pas mal!", "Nada mal!", "Nicht schlecht!");
            AddTranslation("msg_keep_going",
                "Keep going!", "¡Sigue adelante!", "Continue!", "Continue!", "Weiter so!");
            AddTranslation("msg_nice_try",
                "Nice try!", "¡Buen intento!", "Bel essai!", "Boa tentativa!", "Guter Versuch!");
            AddTranslation("msg_getting_better",
                "Getting better!", "¡Mejorando!", "En progression!", "Melhorando!", "Wird besser!");

            // ==================== SUCCESS MESSAGES - Level 5 (BASIC 4-5s) MOTIVATIONAL ====================
            AddTranslation("msg_completed",
                "Completed!", "¡Completado!", "Terminé!", "Completo!", "Fertig!");
            AddTranslation("msg_done",
                "Done!", "¡Hecho!", "Fait!", "Feito!", "Erledigt!");
            AddTranslation("msg_finished",
                "Finished!", "¡Terminado!", "Fini!", "Finalizado!", "Beendet!");
            AddTranslation("msg_keep_practicing",
                "Keep practicing!", "¡Sigue practicando!", "Continue à pratiquer!", "Continue praticando!", "Weiter üben!");
            AddTranslation("msg_you_can_improve",
                "You can do better!", "¡Puedes hacerlo mejor!", "Tu peux faire mieux!", "Você pode melhorar!", "Du kannst es besser!");

            // ==================== SUCCESS MESSAGES - Level 6 (NO CLASSIFY 5s+) EMOTIONAL SUPPORT ====================
            AddTranslation("msg_almost_there",
                "Almost there... keep trying! 💙", "Casi lo logras... ¡sigue intentando! 💙", "Presque... continue d'essayer! 💙", "Quase lá... continue tentando! 💙", "Fast geschafft... bleib dran! 💙");
            AddTranslation("msg_breathe_continue",
                "Breathe and continue 💙", "Respira y sigue 💙", "Respire et continue 💙", "Respire e continue 💙", "Atme und mach weiter 💙");
            AddTranslation("msg_next_will_be_better",
                "Next one will be better! 🌟", "¡El siguiente será mejor! 🌟", "Le prochain sera meilleur! 🌟", "O próximo será melhor! 🌟", "Der nächste wird besser! 🌟");
            AddTranslation("msg_dont_give_up",
                "Don't give up! 💪", "¡No te rindas! 💪", "N'abandonne pas! 💪", "Não desista! 💪", "Gib nicht auf! 💪");
            AddTranslation("msg_patience_wins",
                "Patience wins 🧘", "La paciencia gana 🧘", "La patience gagne 🧘", "A paciência vence 🧘", "Geduld gewinnt 🧘");
            AddTranslation("msg_every_try_counts",
                "Every try counts! 📈", "¡Cada intento cuenta! 📈", "Chaque essai compte! 📈", "Cada tentativa conta! 📈", "Jeder Versuch zählt! 📈");
            AddTranslation("msg_progress_not_perfection",
                "Progress, not perfection 🌱", "Progreso, no perfección 🌱", "Progrès, pas perfection 🌱", "Progresso, não perfeição 🌱", "Fortschritt, nicht Perfektion 🌱");
            AddTranslation("msg_keep_calm",
                "Stay calm and try again 🌊", "Mantén la calma e intenta de nuevo 🌊", "Reste calme et réessaie 🌊", "Fique calmo e tente novamente 🌊", "Bleib ruhig und versuch es nochmal 🌊");
            AddTranslation("msg_believe_yourself",
                "Believe in yourself! ✨", "¡Cree en ti mismo! ✨", "Crois en toi! ✨", "Acredite em você! ✨", "Glaub an dich! ✨");
            AddTranslation("msg_stay_focused",
                "Stay focused, you got this! 🎯", "¡Concéntrate, tú puedes! 🎯", "Reste concentré, tu peux le faire! 🎯", "Fique focado, você consegue! 🎯", "Bleib fokussiert, du schaffst das! 🎯");

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
            AddTranslation("skillz_cash_tab",
                "Cash", "Dinero", "Argent", "Dinheiro", "Geld");
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
                "Leave Tournament", "Abandonar Torneo", "Quitter le Tournoi", "Abandonar Torneio", "Turnier Verlassen");
            AddTranslation("exit_confirm_message",
                "Are you sure you want to leave? Your progress in this tournament will be lost.", "¿Seguro que quieres abandonar? Tu progreso en este torneo se perderá.", "Êtes-vous sûr de vouloir quitter? Votre progression sera perdue.", "Tem certeza que quer sair? Seu progresso será perdido.", "Bist du sicher? Dein Fortschritt geht verloren.");
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

            // ==================== LOGOUT CONFIRM ====================
            AddTranslation("logout_confirm_title",
                "Logout?", "¿Cerrar Sesión?", "Déconnexion?", "Sair?", "Abmelden?");
            AddTranslation("logout_confirm_message",
                "Are you sure you want to logout?", "¿Estás seguro de que quieres cerrar sesión?", "Êtes-vous sûr de vouloir vous déconnecter?", "Tem certeza de que deseja sair?", "Bist du sicher, dass du dich abmelden möchtest?");

            // ==================== SCORES TABS ====================
            AddTranslation("personal_tab",
                "Personal", "Personal", "Personnel", "Pessoal", "Persönlich");
            AddTranslation("personal_best_time",
                "Personal Best Time", "Mejor Tiempo Personal", "Meilleur Temps Personnel", "Melhor Tempo Pessoal", "Persönliche Bestzeit");

            // ==================== SEARCH OPTIONS ====================
            AddTranslation("search_options_title",
                "Search Options", "Opciones de Búsqueda", "Options de recherche", "Opções de Busca", "Suchoptionen");

            // ==================== CREATE TOURNAMENT ====================
            AddTranslation("create_tournament_title",
                "Create Tournament", "Crear Torneo", "Créer un tournoi", "Criar Torneio", "Turnier erstellen");

            // ==================== BUTTONS ====================
            AddTranslation("later_button",
                "Later", "Más tarde", "Plus tard", "Mais tarde", "Später");

            // ==================== ERROR MESSAGES - USERNAME ====================
            AddTranslation("error_username_empty",
                "You need a player name!", "¡Necesitas un nombre de jugador!", "Vous avez besoin d'un nom de joueur!", "Você precisa de um nome de jogador!", "Du brauchst einen Spielernamen!");
            AddTranslation("error_username_too_short",
                "Name is too short (minimum 3 characters)", "El nombre es muy corto (mínimo 3 caracteres)", "Le nom est trop court (minimum 3 caractères)", "O nome é muito curto (mínimo 3 caracteres)", "Der Name ist zu kurz (mindestens 3 Zeichen)");
            AddTranslation("error_username_too_long",
                "Name is too long (maximum 20 characters)", "El nombre es muy largo (máximo 20 caracteres)", "Le nom est trop long (maximum 20 caractères)", "O nome é muito longo (máximo 20 caracteres)", "Der Name ist zu lang (maximal 20 Zeichen)");
            AddTranslation("error_username_invalid_chars",
                "Only letters, numbers and underscores", "Solo letras, números y guiones bajos", "Uniquement lettres, chiffres et underscores", "Apenas letras, números e underscores", "Nur Buchstaben, Zahlen und Unterstriche");
            AddTranslation("error_username_taken",
                "That name is already taken, try another", "Ese nombre ya está tomado, prueba otro", "Ce nom est déjà pris, essayez un autre", "Esse nome já está em uso, tente outro", "Dieser Name ist bereits vergeben, versuche einen anderen");

            // ==================== ERROR MESSAGES - EMAIL ====================
            AddTranslation("error_email_empty",
                "Enter your email address", "Ingresa tu correo electrónico", "Entrez votre adresse e-mail", "Digite seu endereço de e-mail", "Gib deine E-Mail-Adresse ein");
            AddTranslation("error_email_invalid",
                "Hmm... that email doesn't look valid", "Hmm... ese correo no parece válido", "Hmm... cet e-mail ne semble pas valide", "Hmm... esse e-mail não parece válido", "Hmm... diese E-Mail scheint ungültig zu sein");
            AddTranslation("error_email_already_registered",
                "This email already has an account", "Este correo ya tiene una cuenta", "Cet e-mail a déjà un compte", "Este e-mail já possui uma conta", "Diese E-Mail hat bereits ein Konto");

            // ==================== ERROR MESSAGES - PASSWORD ====================
            AddTranslation("error_password_empty",
                "Create a password", "Crea una contraseña", "Créez un mot de passe", "Crie uma senha", "Erstelle ein Passwort");
            AddTranslation("error_password_too_short",
                "Password is too short (minimum 6 characters)", "La contraseña es muy corta (mínimo 6 caracteres)", "Le mot de passe est trop court (minimum 6 caractères)", "A senha é muito curta (mínimo 6 caracteres)", "Das Passwort ist zu kurz (mindestens 6 Zeichen)");
            AddTranslation("error_password_weak",
                "Add numbers or symbols for more security", "Agrega números o símbolos para mayor seguridad", "Ajoutez des chiffres ou symboles pour plus de sécurité", "Adicione números ou símbolos para mais segurança", "Füge Zahlen oder Symbole für mehr Sicherheit hinzu");

            // ==================== ERROR MESSAGES - CONFIRM PASSWORD ====================
            AddTranslation("error_confirm_password_empty",
                "Confirm your password", "Confirma tu contraseña", "Confirmez votre mot de passe", "Confirme sua senha", "Bestätige dein Passwort");
            AddTranslation("error_passwords_not_match",
                "Passwords don't match", "Las contraseñas no coinciden", "Les mots de passe ne correspondent pas", "As senhas não coincidem", "Die Passwörter stimmen nicht überein");

            // ==================== ERROR MESSAGES - GENERAL / NETWORK ====================
            AddTranslation("error_no_connection",
                "No internet connection. Check your network", "Sin conexión a internet. Revisa tu red", "Pas de connexion internet. Vérifiez votre réseau", "Sem conexão com a internet. Verifique sua rede", "Keine Internetverbindung. Überprüfe dein Netzwerk");
            AddTranslation("error_server",
                "Something went wrong. Try again", "Algo salió mal. Intenta de nuevo", "Quelque chose s'est mal passé. Réessayez", "Algo deu errado. Tente novamente", "Etwas ist schief gelaufen. Versuche es erneut");
            AddTranslation("error_timeout",
                "Server took too long. Try again", "El servidor tardó mucho. Intenta de nuevo", "Le serveur a pris trop de temps. Réessayez", "O servidor demorou muito. Tente novamente", "Der Server hat zu lange gebraucht. Versuche es erneut");

            // ==================== ERROR MESSAGES - LOGIN SPECIFIC ====================
            AddTranslation("error_user_not_found",
                "User not found", "Usuario no encontrado", "Utilisateur non trouvé", "Usuário não encontrado", "Benutzer nicht gefunden");
            AddTranslation("error_wrong_password",
                "Incorrect password", "Contraseña incorrecta", "Mot de passe incorrect", "Senha incorreta", "Falsches Passwort");
            AddTranslation("error_auth_generic",
                "Authentication error. Try again", "Error de autenticación. Intenta nuevamente", "Erreur d'authentification. Réessayez", "Erro de autenticação. Tente novamente", "Authentifizierungsfehler. Versuche es erneut");

            // ==================== ERROR MESSAGES - REGISTER SPECIFIC ====================
            AddTranslation("error_create_account",
                "Could not create account. Try again", "No se pudo crear la cuenta. Intenta de nuevo", "Impossible de créer le compte. Réessayez", "Não foi possível criar a conta. Tente novamente", "Konto konnte nicht erstellt werden. Versuche es erneut");
            AddTranslation("error_save_username",
                "Error saving username", "Error al guardar el nombre de usuario", "Erreur lors de l'enregistrement du nom", "Erro ao salvar o nome de usuário", "Fehler beim Speichern des Benutzernamens");

            // ==================== ERROR PANEL UI ====================
            AddTranslation("ErrorText",
                "Error", "Error", "Erreur", "Erro", "Fehler");
            AddTranslation("ErrorButtonText",
                "Accept", "Aceptar", "Accepter", "Aceitar", "Akzeptieren");

            // ==================== TOURNAMENT FILTERS ====================
            AddTranslation("min_time",
                "Min Time", "Tiempo Mín", "Temps min", "Tempo Mín", "Min. Zeit");
            AddTranslation("max_time",
                "Max Time", "Tiempo Máx", "Temps max", "Tempo Máx", "Max. Zeit");
            AddTranslation("min_players",
                "Min Players", "Mín. Jugadores", "Joueurs min", "Mín. Jogadores", "Min. Spieler");
            AddTranslation("type",
                "Type", "Tipo", "Type", "Tipo", "Typ");

            // ==================== REGISTER SCREEN ====================
            AddTranslation("register_title",
                "Create an account", "Crea una cuenta", "Créer un compte", "Criar uma conta", "Konto erstellen");
            AddTranslation("username_input_placeholder",
                "Username", "Nombre de Usuario", "Nom d'utilisateur", "Nome de Usuário", "Benutzername");
            AddTranslation("confirm_password_placeholder",
                "Confirm Password", "Confirmar Contraseña", "Confirmer le mot de passe", "Confirmar Senha", "Passwort bestätigen");
            AddTranslation("create_account_button",
                "Create Account", "Crear Cuenta", "Créer un compte", "Criar Conta", "Konto erstellen");

            // ==================== BOOT ERRORS ====================
            AddTranslation("boot_error_firebase",
                "Could not connect to services", "No se pudo conectar a los servicios", "Impossible de se connecter aux services", "Não foi possível conectar aos serviços", "Verbindung zu Diensten fehlgeschlagen");
            AddTranslation("boot_error_no_internet",
                "Internet connection required", "Se requiere conexión a internet", "Connexion internet requise", "Conexão com internet necessária", "Internetverbindung erforderlich");
            AddTranslation("boot_error_timeout",
                "Connection timed out. Check your internet", "Conexión agotada. Revisa tu internet", "Connexion expirée. Vérifiez votre internet", "Conexão expirou. Verifique sua internet", "Verbindung abgelaufen. Prüfe dein Internet");
            AddTranslation("boot_retry_button",
                "Retry", "Reintentar", "Réessayer", "Tentar novamente", "Erneut versuchen");
            AddTranslation("boot_exit_button",
                "Exit", "Salir", "Quitter", "Sair", "Beenden");

            // ==================== ERROR MESSAGES - ADDITIONAL ====================
            AddTranslation("error_session_expired",
                "Your session expired. Please login again", "Tu sesión expiró. Inicia sesión de nuevo", "Votre session a expiré. Reconnectez-vous", "Sua sessão expirou. Faça login novamente", "Deine Sitzung ist abgelaufen. Melde dich erneut an");
            AddTranslation("error_account_disabled",
                "Your account has been suspended", "Tu cuenta ha sido suspendida", "Votre compte a été suspendu", "Sua conta foi suspensa", "Dein Konto wurde gesperrt");
            AddTranslation("error_account_not_found",
                "No account exists with that email", "No existe una cuenta con ese correo", "Aucun compte n'existe avec cet e-mail", "Não existe conta com esse e-mail", "Kein Konto mit dieser E-Mail vorhanden");
            AddTranslation("error_wrong_credentials",
                "Incorrect email or password", "Email o contraseña incorrectos", "E-mail ou mot de passe incorrect", "E-mail ou senha incorretos", "E-Mail oder Passwort falsch");
            AddTranslation("error_too_many_attempts",
                "Too many attempts. Wait a few minutes", "Demasiados intentos. Espera unos minutos", "Trop de tentatives. Attendez quelques minutes", "Muitas tentativas. Aguarde alguns minutos", "Zu viele Versuche. Warte ein paar Minuten");
            AddTranslation("error_google_auth",
                "Error signing in with Google", "Error al iniciar con Google", "Erreur de connexion avec Google", "Erro ao entrar com Google", "Fehler bei der Anmeldung mit Google");
            AddTranslation("error_google_auth_cancelled",
                "Google sign in cancelled", "Inicio con Google cancelado", "Connexion Google annulée", "Login com Google cancelado", "Google-Anmeldung abgebrochen");
            AddTranslation("error_apple_auth",
                "Error signing in with Apple", "Error al iniciar con Apple", "Erreur de connexion avec Apple", "Erro ao entrar com Apple", "Fehler bei der Anmeldung mit Apple");
            AddTranslation("error_apple_auth_cancelled",
                "Apple sign in cancelled", "Inicio con Apple cancelado", "Connexion Apple annulée", "Login com Apple cancelado", "Apple-Anmeldung abgebrochen");
            AddTranslation("sign_in_apple",
                "Sign in with Apple", "Iniciar con Apple", "Se connecter avec Apple", "Entrar com Apple", "Mit Apple anmelden");
            AddTranslation("sign_in_google",
                "Sign in with Google", "Iniciar con Google", "Se connecter avec Google", "Entrar com Google", "Mit Google anmelden");
            AddTranslation("error_register_email_empty",
                "Enter your email address", "Ingresa tu correo electrónico", "Entrez votre adresse e-mail", "Digite seu e-mail", "Gib deine E-Mail-Adresse ein");
            AddTranslation("error_register_email_invalid",
                "That email doesn't look valid", "Ese correo no parece válido", "Cet e-mail ne semble pas valide", "Esse e-mail não parece válido", "Diese E-Mail scheint ungültig zu sein");
            AddTranslation("error_register_password_empty",
                "Create a password", "Crea una contraseña", "Créez un mot de passe", "Crie uma senha", "Erstelle ein Passwort");

            // ==================== ERROR MESSAGES - PROFILE/SETTINGS ====================
            AddTranslation("error_loading_profile",
                "Error loading your profile", "Error al cargar tu perfil", "Erreur lors du chargement du profil", "Erro ao carregar seu perfil", "Fehler beim Laden deines Profils");
            AddTranslation("error_name_empty",
                "Name cannot be empty", "El nombre no puede estar vacío", "Le nom ne peut pas être vide", "O nome não pode estar vazio", "Der Name darf nicht leer sein");
            AddTranslation("error_name_taken",
                "That name is already taken", "Ese nombre ya está tomado", "Ce nom est déjà pris", "Esse nome já está em uso", "Dieser Name ist bereits vergeben");
            AddTranslation("error_changing_name",
                "Error changing name", "Error al cambiar nombre", "Erreur lors du changement de nom", "Erro ao mudar nome", "Fehler beim Ändern des Namens");
            AddTranslation("error_logout",
                "Error logging out", "Error al cerrar sesión", "Erreur lors de la déconnexion", "Erro ao sair", "Fehler beim Abmelden");
            AddTranslation("error_deleting_account",
                "Error deleting account", "Error al eliminar cuenta", "Erreur lors de la suppression du compte", "Erro ao excluir conta", "Fehler beim Löschen des Kontos");
            AddTranslation("confirm_delete_account",
                "Delete your account? This action cannot be undone", "¿Eliminar tu cuenta? Esta acción no se puede deshacer", "Supprimer votre compte? Cette action est irréversible", "Excluir sua conta? Esta ação não pode ser desfeita", "Konto löschen? Diese Aktion kann nicht rückgängig gemacht werden");

            // ==================== ERROR MESSAGES - SCORES ====================
            AddTranslation("error_saving_score",
                "Error saving your score", "Error al guardar tu puntuación", "Erreur lors de l'enregistrement du score", "Erro ao salvar sua pontuação", "Fehler beim Speichern deiner Punktzahl");
            AddTranslation("error_loading_scores",
                "Error loading scores", "Error al cargar puntuaciones", "Erreur lors du chargement des scores", "Erro ao carregar pontuações", "Fehler beim Laden der Punktzahlen");
            AddTranslation("error_scores_need_connection",
                "Connect to internet to see rankings", "Conecta a internet para ver rankings", "Connectez-vous à internet pour voir le classement", "Conecte-se à internet para ver rankings", "Verbinde dich mit dem Internet für Rankings");
            AddTranslation("error_no_personal_scores",
                "You don't have any scores yet", "Aún no tienes puntuaciones", "Vous n'avez pas encore de scores", "Você ainda não tem pontuações", "Du hast noch keine Punktzahlen");
            AddTranslation("error_no_global_scores",
                "No scores yet", "No hay puntuaciones todavía", "Pas encore de scores", "Ainda não há pontuações", "Noch keine Punktzahlen");

            // ==================== ERROR MESSAGES - TOURNAMENTS ====================
            AddTranslation("error_tournaments_need_connection",
                "Tournaments require internet connection", "Los torneos requieren conexión a internet", "Les tournois nécessitent une connexion internet", "Torneios requerem conexão com a internet", "Turniere benötigen Internetverbindung");
            AddTranslation("error_loading_tournaments",
                "Error loading tournaments", "Error al cargar torneos", "Erreur lors du chargement des tournois", "Erro ao carregar torneios", "Fehler beim Laden der Turniere");
            AddTranslation("error_tournament_not_found",
                "Tournament not found", "Torneo no encontrado", "Tournoi non trouvé", "Torneio não encontrado", "Turnier nicht gefunden");
            AddTranslation("error_invalid_code",
                "Invalid tournament code", "Código de torneo inválido", "Code de tournoi invalide", "Código de torneio inválido", "Ungültiger Turniercode");
            AddTranslation("error_tournament_full",
                "This tournament is full", "Este torneo está lleno", "Ce tournoi est complet", "Este torneio está cheio", "Dieses Turnier ist voll");
            AddTranslation("error_tournament_expired",
                "This tournament has ended", "Este torneo ya terminó", "Ce tournoi est terminé", "Este torneio já terminou", "Dieses Turnier ist beendet");
            AddTranslation("error_already_in_tournament",
                "You're already in this tournament", "Ya estás participando en este torneo", "Vous participez déjà à ce tournoi", "Você já está neste torneio", "Du bist bereits in diesem Turnier");
            AddTranslation("error_joining_tournament",
                "Error joining tournament", "Error al unirse al torneo", "Erreur pour rejoindre le tournoi", "Erro ao entrar no torneio", "Fehler beim Beitreten zum Turnier");
            AddTranslation("error_leaving_tournament",
                "Error leaving tournament", "Error al salir del torneo", "Erreur pour quitter le tournoi", "Erro ao sair do torneio", "Fehler beim Verlassen des Turniers");
            AddTranslation("error_not_premium",
                "You need Premium to create tournaments", "Necesitas Premium para crear torneos", "Vous avez besoin de Premium pour créer des tournois", "Você precisa de Premium para criar torneios", "Du brauchst Premium um Turniere zu erstellen");
            AddTranslation("error_creating_tournament",
                "Error creating tournament", "Error al crear torneo", "Erreur lors de la création du tournoi", "Erro ao criar torneio", "Fehler beim Erstellen des Turniers");
            AddTranslation("error_tournament_limit",
                "You have the maximum active tournaments", "Ya tienes el máximo de torneos activos", "Vous avez le maximum de tournois actifs", "Você tem o máximo de torneios ativos", "Du hast die maximale Anzahl aktiver Turniere");

            // ==================== ERROR MESSAGES - ADS ====================
            AddTranslation("error_loading_ad",
                "Error loading ad", "Error al cargar anuncio", "Erreur lors du chargement de la pub", "Erro ao carregar anúncio", "Fehler beim Laden der Werbung");
            AddTranslation("error_no_ads_available",
                "Ad not available. Try again later", "Anuncio no disponible. Intenta más tarde", "Pub non disponible. Réessayez plus tard", "Anúncio não disponível. Tente mais tarde", "Werbung nicht verfügbar. Versuche es später");

            // ==================== REGISTER - ADDITIONAL ====================
            AddTranslation("already_have_account",
                "Already have an account?", "¿Ya tienes una cuenta?", "Vous avez déjà un compte?", "Já tem uma conta?", "Bereits ein Konto?");
            AddTranslation("back_to_login",
                "Back to Login", "Volver a Iniciar Sesión", "Retour à la connexion", "Voltar ao Login", "Zurück zum Login");

            // ==================== PREMIUM - ADDITIONAL ====================
            AddTranslation("premium_button",
                "Premium", "Premium", "Premium", "Premium", "Premium");
            AddTranslation("premium_banner",
                "Go Premium!", "¡Hazte Premium!", "Passez Premium!", "Seja Premium!", "Werde Premium!");
            AddTranslation("tired_of_ads",
                "Tired of ads?", "¿Cansado de anuncios?", "Fatigué des pubs?", "Cansado de anúncios?", "Genug von Werbung?");
            AddTranslation("remove_ads_now",
                "Remove ads now", "Quita los anuncios ahora", "Supprimez les pubs maintenant", "Remova os anúncios agora", "Werbung jetzt entfernen");
            AddTranslation("no_thanks",
                "No thanks", "No gracias", "Non merci", "Não obrigado", "Nein danke");
            AddTranslation("premium_unlock_tournaments",
                "Unlock tournament creation!", "¡Desbloquea la creación de torneos!", "Débloquez la création de tournois!", "Desbloqueie a criação de torneios!", "Schalte Turniererstellung frei!");
            AddTranslation("purchase_error",
                "Purchase failed. Try again later", "Error en la compra. Intenta más tarde", "Échec de l'achat. Réessayez plus tard", "Falha na compra. Tente mais tarde", "Kauf fehlgeschlagen. Versuche es später");
            AddTranslation("restore_success",
                "Purchases restored successfully!", "¡Compras restauradas exitosamente!", "Achats restaurés avec succès!", "Compras restauradas com sucesso!", "Käufe erfolgreich wiederhergestellt!");
            AddTranslation("restore_error",
                "Could not restore purchases", "No se pudieron restaurar las compras", "Impossible de restaurer les achats", "Não foi possível restaurar as compras", "Käufe konnten nicht wiederhergestellt werden");
            AddTranslation("restore_nothing",
                "No purchases to restore", "No hay compras para restaurar", "Aucun achat à restaurer", "Nenhuma compra para restaurar", "Keine Käufe zum Wiederherstellen");

            // ==================== SEARCH ====================
            AddTranslation("search_tournament",
                "Search Tournament", "Buscar Torneo", "Rechercher Tournoi", "Buscar Torneio", "Turnier suchen");
            AddTranslation("search_options",
                "Search Options", "Opciones de Búsqueda", "Options de recherche", "Opções de Busca", "Suchoptionen");
            AddTranslation("username_search_placeholder",
                "Search by username...", "Buscar por usuario...", "Rechercher par nom...", "Buscar por usuário...", "Nach Benutzer suchen...");

            // ==================== THEMES ====================
            AddTranslation("theme_selector_title",
                "Select Theme", "Seleccionar Tema", "Sélectionner le thème", "Selecionar Tema", "Thema auswählen");
            AddTranslation("theme_current",
                "Current Theme", "Tema Actual", "Thème actuel", "Tema Atual", "Aktuelles Thema");
            AddTranslation("theme_preview",
                "Preview", "Vista Previa", "Aperçu", "Visualização", "Vorschau");
            AddTranslation("theme_apply",
                "Apply Theme", "Aplicar Tema", "Appliquer le thème", "Aplicar Tema", "Thema anwenden");
            AddTranslation("theme_neon_dark",
                "Neon Dark", "Neón Oscuro", "Néon Sombre", "Neon Escuro", "Neon Dunkel");
            AddTranslation("theme_clean_light",
                "Clean Light", "Luz Limpia", "Lumière Pure", "Luz Limpa", "Sauberes Licht");
            AddTranslation("theme_retro_arcade",
                "Retro Arcade", "Arcade Retro", "Arcade Rétro", "Arcade Retrô", "Retro Arcade");
            AddTranslation("theme_ocean",
                "Ocean", "Océano", "Océan", "Oceano", "Ozean");
            AddTranslation("theme_volcano",
                "Volcano", "Volcán", "Volcan", "Vulcão", "Vulkan");
            AddTranslation("theme_cyberpunk",
                "Cyberpunk", "Cyberpunk", "Cyberpunk", "Cyberpunk", "Cyberpunk");
            AddTranslation("theme_premium_required",
                "Premium theme", "Tema Premium", "Thème Premium", "Tema Premium", "Premium-Thema");
            AddTranslation("change_theme",
                "Change Theme", "Cambiar Tema", "Changer le thème", "Mudar Tema", "Thema ändern");
            AddTranslation("change_style",
                "Change Style", "Cambiar Estilo", "Changer le style", "Mudar Estilo", "Stil ändern");

            // ==================== STYLES PRO PROMPT ====================
            AddTranslation("styles_pro_title",
                "Styles PRO", "Estilos PRO", "Styles PRO", "Estilos PRO", "Styles PRO");
            AddTranslation("styles_pro_unlock_themes",
                "Unlock 5 exclusive themes:", "Desbloquea 5 temas exclusivos:", "Débloquez 5 thèmes exclusifs:", "Desbloqueie 5 temas exclusivos:", "Schalte 5 exklusive Themen frei:");
            AddTranslation("styles_pro_price",
                "$29 MXN", "$29 MXN", "29 MXN", "R$29", "29 MXN");
            AddTranslation("close_button",
                "Close", "Cerrar", "Fermer", "Fechar", "Schließen");
            AddTranslation("cancel",
                "Cancel", "Cancelar", "Annuler", "Cancelar", "Abbrechen");
            AddTranslation("theme_clean_light_desc",
                "Professional", "Profesional", "Professionnel", "Profissional", "Professionell");
            AddTranslation("theme_cyberpunk_desc",
                "Futuristic", "Futurista", "Futuriste", "Futurista", "Futuristisch");
            AddTranslation("theme_ocean_desc",
                "Relaxing", "Relajante", "Relaxant", "Relaxante", "Entspannend");
            AddTranslation("theme_retro_desc",
                "Nostalgic", "Nostálgico", "Nostalgique", "Nostálgico", "Nostalgisch");
            AddTranslation("theme_volcano_desc",
                "Intense", "Intenso", "Intense", "Intenso", "Intensiv");

            // ==================== INPUT HINTS ====================
            AddTranslation("hint_username",
                "3-20 characters. Letters, numbers and _ only", "3-20 caracteres. Solo letras, números y _", "3-20 caractères. Lettres, chiffres et _ uniquement", "3-20 caracteres. Apenas letras, números e _", "3-20 Zeichen. Nur Buchstaben, Zahlen und _");
            AddTranslation("hint_email",
                "Enter a valid email address", "Ingresa un correo válido", "Entrez une adresse e-mail valide", "Digite um e-mail válido", "Gib eine gültige E-Mail-Adresse ein");
            AddTranslation("hint_password",
                "Minimum 6 characters", "Mínimo 6 caracteres", "Minimum 6 caractères", "Mínimo 6 caracteres", "Mindestens 6 Zeichen");
            AddTranslation("hint_confirm_password",
                "Repeat your password", "Repite tu contraseña", "Répétez votre mot de passe", "Repita sua senha", "Wiederhole dein Passwort");
            AddTranslation("placeholder_username",
                "Username (3-20 chars)", "Usuario (3-20 chars)", "Nom d'utilisateur (3-20)", "Usuário (3-20 chars)", "Benutzername (3-20)");
            AddTranslation("placeholder_email",
                "email@example.com", "correo@ejemplo.com", "email@exemple.com", "email@exemplo.com", "email@beispiel.com");
            AddTranslation("placeholder_password",
                "Password (min 6)", "Contraseña (mín 6)", "Mot de passe (min 6)", "Senha (mín 6)", "Passwort (min 6)");
            AddTranslation("placeholder_confirm",
                "Confirm password", "Confirmar contraseña", "Confirmer mot de passe", "Confirmar senha", "Passwort bestätigen");
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
