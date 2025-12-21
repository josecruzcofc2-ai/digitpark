using UnityEngine;
using System.Collections;

namespace DigitPark.Effects
{
    /// <summary>
    /// Sistema central de feedback - Coordina todos los efectos visuales y hapticos
    /// para crear una experiencia satisfactoria llena de dopamina
    /// </summary>
    public class FeedbackManager : MonoBehaviour
    {
        public static FeedbackManager Instance { get; private set; }

        [Header("Haptic Settings")]
        [SerializeField] private bool enableHaptics = true;
        [SerializeField] private float lightVibrationDuration = 0.01f;
        [SerializeField] private float mediumVibrationDuration = 0.025f;
        [SerializeField] private float heavyVibrationDuration = 0.05f;

        [Header("Screen Shake")]
        [SerializeField] private float shakeIntensity = 10f;
        [SerializeField] private float shakeDuration = 0.3f;

        [Header("Global Settings")]
        [SerializeField] private bool effectsEnabled = true;
        [SerializeField] [Range(0f, 2f)] private float effectsIntensity = 1f;

        // Referencias a otros managers
        private ParticleSystemManager particleManager;
        private CelebrationManager celebrationManager;

        // Camera shake
        private Vector3 originalCameraPosition;
        private Camera mainCamera;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.localPosition;
            }

            // Buscar o crear los otros managers
            particleManager = GetComponentInChildren<ParticleSystemManager>();
            if (particleManager == null)
            {
                GameObject pmObj = new GameObject("ParticleSystemManager");
                pmObj.transform.SetParent(transform);
                particleManager = pmObj.AddComponent<ParticleSystemManager>();
            }

            celebrationManager = GetComponentInChildren<CelebrationManager>();
            if (celebrationManager == null)
            {
                GameObject cmObj = new GameObject("CelebrationManager");
                cmObj.transform.SetParent(transform);
                celebrationManager = cmObj.AddComponent<CelebrationManager>();
            }

            Debug.Log("[FeedbackManager] Sistema de feedback inicializado");
        }

        #region Button Feedback

        /// <summary>
        /// Feedback ligero para botones normales
        /// </summary>
        public void PlayButtonFeedback(Vector3 worldPosition)
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Light);
            particleManager?.PlayButtonBurst(worldPosition, ParticleType.NeonBurst);
        }

        /// <summary>
        /// Feedback medio para acciones importantes
        /// </summary>
        public void PlayImportantFeedback(Vector3 worldPosition)
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Medium);
            particleManager?.PlayButtonBurst(worldPosition, ParticleType.NeonBurstLarge);
        }

        /// <summary>
        /// Feedback para éxito/victoria
        /// </summary>
        public void PlaySuccessFeedback(Vector3 worldPosition)
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Heavy);
            particleManager?.PlayButtonBurst(worldPosition, ParticleType.SuccessBurst);
            StartCoroutine(ScreenFlash(new Color(0f, 1f, 0.5f, 0.3f), 0.2f));
        }

        /// <summary>
        /// Feedback para error
        /// </summary>
        public void PlayErrorFeedback()
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Heavy);
            ShakeScreen(shakeIntensity * 0.5f, shakeDuration * 0.5f);
            StartCoroutine(ScreenFlash(new Color(1f, 0.2f, 0.2f, 0.3f), 0.15f));
        }

        #endregion

        #region Celebrations

        /// <summary>
        /// Celebracion pequeña (completar nivel)
        /// </summary>
        public void PlaySmallCelebration()
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Heavy);
            celebrationManager?.PlayConfetti(CelebrationType.Small);
            StartCoroutine(ScreenFlash(new Color(1f, 0.84f, 0f, 0.2f), 0.3f));
        }

        /// <summary>
        /// Celebracion grande (nuevo record)
        /// </summary>
        public void PlayBigCelebration()
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Heavy);
            celebrationManager?.PlayConfetti(CelebrationType.Big);
            celebrationManager?.PlayFireworks();
            ShakeScreen(shakeIntensity * 0.3f, 0.5f);
            StartCoroutine(ScreenFlash(new Color(1f, 0.84f, 0f, 0.4f), 0.5f));
            StartCoroutine(PulseEffect());
        }

        /// <summary>
        /// Celebracion epica (logro especial)
        /// </summary>
        public void PlayEpicCelebration()
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Heavy);
            celebrationManager?.PlayConfetti(CelebrationType.Epic);
            celebrationManager?.PlayFireworks();
            celebrationManager?.PlayStarBurst();
            ShakeScreen(shakeIntensity * 0.5f, 0.8f);
            StartCoroutine(EpicScreenEffect());
        }

        #endregion

        #region Gameplay Feedback

        /// <summary>
        /// Feedback al tocar un tile del juego
        /// </summary>
        public void PlayTileTap(Vector3 worldPosition)
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Light);
            particleManager?.PlayRipple(worldPosition);
        }

        /// <summary>
        /// Feedback al acertar
        /// </summary>
        public void PlayCorrectMove(Vector3 worldPosition)
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Medium);
            particleManager?.PlayButtonBurst(worldPosition, ParticleType.SuccessBurst);
        }

        /// <summary>
        /// Feedback al fallar
        /// </summary>
        public void PlayWrongMove(Vector3 worldPosition)
        {
            if (!effectsEnabled) return;

            PlayHaptic(HapticType.Medium);
            particleManager?.PlayButtonBurst(worldPosition, ParticleType.ErrorBurst);
            ShakeScreen(shakeIntensity * 0.3f, 0.15f);
        }

        /// <summary>
        /// Feedback de combo
        /// </summary>
        public void PlayCombo(Vector3 worldPosition, int comboCount)
        {
            if (!effectsEnabled) return;

            float intensity = Mathf.Min(comboCount * 0.2f, 1f);
            PlayHaptic(comboCount >= 5 ? HapticType.Heavy : HapticType.Medium);
            particleManager?.PlayComboBurst(worldPosition, comboCount);

            if (comboCount >= 3)
            {
                ShakeScreen(shakeIntensity * intensity * 0.3f, 0.2f);
            }
        }

        #endregion

        #region Screen Effects

        /// <summary>
        /// Sacude la pantalla
        /// </summary>
        public void ShakeScreen(float intensity = -1f, float duration = -1f)
        {
            if (!effectsEnabled || mainCamera == null) return;

            if (intensity < 0) intensity = shakeIntensity;
            if (duration < 0) duration = shakeDuration;

            intensity *= effectsIntensity;

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                mainCamera.transform.localPosition = originalCameraPosition;
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        private IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity * (1f - elapsed / duration);
                float y = Random.Range(-1f, 1f) * intensity * (1f - elapsed / duration);

                mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            mainCamera.transform.localPosition = originalCameraPosition;
            shakeCoroutine = null;
        }

        /// <summary>
        /// Flash de pantalla con color
        /// </summary>
        public IEnumerator ScreenFlash(Color color, float duration)
        {
            // Crear overlay temporal
            GameObject flashObj = new GameObject("ScreenFlash");
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                flashObj.transform.SetParent(canvas.transform, false);
            }

            RectTransform rt = flashObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            UnityEngine.UI.Image img = flashObj.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            img.raycastTarget = false;

            // Hacer que aparezca encima de todo
            flashObj.transform.SetAsLastSibling();

            // Fade out
            float elapsed = 0f;
            Color startColor = color;
            Color endColor = new Color(color.r, color.g, color.b, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                img.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }

            Destroy(flashObj);
        }

        private IEnumerator PulseEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                yield return ScreenFlash(new Color(1f, 0.84f, 0f, 0.15f), 0.2f);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator EpicScreenEffect()
        {
            // Flash blanco inicial
            yield return ScreenFlash(new Color(1f, 1f, 1f, 0.5f), 0.3f);

            // Pulsos dorados
            for (int i = 0; i < 5; i++)
            {
                yield return ScreenFlash(new Color(1f, 0.84f, 0f, 0.2f), 0.15f);
                yield return new WaitForSeconds(0.05f);
            }
        }

        #endregion

        #region Haptic Feedback

        public enum HapticType
        {
            Light,
            Medium,
            Heavy
        }

        public void PlayHaptic(HapticType type)
        {
            if (!enableHaptics) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidHaptic(type);
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHaptic(type);
#endif
        }

        private void AndroidHaptic(HapticType type)
        {
#if UNITY_ANDROID
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    long duration = type switch
                    {
                        HapticType.Light => 10,
                        HapticType.Medium => 25,
                        HapticType.Heavy => 50,
                        _ => 10
                    };
                    vibrator.Call("vibrate", duration);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FeedbackManager] Haptic error: {e.Message}");
            }
#endif
        }

        private void iOSHaptic(HapticType type)
        {
#if UNITY_IOS
            // iOS usa el sistema de haptics nativo
            // Requiere el plugin iOS Native Haptics o similar
            // Por ahora usamos Handheld.Vibrate como fallback
            Handheld.Vibrate();
#endif
        }

        #endregion

        #region Settings

        public void SetEffectsEnabled(bool enabled)
        {
            effectsEnabled = enabled;
            PlayerPrefs.SetInt("EffectsEnabled", enabled ? 1 : 0);
        }

        public void SetHapticsEnabled(bool enabled)
        {
            enableHaptics = enabled;
            PlayerPrefs.SetInt("HapticsEnabled", enabled ? 1 : 0);
        }

        public void SetEffectsIntensity(float intensity)
        {
            effectsIntensity = Mathf.Clamp(intensity, 0f, 2f);
            PlayerPrefs.SetFloat("EffectsIntensity", effectsIntensity);
        }

        private void LoadSettings()
        {
            effectsEnabled = PlayerPrefs.GetInt("EffectsEnabled", 1) == 1;
            enableHaptics = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;
            effectsIntensity = PlayerPrefs.GetFloat("EffectsIntensity", 1f);
        }

        #endregion
    }
}
