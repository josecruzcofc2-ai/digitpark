using System;
using UnityEngine;
#if UNITY_ANDROID
using Google.Play.Review;
#endif

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio de review/rating in-app
    /// iOS: usa UnityEngine.iOS.Device.RequestStoreReview()
    /// Android: usa Google Play In-App Review API con fallback a store URL
    /// </summary>
    public class ReviewService : MonoBehaviour
    {
        public static ReviewService Instance { get; private set; }

        // PlayerPrefs keys
        private const string KEY_LAST_ASKED = "ReviewLastAsked";
        private const string KEY_SESSION_COUNT = "ReviewSessionCount";
        private const string KEY_GAMES_PLAYED = "ReviewGamesPlayed";
        private const string KEY_REVIEW_GIVEN = "ReviewGiven";

        // Configuracion
        private const int MIN_SESSIONS = 3;
        private const int MIN_GAMES = 5;
        private const int DAYS_BETWEEN_ASKS = 90;

        /// <summary>
        /// Numero de sesiones registradas
        /// </summary>
        public int SessionCount { get; private set; }

        /// <summary>
        /// Numero de partidas completadas registradas
        /// </summary>
        public int GamesPlayed { get; private set; }

        /// <summary>
        /// Si ya se dio review
        /// </summary>
        public bool HasGivenReview { get; private set; }

#if UNITY_ANDROID && !UNITY_EDITOR
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadState();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadState()
        {
            SessionCount = PlayerPrefs.GetInt(KEY_SESSION_COUNT, 0);
            GamesPlayed = PlayerPrefs.GetInt(KEY_GAMES_PLAYED, 0);
            HasGivenReview = PlayerPrefs.GetInt(KEY_REVIEW_GIVEN, 0) == 1;
            Debug.Log($"[Review] Estado cargado - Sesiones: {SessionCount}, Partidas: {GamesPlayed}");
        }

        /// <summary>
        /// Incrementa el contador de sesiones. Llamar al inicio de cada sesion de app.
        /// </summary>
        public void IncrementSessionCount()
        {
            SessionCount++;
            PlayerPrefs.SetInt(KEY_SESSION_COUNT, SessionCount);
            PlayerPrefs.Save();
            Debug.Log($"[Review] Sesion #{SessionCount}");
        }

        /// <summary>
        /// Incrementa el contador de partidas completadas.
        /// </summary>
        public void IncrementGamesPlayed()
        {
            GamesPlayed++;
            PlayerPrefs.SetInt(KEY_GAMES_PLAYED, GamesPlayed);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Intenta mostrar el prompt de review si se cumplen las condiciones.
        /// Llamar despues de eventos positivos (ganar, achievement, racha).
        /// </summary>
        /// <param name="justLost">True si el usuario acaba de perder una partida</param>
        /// <returns>True si se mostro el prompt</returns>
        public bool TryRequestReview(bool justLost = false)
        {
            if (!ShouldShowReview(justLost))
            {
                return false;
            }

            RequestReview();
            return true;
        }

        /// <summary>
        /// Verifica si se deben cumplir todas las condiciones para mostrar review
        /// </summary>
        private bool ShouldShowReview(bool justLost)
        {
            // No mostrar si ya dio review
            if (HasGivenReview) return false;

            // No mostrar si no tiene suficientes sesiones
            if (SessionCount < MIN_SESSIONS) return false;

            // No mostrar si no tiene suficientes partidas
            if (GamesPlayed < MIN_GAMES) return false;

            // No mostrar si acaba de perder
            if (justLost) return false;

            // No mostrar si pidio hace menos de 90 dias
            string lastAsked = PlayerPrefs.GetString(KEY_LAST_ASKED, "");
            if (!string.IsNullOrEmpty(lastAsked))
            {
                if (DateTime.TryParse(lastAsked, out DateTime lastDate))
                {
                    if ((DateTime.Now - lastDate).TotalDays < DAYS_BETWEEN_ASKS)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Muestra el prompt nativo de review
        /// </summary>
        private void RequestReview()
        {
            // Registrar que se pidio
            PlayerPrefs.SetString(KEY_LAST_ASKED, DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.SetInt(KEY_REVIEW_GIVEN, 1); // Asumir que se dio (no hay callback confiable)
            PlayerPrefs.Save();
            HasGivenReview = true;

            Debug.Log("[Review] Solicitando review...");

#if UNITY_IOS && !UNITY_EDITOR
            UnityEngine.iOS.Device.RequestStoreReview();
            Debug.Log("[Review] iOS StoreReview solicitado");
#elif UNITY_ANDROID && !UNITY_EDITOR
            RequestAndroidReview();
#else
            Debug.Log("[Review] Review no disponible en esta plataforma (Editor)");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private async void RequestAndroidReview()
        {
            try
            {
                _reviewManager = new ReviewManager();
                var requestOperation = _reviewManager.RequestReviewFlow();

                // Esperar a que termine la solicitud
                while (!requestOperation.IsDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                    if (this == null) return; // Object destroyed while waiting
                }

                if (requestOperation.Error != ReviewErrorCode.NoError)
                {
                    Debug.LogWarning($"[Review] Error Android review: {requestOperation.Error}");
                    OpenStoreFallback();
                    return;
                }

                _playReviewInfo = requestOperation.GetResult();
                var launchOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);

                while (!launchOperation.IsDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                    if (this == null) return; // Object destroyed while waiting
                }

                if (launchOperation.Error != ReviewErrorCode.NoError)
                {
                    Debug.LogWarning($"[Review] Error lanzando review flow: {launchOperation.Error}");
                    OpenStoreFallback();
                }
                else
                {
                    Debug.Log("[Review] Android review flow completado");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Review] Exception Android review: {e.Message}");
                OpenStoreFallback();
            }
        }
#endif

        /// <summary>
        /// Abre la pagina del store como fallback
        /// </summary>
        private void OpenStoreFallback()
        {
#if UNITY_ANDROID
            Application.OpenURL($"market://details?id={Application.identifier}");
#elif UNITY_IOS
            // Reemplazar XXXXXXXXXX con el App Store ID real
            Application.OpenURL("itms-apps://itunes.apple.com/app/idXXXXXXXXXX?action=write-review");
#endif
            Debug.Log("[Review] Fallback: abriendo store");
        }
    }
}
