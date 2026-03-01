using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Storage;
using DigitPark.Data;
using DigitPark.UI.Components;

namespace DigitPark.Services
{
    /// <summary>
    /// Service for user avatar management with Firebase Storage
    /// - Image selection from gallery (requires NativeGallery plugin)
    /// - Real upload to Firebase Storage
    /// - Local cache for fast loading
    /// - Fallback to default icon
    ///
    /// REQUIREMENTS:
    /// 1. Firebase SDK installed (com.google.firebase.storage)
    /// 2. NativeGallery plugin: https://github.com/yasirkula/UnityNativeGallery
    /// 3. Firebase Storage rules configured
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

        // Events
        public event Action<Sprite> OnAvatarChanged;
        public event Action<float> OnUploadProgress;
        public event Action<string> OnError;

        // Cache
        private string cacheDirectory;
        private Sprite currentAvatarSprite;
        private bool isUploading = false;

        // Constants
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
            // Configure cache directory
            cacheDirectory = Path.Combine(Application.persistentDataPath, "AvatarCache");

            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            // Initialize Firebase Storage
            await InitializeFirebase();

            Debug.Log($"[AvatarService] Initialized. Cache: {cacheDirectory}");
        }

        private async Task InitializeFirebase()
        {
            try
            {
                // Use full namespace to avoid conflict with DigitPark.Services.Firebase
                var dependencyStatus = await global::Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus == global::Firebase.DependencyStatus.Available)
                {
                    storage = FirebaseStorage.DefaultInstance;
                    storageRoot = storage.GetReference(STORAGE_AVATARS_PATH);
                    isFirebaseInitialized = true;
                    Debug.Log("[AvatarService] Firebase Storage initialized successfully");
                }
                else
                {
                    Debug.LogError($"[AvatarService] Firebase dependencies error: {dependencyStatus}");
                    isFirebaseInitialized = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error initializing Firebase: {e.Message}");
                isFirebaseInitialized = false;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Checks if the service is ready
        /// </summary>
        public bool IsReady => isFirebaseInitialized;

        /// <summary>
        /// Opens the device image picker
        /// </summary>
        public void PickAvatarFromGallery()
        {
            if (!isFirebaseInitialized)
            {
                Debug.LogError("[AvatarService] Firebase is not initialized");
                OnError?.Invoke("Service unavailable. Try again later.");
                return;
            }

            if (isUploading)
            {
                Debug.LogWarning("[AvatarService] Upload already in progress");
                OnError?.Invoke("An upload is already in progress");
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            PickImageNative();
#else
            Debug.LogWarning("[AvatarService] Gallery selection only available on mobile");
            OnError?.Invoke("Feature only available on mobile devices");
#endif
        }

        /// <summary>
        /// Loads the current user's avatar
        /// </summary>
        public async Task<Sprite> LoadCurrentUserAvatar()
        {
            var playerData = GetCurrentPlayerData();
            if (playerData == null)
            {
                Debug.LogWarning("[AvatarService] No authenticated user");
                return defaultAvatarSprite;
            }

            return await LoadAvatar(playerData.userId, playerData.avatarUrl, playerData.username);
        }

        /// <summary>
        /// Loads any user's avatar
        /// </summary>
        public async Task<Sprite> LoadAvatar(string userId, string avatarUrl, string username = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return defaultAvatarSprite;
            }

            // 1. Try to load from local cache
            Sprite cachedSprite = LoadFromCache(userId);
            if (cachedSprite != null)
            {
                Debug.Log($"[AvatarService] Avatar loaded from cache: {userId}");

                // If current user, update reference
                var currentPlayer = GetCurrentPlayerData();
                if (currentPlayer != null && currentPlayer.userId == userId)
                {
                    currentAvatarSprite = cachedSprite;
                }

                return cachedSprite;
            }

            // 2. If URL exists, download from Firebase
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                Sprite downloadedSprite = await DownloadAvatar(userId, avatarUrl);
                if (downloadedSprite != null)
                {
                    return downloadedSprite;
                }
            }

            // 3. Fallback: generate avatar with username initial
            string displayName = username;
            if (string.IsNullOrEmpty(displayName))
            {
                // Try to get username from current PlayerData
                var currentPlayer = GetCurrentPlayerData();
                if (currentPlayer != null && currentPlayer.userId == userId)
                {
                    displayName = currentPlayer.username;
                }
            }

            if (!string.IsNullOrEmpty(displayName))
            {
                Debug.Log($"[AvatarService] Generating avatar with initial for: {userId} ({displayName})");
                return AvatarInitialGenerator.GenerateAvatar(displayName, userId);
            }

            // 4. Last fallback: generate with userId as name
            Debug.Log($"[AvatarService] Generating avatar with userId for: {userId}");
            return AvatarInitialGenerator.GenerateAvatar(userId, userId);
        }

        /// <summary>
        /// Gets the current avatar sprite (in-memory cache)
        /// </summary>
        public Sprite GetCurrentAvatarSprite()
        {
            return currentAvatarSprite ?? defaultAvatarSprite;
        }

        /// <summary>
        /// Gets the default sprite
        /// </summary>
        public Sprite GetDefaultAvatarSprite()
        {
            return defaultAvatarSprite;
        }

        /// <summary>
        /// Clears the avatar cache
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
                Debug.Log("[AvatarService] Cache cleared");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error clearing cache: {e.Message}");
            }
        }

        /// <summary>
        /// Removes the current user's avatar from Firebase
        /// </summary>
        public async Task<bool> RemoveAvatar()
        {
            var playerData = GetCurrentPlayerData();
            if (playerData == null) return false;

            try
            {
                // Delete from Firebase Storage
                if (isFirebaseInitialized)
                {
                    StorageReference avatarRef = storageRoot.Child(playerData.userId).Child(AVATAR_FILENAME);
                    await avatarRef.DeleteAsync();
                    Debug.Log("[AvatarService] Avatar deleted from Firebase Storage");
                }

                // Clear URL in player data
                playerData.avatarUrl = "";
                await SavePlayerData(playerData);

                // Clear local cache
                string cachePath = GetCachePath(playerData.userId);
                if (File.Exists(cachePath))
                {
                    File.Delete(cachePath);
                }

                currentAvatarSprite = null;
                OnAvatarChanged?.Invoke(defaultAvatarSprite);

                Debug.Log("[AvatarService] Avatar removed completely");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error removing avatar: {e.Message}");
                OnError?.Invoke("Error removing avatar");
                return false;
            }
        }

        /// <summary>
        /// Invalidates the cache for a specific user (force reload)
        /// </summary>
        public void InvalidateCache(string userId)
        {
            string cachePath = GetCachePath(userId);
            if (File.Exists(cachePath))
            {
                File.Delete(cachePath);
                Debug.Log($"[AvatarService] Cache invalidated for: {userId}");
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
                    Debug.Log($"[AvatarService] Image selected: {path}");
                    ProcessSelectedImage(path);
                }
                else
                {
                    Debug.Log("[AvatarService] Selection cancelled");
                }
            }, "Select Avatar", "image/*");

            if (permission == NativeGallery.Permission.Denied)
            {
                Debug.LogWarning("[AvatarService] Gallery permission denied");
                OnError?.Invoke("Permission denied. Enable photo access in Settings.");
            }
            else if (permission == NativeGallery.Permission.ShouldAsk)
            {
                Debug.Log("[AvatarService] Requesting gallery permission...");
            }
        }
#endif

        private async void ProcessSelectedImage(string path)
        {
            try
            {
                isUploading = true;
                OnUploadProgress?.Invoke(0.1f);

                // Load image from file
                byte[] imageBytes = await Task.Run(() => File.ReadAllBytes(path));
                Texture2D originalTex = new Texture2D(2, 2);
                originalTex.LoadImage(imageBytes);

                OnUploadProgress?.Invoke(0.2f);

                // Process and upload
                await ProcessAndUploadTexture(originalTex);

                // Clean up
                Destroy(originalTex);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error processing image: {e.Message}");
                OnError?.Invoke("Error processing image");
                isUploading = false;
            }
        }

        private async Task ProcessAndUploadTexture(Texture2D originalTex)
        {
            try
            {
                // Resize and crop to square
                Texture2D resizedTex = ResizeAndCropToSquare(originalTex, maxAvatarSize);
                OnUploadProgress?.Invoke(0.3f);

                // Convert to JPEG
                byte[] jpegBytes = resizedTex.EncodeToJPG(jpegQuality);
                OnUploadProgress?.Invoke(0.4f);

                // Upload to Firebase Storage
                string avatarUrl = await UploadToFirebaseStorage(jpegBytes);

                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    var playerData = GetCurrentPlayerData();
                    if (playerData != null)
                    {
                        // Save to local cache
                        SaveToCache(playerData.userId, jpegBytes);

                        // Update URL in player data
                        playerData.avatarUrl = avatarUrl;
                        await SavePlayerData(playerData);
                    }

                    // Create sprite and notify
                    currentAvatarSprite = CreateSprite(resizedTex);
                    OnAvatarChanged?.Invoke(currentAvatarSprite);
                    OnUploadProgress?.Invoke(1f);

                    Debug.Log("[AvatarService] Avatar updated successfully");
                }
                else
                {
                    Destroy(resizedTex);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error in ProcessAndUploadTexture: {e.Message}");
                OnError?.Invoke("Error uploading avatar");
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
                Debug.LogError("[AvatarService] No authenticated user");
                OnError?.Invoke("You must sign in to change your avatar");
                return null;
            }

            if (!isFirebaseInitialized)
            {
                Debug.LogError("[AvatarService] Firebase Storage is not initialized");
                OnError?.Invoke("Service unavailable");
                return null;
            }

            try
            {
                // Reference to file in Storage: avatars/{userId}/avatar.jpg
                StorageReference avatarRef = storageRoot.Child(playerData.userId).Child(AVATAR_FILENAME);

                // Metadata
                var metadata = new MetadataChange
                {
                    ContentType = "image/jpeg"
                };

                // Upload file
                OnUploadProgress?.Invoke(0.5f);

                var uploadTask = avatarRef.PutBytesAsync(imageBytes, metadata);

                // Wait for completion
                await uploadTask;

                OnUploadProgress?.Invoke(0.9f);

                if (uploadTask.IsFaulted || uploadTask.IsCanceled)
                {
                    Debug.LogError($"[AvatarService] Upload error: {uploadTask.Exception?.Message}");
                    OnError?.Invoke("Error uploading avatar");
                    return null;
                }

                // Get download URL
                Uri downloadUri = await avatarRef.GetDownloadUrlAsync();
                string downloadUrl = downloadUri.ToString();

                Debug.Log($"[AvatarService] Avatar uploaded successfully: {downloadUrl}");
                return downloadUrl;
            }
            catch (StorageException se)
            {
                Debug.LogError($"[AvatarService] Firebase Storage error: {se.ErrorCode} - {se.Message}");

                string errorMsg = se.ErrorCode switch
                {
                    StorageException.ErrorQuotaExceeded => "Storage full",
                    StorageException.ErrorNotAuthorized => "Not authorized. Please sign in again.",
                    StorageException.ErrorRetryLimitExceeded => "Connection error. Try again.",
                    _ => "Error uploading avatar"
                };

                OnError?.Invoke(errorMsg);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error uploading avatar: {e.Message}");
                OnError?.Invoke("Error uploading avatar");
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

                        // Save to local cache
                        byte[] imageBytes = tex.EncodeToJPG(jpegQuality);
                        SaveToCache(userId, imageBytes);

                        Sprite sprite = CreateSprite(tex);

                        // If current user, update reference
                        var currentPlayer = GetCurrentPlayerData();
                        if (currentPlayer != null && currentPlayer.userId == userId)
                        {
                            currentAvatarSprite = sprite;
                        }

                        Debug.Log($"[AvatarService] Avatar downloaded: {userId}");
                        return sprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[AvatarService] Error downloading avatar: {request.error}");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error downloading avatar: {e.Message}");
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
                Debug.Log($"[AvatarService] Avatar saved to cache: {userId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error saving cache: {e.Message}");
            }
        }

        private Sprite LoadFromCache(string userId)
        {
            try
            {
                string cachePath = GetCachePath(userId);

                if (File.Exists(cachePath))
                {
                    // Verify file is not corrupt (minimum size)
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
                        // Corrupt file, delete
                        File.Delete(cachePath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AvatarService] Error loading cache: {e.Message}");
            }

            return null;
        }

        private string GetCachePath(string userId)
        {
            // Sanitize userId for safe filename
            string safeUserId = string.Join("_", userId.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(cacheDirectory, $"avatar_{safeUserId}.jpg");
        }

        #endregion

        #region Image Processing Helpers

        /// <summary>
        /// Resizes and crops the image to a centered square
        /// </summary>
        private Texture2D ResizeAndCropToSquare(Texture2D source, int targetSize)
        {
            // Create temporary texture for cropping
            RenderTexture rt = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;

            // Blit
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            GL.Clear(true, true, Color.black);
            Graphics.Blit(source, rt);

            // Read pixels
            Texture2D result = new Texture2D(targetSize, targetSize, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
            result.Apply();

            // Clean up
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
