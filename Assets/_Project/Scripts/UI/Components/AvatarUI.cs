using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DigitPark.Services;

namespace DigitPark.UI.Components
{
    /// <summary>
    /// Componente UI para mostrar avatares de usuarios
    /// Soporta:
    /// - Carga automática del avatar del usuario actual
    /// - Carga de avatares de otros usuarios por ID
    /// - Estado de carga con spinner/placeholder
    /// - Botón para cambiar avatar (si es editable)
    /// - Icono por defecto cuando no hay avatar
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class AvatarUI : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private bool loadCurrentUserOnStart = true;
        [SerializeField] private bool isEditable = false;
        [SerializeField] private string targetUserId = "";

        [Header("UI References")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Image defaultIconImage;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private Button editButton;
        [SerializeField] private Image progressFill;

        [Header("Sprites")]
        [SerializeField] private Sprite defaultAvatarSprite;
        [SerializeField] private Sprite loadingSprite;

        // Estado
        private bool isLoading = false;
        private string currentLoadedUserId = "";

        #region Unity Lifecycle

        private void Awake()
        {
            if (avatarImage == null)
            {
                avatarImage = GetComponent<Image>();
            }
        }

        private async void Start()
        {
            SetupUI();

            if (loadCurrentUserOnStart)
            {
                await LoadCurrentUserAvatar();
            }
            else if (!string.IsNullOrEmpty(targetUserId))
            {
                await LoadUserAvatar(targetUserId, "");
            }
        }

        private void OnEnable()
        {
            // Suscribirse a cambios de avatar
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.OnAvatarChanged += OnAvatarChanged;
                AvatarService.Instance.OnUploadProgress += OnUploadProgress;
                AvatarService.Instance.OnError += OnAvatarError;
            }
        }

        private void OnDisable()
        {
            // Desuscribirse
            if (AvatarService.Instance != null)
            {
                AvatarService.Instance.OnAvatarChanged -= OnAvatarChanged;
                AvatarService.Instance.OnUploadProgress -= OnUploadProgress;
                AvatarService.Instance.OnError -= OnAvatarError;
            }
        }

        #endregion

        #region Setup

        private void SetupUI()
        {
            // Configurar botón de edición
            if (editButton != null)
            {
                editButton.gameObject.SetActive(isEditable);
                editButton.onClick.AddListener(OnEditButtonClicked);
            }

            // Ocultar indicador de carga inicialmente
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }

            // Ocultar progress fill
            if (progressFill != null)
            {
                progressFill.fillAmount = 0f;
                progressFill.gameObject.SetActive(false);
            }

            // Mostrar sprite por defecto
            SetDefaultState();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Carga el avatar del usuario actual
        /// </summary>
        public async Task LoadCurrentUserAvatar()
        {
            if (AvatarService.Instance == null)
            {
                Debug.LogWarning("[AvatarUI] AvatarService no disponible");
                SetDefaultState();
                return;
            }

            SetLoadingState(true);

            try
            {
                Sprite avatar = await AvatarService.Instance.LoadCurrentUserAvatar();
                SetAvatarSprite(avatar);

                var playerData = GetCurrentPlayerData();
                if (playerData != null)
                {
                    currentLoadedUserId = playerData.userId;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AvatarUI] Error cargando avatar: {e.Message}");
                SetDefaultState();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Carga el avatar de un usuario específico
        /// </summary>
        public async Task LoadUserAvatar(string userId, string avatarUrl)
        {
            if (string.IsNullOrEmpty(userId))
            {
                SetDefaultState();
                return;
            }

            if (AvatarService.Instance == null)
            {
                Debug.LogWarning("[AvatarUI] AvatarService no disponible");
                SetDefaultState();
                return;
            }

            // Evitar recargar si ya está cargado
            if (currentLoadedUserId == userId && avatarImage.sprite != null && avatarImage.sprite != defaultAvatarSprite)
            {
                return;
            }

            SetLoadingState(true);

            try
            {
                Sprite avatar = await AvatarService.Instance.LoadAvatar(userId, avatarUrl);
                SetAvatarSprite(avatar);
                currentLoadedUserId = userId;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AvatarUI] Error cargando avatar de {userId}: {e.Message}");
                SetDefaultState();
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Establece un sprite directamente (sin cargar)
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            SetAvatarSprite(sprite ?? defaultAvatarSprite);
        }

        /// <summary>
        /// Fuerza una recarga del avatar
        /// </summary>
        public async Task Refresh()
        {
            currentLoadedUserId = "";

            if (loadCurrentUserOnStart)
            {
                await LoadCurrentUserAvatar();
            }
            else if (!string.IsNullOrEmpty(targetUserId))
            {
                // Invalidar cache antes de recargar
                AvatarService.Instance?.InvalidateCache(targetUserId);
                await LoadUserAvatar(targetUserId, "");
            }
        }

        /// <summary>
        /// Configura el componente para un usuario específico
        /// </summary>
        public void SetTargetUser(string userId, string avatarUrl = "")
        {
            targetUserId = userId;
            loadCurrentUserOnStart = false;
            _ = LoadUserAvatar(userId, avatarUrl);
        }

        /// <summary>
        /// Habilita o deshabilita la edición
        /// </summary>
        public void SetEditable(bool editable)
        {
            isEditable = editable;
            if (editButton != null)
            {
                editButton.gameObject.SetActive(editable);
            }
        }

        #endregion

        #region UI State Management

        private void SetLoadingState(bool loading)
        {
            isLoading = loading;

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(loading);
            }

            if (avatarImage != null)
            {
                avatarImage.color = loading ? new Color(1, 1, 1, 0.5f) : Color.white;
            }
        }

        private void SetDefaultState()
        {
            if (avatarImage != null)
            {
                if (defaultAvatarSprite != null)
                {
                    avatarImage.sprite = defaultAvatarSprite;
                }
                avatarImage.color = Color.white;
            }

            // Mostrar icono por defecto si existe
            if (defaultIconImage != null)
            {
                defaultIconImage.gameObject.SetActive(true);
            }
        }

        private void SetAvatarSprite(Sprite sprite)
        {
            if (avatarImage != null)
            {
                avatarImage.sprite = sprite;
                avatarImage.color = Color.white;
            }

            // Ocultar icono por defecto si hay avatar real
            if (defaultIconImage != null)
            {
                bool hasRealAvatar = sprite != null && sprite != defaultAvatarSprite;
                defaultIconImage.gameObject.SetActive(!hasRealAvatar);
            }
        }

        private void ShowUploadProgress(bool show)
        {
            if (progressFill != null)
            {
                progressFill.gameObject.SetActive(show);
                if (!show)
                {
                    progressFill.fillAmount = 0f;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnEditButtonClicked()
        {
            if (AvatarService.Instance != null && !isLoading)
            {
                ShowUploadProgress(true);
                AvatarService.Instance.PickAvatarFromGallery();
            }
        }

        private void OnAvatarChanged(Sprite newAvatar)
        {
            // Solo actualizar si este componente muestra el usuario actual
            if (loadCurrentUserOnStart)
            {
                SetAvatarSprite(newAvatar);
                ShowUploadProgress(false);
            }
        }

        private void OnUploadProgress(float progress)
        {
            if (progressFill != null && loadCurrentUserOnStart)
            {
                progressFill.fillAmount = progress;
            }
        }

        private void OnAvatarError(string error)
        {
            Debug.LogWarning($"[AvatarUI] Error: {error}");
            ShowUploadProgress(false);
            SetLoadingState(false);
        }

        #endregion

        #region Helpers

        private Data.PlayerData GetCurrentPlayerData()
        {
            if (Services.Firebase.AuthenticationService.Instance != null)
            {
                return Services.Firebase.AuthenticationService.Instance.GetCurrentPlayerData();
            }
            return null;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (avatarImage == null)
            {
                avatarImage = GetComponent<Image>();
            }
        }
#endif

        #endregion
    }
}
