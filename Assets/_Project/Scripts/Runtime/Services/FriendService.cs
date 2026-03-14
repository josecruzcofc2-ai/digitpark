using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;
using DigitPark.Services.Firebase;
using DigitPark.Localization;

namespace DigitPark.Services
{
    /// <summary>
    /// Resultado de una operación de amistad
    /// </summary>
    public class FriendOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public static FriendOperationResult Successful(string message = null)
        {
            return new FriendOperationResult { Success = true, Message = message };
        }

        public static FriendOperationResult Failed(string message, string errorCode = null)
        {
            return new FriendOperationResult { Success = false, Message = message, ErrorCode = errorCode };
        }
    }

    /// <summary>
    /// Servicio para manejar sistema de amigos
    /// </summary>
    public class FriendService : MonoBehaviour
    {
        private static FriendService _instance;
        public static FriendService Instance
        {
            get
            {
                // C-67: Use implicit bool to detect destroyed Unity objects (== null is overloaded)
                if (!_instance)
                {
                    _instance = FindObjectOfType<FriendService>();
                    if (!_instance)
                    {
                        GameObject go = new GameObject("FriendService");
                        DontDestroyOnLoad(go);
                        // Set _instance BEFORE AddComponent to prevent Awake race condition
                        _instance = go.AddComponent<FriendService>();
                    }
                }
                return _instance;
            }
        }

        // Almacenamiento local (simula Firebase)
        private const string FRIEND_REQUESTS_KEY = "FriendRequests";
        private List<FriendRequest> _allRequests = new List<FriendRequest>();

        // Eventos
        public event Action<FriendRequest> OnFriendRequestReceived;
        public event Action<FriendRequest> OnFriendRequestAccepted;
        public event Action<FriendRequest> OnFriendRequestRejected;
        public event Action<string> OnFriendRemoved;
        public event Action OnFriendsListChanged;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadRequestsLocal(); // Immediate local load
                _ = LoadRequestsAsync(); // Async Firebase load (overwrites if available)
                Debug.Log("[FriendService] Inicializado");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private async Task LoadRequestsAsync()
        {
            // Try Firebase first
            try
            {
                var playerData = AuthenticationService.Instance?.GetCurrentPlayerData();
                if (playerData != null && DatabaseService.Instance != null)
                {
                    var firebaseRequests = await DatabaseService.Instance.LoadFriendRequests(playerData.userId);
                    if (firebaseRequests != null && firebaseRequests.Count > 0)
                    {
                        _allRequests = firebaseRequests;
                        SaveRequestsLocal(); // Update local cache
                        Debug.Log($"[FriendService] Cargadas {_allRequests.Count} solicitudes desde Firebase");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FriendService] Firebase load failed, using local: {e.Message}");
            }

            // Fallback: local data already loaded in LoadRequestsLocal()
        }

        private void LoadRequestsLocal()
        {
            if (PlayerPrefs.HasKey(FRIEND_REQUESTS_KEY))
            {
                string json = PlayerPrefs.GetString(FRIEND_REQUESTS_KEY);
                var wrapper = JsonUtility.FromJson<FriendRequestsWrapper>(json);
                if (wrapper?.requests != null)
                {
                    _allRequests = wrapper.requests;
                }
            }
            Debug.Log($"[FriendService] Cargadas {_allRequests.Count} solicitudes locales");
        }

        private void SaveRequests()
        {
            SaveRequestsLocal();
            // Firebase sync happens per-operation in Send/Accept/Reject/Cancel
        }

        private void SaveRequestsLocal()
        {
            var wrapper = new FriendRequestsWrapper { requests = _allRequests };
            PlayerPrefs.SetString(FRIEND_REQUESTS_KEY, JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
        }

        #region Friend Requests

        /// <summary>
        /// Envía una solicitud de amistad
        /// </summary>
        public async Task<FriendOperationResult> SendFriendRequest(string receiverId)
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_not_authenticated"), "NOT_AUTHENTICATED");
            }

            string senderId = currentUser.userId;

            // Validaciones
            if (senderId == receiverId)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_friend_self_request"), "SELF_REQUEST");
            }

            if (currentUser.IsFriend(receiverId))
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_already_friends"), "ALREADY_FRIENDS");
            }

            // Verificar si ya existe una solicitud pendiente
            var existingRequest = _allRequests.Find(r =>
                r.status == FriendRequestStatus.Pending &&
                ((r.senderId == senderId && r.receiverId == receiverId) ||
                 (r.senderId == receiverId && r.receiverId == senderId)));

            if (existingRequest != null)
            {
                // Si el otro usuario ya nos envió solicitud, aceptarla automáticamente
                if (existingRequest.senderId == receiverId)
                {
                    return await AcceptFriendRequest(existingRequest.requestId);
                }
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_pending"), "REQUEST_EXISTS");
            }

            // Obtener datos del receptor
            if (DatabaseService.Instance == null) { Debug.LogWarning("[FriendService] DatabaseService not available"); return FriendOperationResult.Failed(AutoLocalizer.Get("error_not_authenticated"), "SERVICE_UNAVAILABLE"); }
            var receiverData = await DatabaseService.Instance.GetPlayerDataById(receiverId);
            string receiverUsername = receiverData?.username ?? AutoLocalizer.Get("default_player_name");

            // Crear solicitud
            var request = new FriendRequest(
                senderId,
                currentUser.username,
                receiverId,
                receiverUsername
            );
            request.senderAvatarUrl = currentUser.avatarUrl;

            _allRequests.Add(request);
            SaveRequests();

            // Sync to Firebase
            if (DatabaseService.Instance != null)
            {
                _ = DatabaseService.Instance.SaveFriendRequest(request);
            }

            Debug.Log($"[FriendService] Solicitud enviada a {receiverUsername}");

            // Analytics
            AnalyticsService.Instance?.LogFriendRequestSent();

#if UNITY_EDITOR
            await Task.Delay(100); // Simular latencia de red
#endif

            OnFriendRequestReceived?.Invoke(request);

            return FriendOperationResult.Successful(AutoLocalizer.Get("friend_request_sent", receiverUsername));
        }

        /// <summary>
        /// Acepta una solicitud de amistad
        /// </summary>
        public async Task<FriendOperationResult> AcceptFriendRequest(string requestId)
        {
            var request = _allRequests.Find(r => r.requestId == requestId);
            if (request == null)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_not_found"), "NOT_FOUND");
            }

            if (request.status != FriendRequestStatus.Pending)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_already_responded"), "ALREADY_RESPONDED");
            }

            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null || currentUser.userId != request.receiverId)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_unauthorized_accept"), "UNAUTHORIZED");
            }

            // Actualizar estado de solicitud
            request.status = FriendRequestStatus.Accepted;
            request.SetRespondedAt(DateTime.Now);

            // Agregar amigo a la lista del usuario actual
            if (!currentUser.friends.Contains(request.senderId))
            {
                currentUser.friends.Add(request.senderId);
            }

            // Agregar al usuario actual a la lista del remitente
            if (DatabaseService.Instance == null) { Debug.LogWarning("[FriendService] DatabaseService not available"); return FriendOperationResult.Failed(AutoLocalizer.Get("error_not_authenticated"), "SERVICE_UNAVAILABLE"); }
            var senderData = await DatabaseService.Instance.GetPlayerDataById(request.senderId);
            if (senderData != null && !senderData.friends.Contains(currentUser.userId))
            {
                senderData.friends.Add(currentUser.userId);
                await DatabaseService.Instance.SavePlayerData(senderData);
            }

            // Guardar datos del usuario actual
            await DatabaseService.Instance.SavePlayerData(currentUser);
            SaveRequests();

            // Sync status to Firebase
            if (DatabaseService.Instance != null)
            {
                _ = DatabaseService.Instance.UpdateFriendRequestStatus(request);
            }

            Debug.Log($"[FriendService] Solicitud de {request.senderUsername} aceptada");

            // Analytics
            AnalyticsService.Instance?.LogFriendAdded();

            OnFriendRequestAccepted?.Invoke(request);
            OnFriendsListChanged?.Invoke();

            return FriendOperationResult.Successful(AutoLocalizer.Get("now_friends_with", request.senderUsername));
        }

        /// <summary>
        /// Rechaza una solicitud de amistad
        /// </summary>
        public async Task<FriendOperationResult> RejectFriendRequest(string requestId)
        {
            var request = _allRequests.Find(r => r.requestId == requestId);
            if (request == null)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_not_found"), "NOT_FOUND");
            }

            if (request.status != FriendRequestStatus.Pending)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_already_responded"), "ALREADY_RESPONDED");
            }

            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null || currentUser.userId != request.receiverId)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_unauthorized_reject"), "UNAUTHORIZED");
            }

            request.status = FriendRequestStatus.Rejected;
            request.SetRespondedAt(DateTime.Now);
            SaveRequests();

            // Sync status to Firebase
            if (DatabaseService.Instance != null)
            {
                _ = DatabaseService.Instance.UpdateFriendRequestStatus(request);
            }

#if UNITY_EDITOR
            await Task.Delay(50); // Simular latencia de red
#endif

            Debug.Log($"[FriendService] Solicitud de {request.senderUsername} rechazada");

            OnFriendRequestRejected?.Invoke(request);

            return FriendOperationResult.Successful(AutoLocalizer.Get("friend_request_rejected"));
        }

        /// <summary>
        /// Cancela una solicitud enviada
        /// </summary>
        public async Task<FriendOperationResult> CancelFriendRequest(string requestId)
        {
            var request = _allRequests.Find(r => r.requestId == requestId);
            if (request == null)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_not_found"), "NOT_FOUND");
            }

            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null || currentUser.userId != request.senderId)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_unauthorized_cancel"), "UNAUTHORIZED");
            }

            if (request.status != FriendRequestStatus.Pending)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_request_already_responded"), "ALREADY_RESPONDED");
            }

            request.status = FriendRequestStatus.Cancelled;
            request.SetRespondedAt(DateTime.Now);
            SaveRequests();

            // Delete from Firebase (cancelled requests don't need to persist)
            if (DatabaseService.Instance != null)
            {
                _ = DatabaseService.Instance.DeleteFriendRequest(request);
            }

#if UNITY_EDITOR
            await Task.Delay(50); // Simular latencia de red
#endif

            Debug.Log($"[FriendService] Solicitud a {request.receiverUsername} cancelada");

            return FriendOperationResult.Successful(AutoLocalizer.Get("friend_request_cancelled"));
        }

        /// <summary>
        /// Obtiene las solicitudes pendientes recibidas
        /// </summary>
        public async Task<List<FriendRequest>> GetPendingReceivedRequests()
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null)
            {
                return new List<FriendRequest>();
            }

#if UNITY_EDITOR
            await Task.Delay(50); // Simular latencia de red
#endif

            return _allRequests.FindAll(r =>
                r.receiverId == currentUser.userId &&
                r.status == FriendRequestStatus.Pending
            );
        }

        /// <summary>
        /// Obtiene las solicitudes pendientes enviadas
        /// </summary>
        public async Task<List<FriendRequest>> GetPendingSentRequests()
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null)
            {
                return new List<FriendRequest>();
            }

#if UNITY_EDITOR
            await Task.Delay(50); // Simular latencia de red
#endif

            return _allRequests.FindAll(r =>
                r.senderId == currentUser.userId &&
                r.status == FriendRequestStatus.Pending
            );
        }

        /// <summary>
        /// Obtiene el número de solicitudes pendientes
        /// </summary>
        public int GetPendingRequestsCount()
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null) return 0;

            return _allRequests.FindAll(r =>
                r.receiverId == currentUser.userId &&
                r.status == FriendRequestStatus.Pending
            ).Count;
        }

        /// <summary>
        /// Verifica si ya existe una solicitud pendiente con un jugador
        /// </summary>
        public bool HasPendingRequestWith(string playerId)
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null) return false;

            return _allRequests.Exists(r =>
                r.status == FriendRequestStatus.Pending &&
                ((r.senderId == currentUser.userId && r.receiverId == playerId) ||
                 (r.senderId == playerId && r.receiverId == currentUser.userId))
            );
        }

        /// <summary>
        /// Verifica si enviamos una solicitud a este jugador
        /// </summary>
        public bool HasSentRequestTo(string playerId)
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null) return false;

            return _allRequests.Exists(r =>
                r.status == FriendRequestStatus.Pending &&
                r.senderId == currentUser.userId &&
                r.receiverId == playerId
            );
        }

        #endregion

        #region Friends List

        /// <summary>
        /// Elimina un amigo
        /// </summary>
        public async Task<FriendOperationResult> RemoveFriend(string friendId)
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null)
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_not_authenticated"), "NOT_AUTHENTICATED");
            }

            if (!currentUser.friends.Contains(friendId))
            {
                return FriendOperationResult.Failed(AutoLocalizer.Get("error_not_friends"), "NOT_FRIENDS");
            }

            // Eliminar de la lista del usuario actual
            currentUser.friends.Remove(friendId);

            // Eliminar de la lista del otro usuario
            if (DatabaseService.Instance == null) { Debug.LogWarning("[FriendService] DatabaseService not available"); return FriendOperationResult.Failed(AutoLocalizer.Get("error_not_authenticated"), "SERVICE_UNAVAILABLE"); }
            var friendData = await DatabaseService.Instance.GetPlayerDataById(friendId);
            if (friendData != null)
            {
                friendData.friends.Remove(currentUser.userId);
                await DatabaseService.Instance.SavePlayerData(friendData);
            }

            // Guardar cambios
            await DatabaseService.Instance.SavePlayerData(currentUser);

            Debug.Log($"[FriendService] Amigo {friendId} eliminado");

            OnFriendRemoved?.Invoke(friendId);
            OnFriendsListChanged?.Invoke();

            return FriendOperationResult.Successful(AutoLocalizer.Get("friend_removed"));
        }

        /// <summary>
        /// Obtiene la lista de amigos con información completa
        /// </summary>
        public async Task<List<FriendInfo>> GetFriendsList()
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null)
            {
                return new List<FriendInfo>();
            }

            var friendsList = new List<FriendInfo>();

            foreach (var friendId in currentUser.friends)
            {
                if (DatabaseService.Instance == null) { Debug.LogWarning("[FriendService] DatabaseService not available"); break; }
                var friendData = await DatabaseService.Instance.GetPlayerDataById(friendId);
                if (friendData != null)
                {
                    bool online = await IsPlayerOnline(friendId);
                    friendsList.Add(new FriendInfo
                    {
                        odId = friendId,
                        username = friendData.username,
                        avatarUrl = friendData.avatarUrl,
                        isOnline = online,
                        winRate = friendData.GetWinRate(),
                        favoriteGame = GetFavoriteGame(friendData)
                    });
                }
            }

            return friendsList;
        }

        /// <summary>
        /// Obtiene el número de amigos
        /// </summary>
        public int GetFriendsCount()
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null) return 0;
            return currentUser.friends?.Count ?? 0;
        }

        /// <summary>
        /// Verifica si un jugador es amigo
        /// </summary>
        public bool IsFriend(string playerId)
        {
            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            return currentUser?.IsFriend(playerId) ?? false;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Verifica si un jugador está online usando Firebase lastLoginDate.
        /// Considera online si estuvo activo en los últimos 5 minutos.
        /// </summary>
        private async Task<bool> IsPlayerOnline(string userId)
        {
            if (DatabaseService.Instance == null) return false;
            try
            {
                var playerData = await DatabaseService.Instance.GetPlayerDataById(userId);
                if (playerData != null)
                {
                    var lastSeen = playerData.lastLoginDate;
                    // Consider online if active in last 5 minutes
                    return (DateTime.UtcNow - lastSeen).TotalMinutes < 5;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FriendService] Error checking online status: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Obtiene el juego favorito de un jugador
        /// </summary>
        private string GetFavoriteGame(PlayerData playerData)
        {
            if (playerData == null) return "DigitRush";

            var games = new Dictionary<string, int>
            {
                { "DigitRush", playerData.digitRushStats?.gamesPlayed ?? 0 },
                { "MemoryPairs", playerData.memoryPairsStats?.gamesPlayed ?? 0 },
                { "QuickMath", playerData.quickMathStats?.gamesPlayed ?? 0 },
                { "FlashTap", playerData.flashTapStats?.gamesPlayed ?? 0 },
                { "OddOneOut", playerData.oddOneOutStats?.gamesPlayed ?? 0 }
            };

            string favorite = "DigitRush";
            int maxPlayed = 0;

            foreach (var kvp in games)
            {
                if (kvp.Value > maxPlayed)
                {
                    maxPlayed = kvp.Value;
                    favorite = kvp.Key;
                }
            }

            return favorite;
        }

        #endregion
    }
}
