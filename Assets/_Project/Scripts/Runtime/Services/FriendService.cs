using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DigitPark.Data;
using DigitPark.Services.Firebase;

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
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FriendService>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("FriendService");
                        _instance = go.AddComponent<FriendService>();
                        DontDestroyOnLoad(go);
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
                LoadRequests();
                Debug.Log("[FriendService] Inicializado");
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void LoadRequests()
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
            Debug.Log($"[FriendService] Cargadas {_allRequests.Count} solicitudes");
        }

        private void SaveRequests()
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
                return FriendOperationResult.Failed("No hay usuario autenticado", "NOT_AUTHENTICATED");
            }

            string senderId = currentUser.userId;

            // Validaciones
            if (senderId == receiverId)
            {
                return FriendOperationResult.Failed("No puedes enviarte una solicitud a ti mismo", "SELF_REQUEST");
            }

            if (currentUser.IsFriend(receiverId))
            {
                return FriendOperationResult.Failed("Este jugador ya es tu amigo", "ALREADY_FRIENDS");
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
                return FriendOperationResult.Failed("Ya existe una solicitud pendiente", "REQUEST_EXISTS");
            }

            // Obtener datos del receptor
            var receiverData = await DatabaseService.Instance?.GetPlayerDataById(receiverId);
            string receiverUsername = receiverData?.username ?? "Jugador";

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

            Debug.Log($"[FriendService] Solicitud enviada a {receiverUsername}");

            // Analytics
            AnalyticsService.Instance?.LogFriendRequestSent();

            // Simular delay de red
            await Task.Delay(100);

            OnFriendRequestReceived?.Invoke(request);

            return FriendOperationResult.Successful($"Solicitud enviada a {receiverUsername}");
        }

        /// <summary>
        /// Acepta una solicitud de amistad
        /// </summary>
        public async Task<FriendOperationResult> AcceptFriendRequest(string requestId)
        {
            var request = _allRequests.Find(r => r.requestId == requestId);
            if (request == null)
            {
                return FriendOperationResult.Failed("Solicitud no encontrada", "NOT_FOUND");
            }

            if (request.status != FriendRequestStatus.Pending)
            {
                return FriendOperationResult.Failed("Esta solicitud ya fue respondida", "ALREADY_RESPONDED");
            }

            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null || currentUser.userId != request.receiverId)
            {
                return FriendOperationResult.Failed("No tienes permiso para aceptar esta solicitud", "UNAUTHORIZED");
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
            var senderData = await DatabaseService.Instance?.GetPlayerDataById(request.senderId);
            if (senderData != null && !senderData.friends.Contains(currentUser.userId))
            {
                senderData.friends.Add(currentUser.userId);
                await DatabaseService.Instance?.SavePlayerData(senderData);
            }

            // Guardar datos del usuario actual
            await DatabaseService.Instance?.SavePlayerData(currentUser);
            SaveRequests();

            Debug.Log($"[FriendService] Solicitud de {request.senderUsername} aceptada");

            // Analytics
            AnalyticsService.Instance?.LogFriendAdded();

            OnFriendRequestAccepted?.Invoke(request);
            OnFriendsListChanged?.Invoke();

            return FriendOperationResult.Successful($"Ahora eres amigo de {request.senderUsername}");
        }

        /// <summary>
        /// Rechaza una solicitud de amistad
        /// </summary>
        public async Task<FriendOperationResult> RejectFriendRequest(string requestId)
        {
            var request = _allRequests.Find(r => r.requestId == requestId);
            if (request == null)
            {
                return FriendOperationResult.Failed("Solicitud no encontrada", "NOT_FOUND");
            }

            if (request.status != FriendRequestStatus.Pending)
            {
                return FriendOperationResult.Failed("Esta solicitud ya fue respondida", "ALREADY_RESPONDED");
            }

            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null || currentUser.userId != request.receiverId)
            {
                return FriendOperationResult.Failed("No tienes permiso para rechazar esta solicitud", "UNAUTHORIZED");
            }

            request.status = FriendRequestStatus.Rejected;
            request.SetRespondedAt(DateTime.Now);
            SaveRequests();

            await Task.Delay(50);

            Debug.Log($"[FriendService] Solicitud de {request.senderUsername} rechazada");

            OnFriendRequestRejected?.Invoke(request);

            return FriendOperationResult.Successful("Solicitud rechazada");
        }

        /// <summary>
        /// Cancela una solicitud enviada
        /// </summary>
        public async Task<FriendOperationResult> CancelFriendRequest(string requestId)
        {
            var request = _allRequests.Find(r => r.requestId == requestId);
            if (request == null)
            {
                return FriendOperationResult.Failed("Solicitud no encontrada", "NOT_FOUND");
            }

            var currentUser = AuthenticationService.Instance?.GetCurrentPlayerData();
            if (currentUser == null || currentUser.userId != request.senderId)
            {
                return FriendOperationResult.Failed("No tienes permiso para cancelar esta solicitud", "UNAUTHORIZED");
            }

            if (request.status != FriendRequestStatus.Pending)
            {
                return FriendOperationResult.Failed("Esta solicitud ya fue respondida", "ALREADY_RESPONDED");
            }

            request.status = FriendRequestStatus.Cancelled;
            request.SetRespondedAt(DateTime.Now);
            SaveRequests();

            await Task.Delay(50);

            Debug.Log($"[FriendService] Solicitud a {request.receiverUsername} cancelada");

            return FriendOperationResult.Successful("Solicitud cancelada");
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

            await Task.Delay(50);

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

            await Task.Delay(50);

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
                return FriendOperationResult.Failed("No hay usuario autenticado", "NOT_AUTHENTICATED");
            }

            if (!currentUser.friends.Contains(friendId))
            {
                return FriendOperationResult.Failed("Este jugador no es tu amigo", "NOT_FRIENDS");
            }

            // Eliminar de la lista del usuario actual
            currentUser.friends.Remove(friendId);

            // Eliminar de la lista del otro usuario
            var friendData = await DatabaseService.Instance?.GetPlayerDataById(friendId);
            if (friendData != null)
            {
                friendData.friends.Remove(currentUser.userId);
                await DatabaseService.Instance?.SavePlayerData(friendData);
            }

            // Guardar cambios
            await DatabaseService.Instance?.SavePlayerData(currentUser);

            Debug.Log($"[FriendService] Amigo {friendId} eliminado");

            OnFriendRemoved?.Invoke(friendId);
            OnFriendsListChanged?.Invoke();

            return FriendOperationResult.Successful("Amigo eliminado");
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
                var friendData = await DatabaseService.Instance?.GetPlayerDataById(friendId);
                if (friendData != null)
                {
                    friendsList.Add(new FriendInfo
                    {
                        odId = friendId,
                        username = friendData.username,
                        avatarUrl = friendData.avatarUrl,
                        isOnline = IsPlayerOnline(friendId),
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
        /// Verifica si un jugador está online (simulado)
        /// </summary>
        private bool IsPlayerOnline(string playerId)
        {
            // En producción esto vendría de Firebase Presence
            // Por ahora simulamos con un 30% de probabilidad
            return UnityEngine.Random.value < 0.3f;
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
