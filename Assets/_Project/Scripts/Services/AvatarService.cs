using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Storage;
using Firebase.Extensions;
using DigitPark.Data;

namespace DigitPark.Services
{
    /// <summary>
    /// Servicio para gestión de avatares de usuario con Firebase Storage
    /// - Selección de imagen desde galería (requiere NativeGallery plugin)
    /// - Subida real a Firebase Storage
    /// - Cache local para carga rápida
    /// - Fallback a icono por defecto
    ///
    /// REQUISITOS:
    /// 1. Firebase SDK instalado (com.google.firebase.storage)
    /// 2. NativeGallery plugin: https://github.com/yasirkula/UnityNativeGallery
    /// 3. Reglas de Firebase Storage configuradas
    /// </summary>
    public class AvatarService : MonoBehaviour
    {
        public static AvatarService Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxAvatarSize = 256;
        [SerializeField] private int jpegQuality = 85;
        [SerializeField] private float downloadTimeout = 30f;

        [Header("Default Avatar")]
        [SerializeField] private Sprite defaultAvatarSprite;

        // Firebase
        private FirebaseStorage storage;
        private StorageReference storageRoot;
        private bool isFirebaseInitialized = false;

        // Eventos
        public event Action<Sprite> OnAvatarChanged;
        public event Action<float> OnUploadProgress;
        public event Action<string> OnError;

        // Cache
        private string cacheDirectory;
        private Sprite currentAvatarSprite;
        private bool isUploading = false;

        // Constantes
        private const string AVATAR_FILENAME = "avatar.jpg";
        private const string STORAGE_AVATARS_PATH = "avatars";

        #region Unity Lifecycle

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

        private async void Initialize()
        {
            // Configurar directorio de cache
            cacheDirectory = Path.Combine(Application.persistentDataPath, "AvatarCache");

            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            // Inicializar Firebase Storage
            await InitializeFirebase();

            Debug.Log($"[AvatarService] Inicializado. Cache: {cacheDirectory}");
        }

        private async Task InitializeFirebase()
        {
            try
            {
                var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    storage = FirebaseStorage.DefaultInstance;
                    storageRoot = storage.GetReference(STORAGE_AVATARS_PATH);
                    isFirebaseInitialized = true;
                    Debug.Log("[AvatarService] Firebase Storage inicializado correctamente");
                }
                else
                {
                    Debug.LogError($"[AvatarService] Firebase dependencies error: {dependencyStatus}");
                    isFirebaseInitialized = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error inicializando Firebase: {e.Message}");
                isFirebaseInitialized = false;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Verifica si el servicio está listo
        /// </summary>
        public bool IsReady => isFirebaseInitialized;

        /// <summary>
        /// Abre el selector de imágenes del dispositivo
        /// </summary>
        public void PickAvatarFromGallery()
        {
            if (!isFirebaseInitialized)
            {
                Debug.LogError("[AvatarService] Firebase no está inicializado");
                OnError?.Invoke("Servicio no disponible. Intenta más tarde.");
                return;
            }

            if (isUploading)
            {
                Debug.LogWarning("[AvatarService] Ya hay una subida en progreso");
                OnError?.Invoke("Ya hay una subida en progreso");
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            PickImageNative();
#else
            Debug.LogWarning("[AvatarService] Selección de galería solo disponible en móviles");
            OnError?.Invoke("Función solo disponible en dispositivos móviles");
#endif
        }

        /// <summary>
        /// Carga el avatar del usuario actual
        /// </summary>
        public async Task<Sprite> LoadCurrentUserAvatar()
        {
            var playerData = GetCurrentPlayerData();
            if (playerData == null)
            {
                Debug.LogWarning("[AvatarService] No hay usuario autenticado");
                return defaultAvatarSprite;
            }

            return await LoadAvatar(playerData.userId, playerData.avatarUrl);
        }

        /// <summary>
        /// Carga el avatar de cualquier usuario
        /// </summary>
        public async Task<Sprite> LoadAvatar(string userId, string avatarUrl)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return defaultAvatarSprite;
            }

            // 1. Intentar cargar desde cache local
            Sprite cachedSprite = LoadFromCache(userId);
            if (cachedSprite != null)
            {
                Debug.Log($"[AvatarService] Avatar cargado desde cache: {userId}");

                // Si es el usuario actual, actualizar referencia
                var currentPlayer = GetCurrentPlayerData();
                if (currentPlayer != null && currentPlayer.userId == userId)
                {
                    currentAvatarSprite = cachedSprite;
                }

                return cachedSprite;
            }

            // 2. Si hay URL, descargar de Firebase
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                Sprite downloadedSprite = await DownloadAvatar(userId, avatarUrl);
                if (downloadedSprite != null)
                {
                    return downloadedSprite;
                }
            }

            // 3. Fallback a avatar por defecto
            Debug.Log($"[AvatarService] Usando avatar por defecto para: {userId}");
            return defaultAvatarSprite;
        }

        /// <summary>
        /// Obtiene el sprite del avatar actual (cache en memoria)
        /// </summary>
        public Sprite GetCurrentAvatarSprite()
        {
            return currentAvatarSprite ?? defaultAvatarSprite;
        }

        /// <summary>
        /// Obtiene el sprite por defecto
        /// </summary>
        public Sprite GetDefaultAvatarSprite()
        {
            return defaultAvatarSprite;
        }

        /// <summary>
        /// Limpia el cache de avatares
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(cacheDirectory))
                {
                    Directory.Delete(cacheDirectory, true);
                    Directory.CreateDirectory(cacheDirectory);
                }
                currentAvatarSprite = null;
                Debug.Log("[AvatarService] Cache limpiado");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error limpiando cache: {e.Message}");
            }
        }

        /// <summary>
        /// Elimina el avatar del usuario actual de Firebase
        /// </summary>
        public async Task<bool> RemoveAvatar()
        {
            var playerData = GetCurrentPlayerData();
            if (playerData == null) return false;

            try
            {
                // Eliminar de Firebase Storage
                if (isFirebaseInitialized)
                {
                    StorageReference avatarRef = storageRoot.Child(playerData.userId).Child(AVATAR_FILENAME);
                    await avatarRef.DeleteAsync();
                    Debug.Log("[AvatarService] Avatar eliminado de Firebase Storage");
                }

                // Limpiar URL en datos del jugador
                playerData.avatarUrl = "";
                await SavePlayerData(playerData);

                // Limpiar cache local
                string cachePath = GetCachePath(playerData.userId);
                if (File.Exists(cachePath))
                {
                    File.Delete(cachePath);
                }

                currentAvatarSprite = null;
                OnAvatarChanged?.Invoke(defaultAvatarSprite);

                Debug.Log("[AvatarService] Avatar eliminado completamente");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error eliminando avatar: {e.Message}");
                OnError?.Invoke("Error al eliminar el avatar");
                return false;
            }
        }

        /// <summary>
        /// Invalida el cache de un usuario específico (forzar recarga)
        /// </summary>
        public void InvalidateCache(string userId)
        {
            string cachePath = GetCachePath(userId);
            if (File.Exists(cachePath))
            {
                File.Delete(cachePath);
                Debug.Log($"[AvatarService] Cache invalidado para: {userId}");
            }
        }

        #endregion

        #region Image Selection (NativeGallery)

#if UNITY_ANDROID || UNITY_IOS
        private void PickImageNative()
        {
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Debug.Log($"[AvatarService] Imagen seleccionada: {path}");
                    ProcessSelectedImage(path);
                }
                else
                {
                    Debug.Log("[AvatarService] Selección cancelada");
                }
            }, "Seleccionar Avatar", "image/*");

            if (permission == NativeGallery.Permission.Denied)
            {
                Debug.LogWarning("[AvatarService] Permiso de galería denegado");
                OnError?.Invoke("Permiso denegado. Habilita el acceso a fotos en Configuración.");
            }
            else if (permission == NativeGallery.Permission.ShouldAsk)
            {
                Debug.Log("[AvatarService] Solicitando permiso de galería...");
            }
        }
#endif

        private async void ProcessSelectedImage(string path)
        {
            try
            {
                isUploading = true;
                OnUploadProgress?.Invoke(0.1f);

                // Cargar imagen desde archivo
                byte[] imageBytes = await Task.Run(() => File.ReadAllBytes(path));
                Texture2D originalTex = new Texture2D(2, 2);
                originalTex.LoadImage(imageBytes);

                OnUploadProgress?.Invoke(0.2f);

                // Procesar y subir
                await ProcessAndUploadTexture(originalTex);

                // Limpiar
                Destroy(originalTex);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error procesando imagen: {e.Message}");
                OnError?.Invoke("Error al procesar la imagen");
                isUploading = false;
            }
        }

        private async Task ProcessAndUploadTexture(Texture2D originalTex)
        {
            try
            {
                // Redimensionar y recortar a cuadrado
                Texture2D resizedTex = ResizeAndCropToSquare(originalTex, maxAvatarSize);
                OnUploadProgress?.Invoke(0.3f);

                // Convertir a JPEG
                byte[] jpegBytes = resizedTex.EncodeToJPG(jpegQuality);
                OnUploadProgress?.Invoke(0.4f);

                // Subir a Firebase Storage
                string avatarUrl = await UploadToFirebaseStorage(jpegBytes);

                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    var playerData = GetCurrentPlayerData();
                    if (playerData != null)
                    {
                        // Guardar en cache local
                        SaveToCache(playerData.userId, jpegBytes);

                        // Actualizar URL en datos del jugador
                        playerData.avatarUrl = avatarUrl;
                        await SavePlayerData(playerData);
                    }

                    // Crear sprite y notificar
                    currentAvatarSprite = CreateSprite(resizedTex);
                    OnAvatarChanged?.Invoke(currentAvatarSprite);
                    OnUploadProgress?.Invoke(1f);

                    Debug.Log("[AvatarService] Avatar actualizado exitosamente");
                }
                else
                {
                    Destroy(resizedTex);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error en ProcessAndUploadTexture: {e.Message}");
                OnError?.Invoke("Error al subir el avatar");
            }
            finally
            {
                isUploading = false;
            }
        }

        #endregion

        #region Firebase Storage Upload

        private async Task<string> UploadToFirebaseStorage(byte[] imageBytes)
        {
            var playerData = GetCurrentPlayerData();
            if (playerData == null)
            {
                Debug.LogError("[AvatarService] No hay usuario autenticado");
                OnError?.Invoke("Debes iniciar sesión para cambiar tu avatar");
                return null;
            }

            if (!isFirebaseInitialized)
            {
                Debug.LogError("[AvatarService] Firebase Storage no está inicializado");
                OnError?.Invoke("Servicio no disponible");
                return null;
            }

            try
            {
                // Referencia al archivo en Storage: avatars/{userId}/avatar.jpg
                StorageReference avatarRef = storageRoot.Child(playerData.userId).Child(AVATAR_FILENAME);

                // Metadata
                var metadata = new MetadataChange
                {
                    ContentType = "image/jpeg",
                    CustomMetadata = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "uploadedAt", DateTime.UtcNow.ToString("o") },
                        { "userId", playerData.userId }
                    }
                };

                // Subir con seguimiento de progreso
                var uploadTask = avatarRef.PutBytesAsync(imageBytes, metadata);

                // Monitorear progreso
                uploadTask.Progress.ProgressChanged += (sender, args) =>
                {
                    if (args.TotalByteCount > 0)
                    {
                        float progress = 0.4f + (0.5f * args.BytesTransferred / args.TotalByteCount);
                        OnUploadProgress?.Invoke(progress);
                    }
                };

                // Esperar a que termine
                await uploadTask;

                if (uploadTask.IsFaulted || uploadTask.IsCanceled)
                {
                    Debug.LogError($"[AvatarService] Error en subida: {uploadTask.Exception?.Message}");
                    OnError?.Invoke("Error al subir el avatar");
                    return null;
                }

                // Obtener URL de descarga
                Uri downloadUri = await avatarRef.GetDownloadUrlAsync();
                string downloadUrl = downloadUri.ToString();

                Debug.Log($"[AvatarService] Avatar subido exitosamente: {downloadUrl}");
                return downloadUrl;
            }
            catch (StorageException se)
            {
                Debug.LogError($"[AvatarService] Firebase Storage error: {se.ErrorCode} - {se.Message}");

                string errorMsg = se.ErrorCode switch
                {
                    StorageException.ErrorQuotaExceeded => "Almacenamiento lleno",
                    StorageException.ErrorNotAuthorized => "No autorizado. Inicia sesión nuevamente.",
                    StorageException.ErrorRetryLimitExceeded => "Error de conexión. Intenta de nuevo.",
                    _ => "Error al subir el avatar"
                };

                OnError?.Invoke(errorMsg);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error subiendo avatar: {e.Message}");
                OnError?.Invoke("Error al subir el avatar");
                return null;
            }
        }

        #endregion

        #region Download from Firebase

        private async Task<Sprite> DownloadAvatar(string userId, string avatarUrl)
        {
            try
            {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(avatarUrl))
                {
                    request.timeout = (int)downloadTimeout;

                    var operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D tex = DownloadHandlerTexture.GetContent(request);

                        // Guardar en cache local
                        byte[] imageBytes = tex.EncodeToJPG(jpegQuality);
                        SaveToCache(userId, imageBytes);

                        Sprite sprite = CreateSprite(tex);

                        // Si es el usuario actual, actualizar referencia
                        var currentPlayer = GetCurrentPlayerData();
                        if (currentPlayer != null && currentPlayer.userId == userId)
                        {
                            currentAvatarSprite = sprite;
                        }

                        Debug.Log($"[AvatarService] Avatar descargado: {userId}");
                        return sprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[AvatarService] Error descargando avatar: {request.error}");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error descargando avatar: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Local Cache

        private void SaveToCache(string userId, byte[] imageBytes)
        {
            try
            {
                string cachePath = GetCachePath(userId);
                File.WriteAllBytes(cachePath, imageBytes);
                Debug.Log($"[AvatarService] Avatar guardado en cache: {userId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error guardando cache: {e.Message}");
            }
        }

        private Sprite LoadFromCache(string userId)
        {
            try
            {
                string cachePath = GetCachePath(userId);

                if (File.Exists(cachePath))
                {
                    // Verificar que el archivo no esté corrupto (tamaño mínimo)
                    FileInfo fileInfo = new FileInfo(cachePath);
                    if (fileInfo.Length < 100)
                    {
                        File.Delete(cachePath);
                        return null;
                    }

                    byte[] imageBytes = File.ReadAllBytes(cachePath);
                    Texture2D tex = new Texture2D(2, 2);

                    if (tex.LoadImage(imageBytes))
                    {
                        return CreateSprite(tex);
                    }
                    else
                    {
                        // Archivo corrupto, eliminar
                        File.Delete(cachePath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error cargando cache: {e.Message}");
            }

            return null;
        }

        private string GetCachePath(string userId)
        {
            // Sanitizar userId para nombre de archivo seguro
            string safeUserId = string.Join("_", userId.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(cacheDirectory, $"avatar_{safeUserId}.jpg");
        }

        #endregion

        #region Image Processing Helpers

        /// <summary>
        /// Redimensiona y recorta la imagen a un cuadrado centrado
        /// </summary>
        private Texture2D ResizeAndCropToSquare(Texture2D source, int targetSize)
        {
            // Determinar el tamaño del recorte cuadrado (el lado más pequeño)
            int cropSize = Mathf.Min(source.width, source.height);

            // Calcular offset para centrar el recorte
            int offsetX = (source.width - cropSize) / 2;
            int offsetY = (source.height - cropSize) / 2;

            // Crear textura temporal para el recorte
            RenderTexture rt = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;

            // Calcular UVs para el recorte centrado
            float uvOffsetX = (float)offsetX / source.width;
            float uvOffsetY = (float)offsetY / source.height;
            float uvSize = (float)cropSize / Mathf.Max(source.width, source.height);

            // Blit con recorte
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            GL.Clear(true, true, Color.black);

            // Usar Graphics.Blit para copiar
            Graphics.Blit(source, rt);

            // Leer pixels
            Texture2D result = new Texture2D(targetSize, targetSize, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
            result.Apply();

            // Limpiar
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        private Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        #endregion

        #region Data Access Helpers

        private PlayerData GetCurrentPlayerData()
        {
            if (Firebase.AuthenticationService.Instance != null)
            {
                return Firebase.AuthenticationService.Instance.GetCurrentPlayerData();
            }
            return null;
        }

        private async Task SavePlayerData(PlayerData playerData)
        {
            if (Firebase.DatabaseService.Instance != null)
            {
                await Firebase.DatabaseService.Instance.SavePlayerData(playerData);
            }
        }

        #endregion
    }
}
