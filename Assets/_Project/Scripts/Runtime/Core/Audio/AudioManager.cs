using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.Managers
{
    /// <summary>
    /// Manager de audio del juego
    /// Gestiona música de fondo y efectos de sonido
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip leaderboardMusic;
        [Header("SFX Clips")]
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip correctTouchSFX;
        [SerializeField] private AudioClip wrongTouchSFX;
        [SerializeField] private AudioClip gameCompleteSFX;
        [SerializeField] private AudioClip newRecordSFX;
        [SerializeField] private AudioClip coinsSFX;
        [SerializeField] private AudioClip levelUpSFX;

        [Header("Settings")]
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 0.8f;
        [SerializeField] private float crossfadeDuration = 1f;

        // Diccionario de SFX para acceso rápido
        private Dictionary<string, AudioClip> sfxDictionary;

        // Crossfade
        private AudioSource _crossfadeSource;
        private float crossfadeTimer = 0f;
        private bool isCrossfading = false;

        private void Awake()
        {
            // Singleton
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

        private void Update()
        {
            // Actualizar crossfade si está activo
            if (isCrossfading)
            {
                UpdateCrossfade();
            }
        }

        /// <summary>
        /// Inicializa el AudioManager
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[Audio] AudioManager iniciado");

            // Crear AudioSources si no existen
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            // Create secondary AudioSource for crossfading
            {
                GameObject crossfadeObj = new GameObject("CrossfadeSource");
                crossfadeObj.transform.SetParent(transform);
                _crossfadeSource = crossfadeObj.AddComponent<AudioSource>();
                _crossfadeSource.loop = true;
                _crossfadeSource.playOnAwake = false;
                _crossfadeSource.volume = 0f;
            }

            // Inicializar diccionario de SFX
            InitializeSFXDictionary();

            // Cargar configuraciones guardadas
            LoadAudioSettings();
        }

        /// <summary>
        /// Inicializa el diccionario de efectos de sonido
        /// </summary>
        private void InitializeSFXDictionary()
        {
            sfxDictionary = new Dictionary<string, AudioClip>
            {
                { "ButtonClick", buttonClickSFX },
                { "CorrectTouch", correctTouchSFX },
                { "WrongTouch", wrongTouchSFX },
                { "GameComplete", gameCompleteSFX },
                { "NewRecord", newRecordSFX },
                { "Coins", coinsSFX },
                { "LevelUp", levelUpSFX }
            };

            Debug.Log($"[Audio] {sfxDictionary.Count} efectos de sonido registrados");
        }

        #region Music

        /// <summary>
        /// Reproduce música de fondo
        /// </summary>
        public void PlayMusic(string musicName, bool crossfade = true)
        {
            AudioClip clip = GetMusicClip(musicName);

            if (clip == null)
            {
                Debug.LogWarning($"[Audio] Música no encontrada: {musicName}");
                return;
            }

            PlayMusic(clip, crossfade);
        }

        /// <summary>
        /// Reproduce un clip de música
        /// </summary>
        public void PlayMusic(AudioClip clip, bool crossfade = true)
        {
            if (clip == null) return;

            // Si ya está sonando, no hacer nada
            if (musicSource.isPlaying && musicSource.clip == clip)
                return;

            // Also skip if we're already crossfading to this clip
            if (isCrossfading && _crossfadeSource != null && _crossfadeSource.clip == clip)
                return;

            if (crossfade && musicSource.isPlaying)
            {
                // Crossfade
                StartCrossfade(clip);
            }
            else
            {
                // Cambio directo
                musicSource.clip = clip;
                musicSource.Play();
            }

            Debug.Log($"[Audio] Reproduciendo música: {clip.name}");
        }

        /// <summary>
        /// Detiene la música
        /// </summary>
        public void StopMusic(bool fade = true)
        {
            // Cancel any in-progress crossfade
            if (isCrossfading)
            {
                isCrossfading = false;
                _crossfadeSource.Stop();
                _crossfadeSource.volume = 0f;
            }

            if (fade)
            {
                StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Pausa la música
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
            if (isCrossfading && _crossfadeSource != null)
                _crossfadeSource.Pause();
        }

        /// <summary>
        /// Reanuda la música
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
            if (isCrossfading && _crossfadeSource != null)
                _crossfadeSource.UnPause();
        }

        /// <summary>
        /// Obtiene un clip de música por nombre
        /// </summary>
        private AudioClip GetMusicClip(string name)
        {
            switch (name.ToLower())
            {
                case "mainmenu": return mainMenuMusic;
                case "gameplay": return gameplayMusic;
                case "leaderboard": return leaderboardMusic;
                default: return null;
            }
        }

        #endregion

        #region SFX

        /// <summary>
        /// Reproduce un efecto de sonido
        /// </summary>
        public void PlaySFX(string sfxName)
        {
            if (sfxDictionary.ContainsKey(sfxName))
            {
                PlaySFX(sfxDictionary[sfxName]);
            }
            else
            {
                Debug.LogWarning($"[Audio] SFX no encontrado: {sfxName}");
            }
        }

        /// <summary>
        /// Reproduce un clip de SFX
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;

            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// Reproduce SFX con pitch aleatorio (variación).
        /// Uses a dedicated AudioSource to avoid pitch affecting other SFX via PlayOneShot.
        /// </summary>
        public void PlaySFXWithRandomPitch(string sfxName, float minPitch = 0.9f, float maxPitch = 1.1f)
        {
            if (sfxDictionary.ContainsKey(sfxName))
            {
                AudioClip clip = sfxDictionary[sfxName];
                if (clip == null) return;

                EnsurePitchedSFXSource();
                _pitchedSfxSource.pitch = Random.Range(minPitch, maxPitch);
                _pitchedSfxSource.clip = clip;
                _pitchedSfxSource.volume = sfxVolume;
                _pitchedSfxSource.Play();
            }
        }

        /// <summary>Dedicated AudioSource for pitched SFX, so pitch changes don't affect the main sfxSource.</summary>
        private AudioSource _pitchedSfxSource;

        private void EnsurePitchedSFXSource()
        {
            if (_pitchedSfxSource != null) return;
            GameObject pitchedObj = new GameObject("PitchedSFXSource");
            pitchedObj.transform.SetParent(transform);
            _pitchedSfxSource = pitchedObj.AddComponent<AudioSource>();
            _pitchedSfxSource.loop = false;
            _pitchedSfxSource.playOnAwake = false;
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// Establece el volumen de la música
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;

            PlayerPrefs.SetFloat("DP_MusicVolume", musicVolume);
            PlayerPrefs.Save();

            Debug.Log($"[Audio] Volumen de música: {musicVolume}");
        }

        /// <summary>
        /// Establece el volumen de los SFX
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;

            PlayerPrefs.SetFloat("DP_SFXVolume", sfxVolume);
            PlayerPrefs.Save();

            Debug.Log($"[Audio] Volumen de SFX: {sfxVolume}");
        }

        /// <summary>
        /// Obtiene el volumen de música actual
        /// </summary>
        public float GetMusicVolume()
        {
            return musicVolume;
        }

        /// <summary>
        /// Obtiene el volumen de SFX actual
        /// </summary>
        public float GetSFXVolume()
        {
            return sfxVolume;
        }

        #endregion

        #region Crossfade

        /// <summary>
        /// Inicia un crossfade a una nueva música using two AudioSources
        /// </summary>
        private void StartCrossfade(AudioClip newClip)
        {
            // Set the new clip on _crossfadeSource and start playing at volume 0
            _crossfadeSource.clip = newClip;
            _crossfadeSource.volume = 0f;
            _crossfadeSource.Play();

            crossfadeTimer = 0f;
            isCrossfading = true;

            Debug.Log($"[Audio] Iniciando crossfade a: {newClip.name}");
        }

        /// <summary>
        /// Actualiza el crossfade interpolando volumes between both sources
        /// </summary>
        private void UpdateCrossfade()
        {
            crossfadeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(crossfadeTimer / crossfadeDuration);

            // Fade old source down, fade new source up
            musicSource.volume = Mathf.Lerp(musicVolume, 0f, progress);
            _crossfadeSource.volume = Mathf.Lerp(0f, musicVolume, progress);

            if (progress >= 1f)
            {
                // Crossfade complete — stop the old source and swap references
                musicSource.Stop();
                musicSource.volume = 0f;

                // Swap: _crossfadeSource becomes the active musicSource
                AudioSource oldSource = musicSource;
                musicSource = _crossfadeSource;
                _crossfadeSource = oldSource;

                musicSource.volume = musicVolume;
                isCrossfading = false;

                Debug.Log($"[Audio] Crossfade completado: {musicSource.clip.name}");
            }
        }

        /// <summary>
        /// Fade out de la música
        /// </summary>
        private System.Collections.IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / crossfadeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = musicVolume;
        }

        #endregion

        #region Settings

        /// <summary>
        /// Carga las configuraciones de audio guardadas
        /// </summary>
        private void LoadAudioSettings()
        {
            if (PlayerPrefs.HasKey("DP_MusicVolume"))
            {
                musicVolume = PlayerPrefs.GetFloat("DP_MusicVolume");
                if (musicSource != null) musicSource.volume = musicVolume;
            }

            if (PlayerPrefs.HasKey("DP_SFXVolume"))
            {
                sfxVolume = PlayerPrefs.GetFloat("DP_SFXVolume");
                if (sfxSource != null) sfxSource.volume = sfxVolume;
            }

            Debug.Log($"[Audio] Configuración cargada - Música: {musicVolume}, SFX: {sfxVolume}");
        }

        /// <summary>
        /// Mutea todo el audio
        /// </summary>
        public void MuteAll(bool mute)
        {
            musicSource.mute = mute;
            sfxSource.mute = mute;
            if (_crossfadeSource != null) _crossfadeSource.mute = mute;
            if (_pitchedSfxSource != null) _pitchedSfxSource.mute = mute;

            Debug.Log($"[Audio] Audio muteado: {mute}");
        }

        #endregion
    }
}
