using System;
using System.Collections.Generic;
using DigitPark.Localization;

namespace DigitPark.Data
{
    /// <summary>
    /// Estado de una solicitud de amistad
    /// </summary>
    public enum FriendRequestStatus
    {
        Pending,    // Esperando respuesta
        Accepted,   // Aceptada
        Rejected,   // Rechazada
        Cancelled   // Cancelada por el remitente
    }

    /// <summary>
    /// Solicitud de amistad entre dos jugadores
    /// </summary>
    [Serializable]
    public class FriendRequest
    {
        public string requestId;
        public string senderId;
        public string senderUsername;
        public string senderAvatarUrl;
        public string receiverId;
        public string receiverUsername;
        public FriendRequestStatus status;
        public string createdAt;
        public string respondedAt;

        public FriendRequest()
        {
            requestId = Guid.NewGuid().ToString();
            status = FriendRequestStatus.Pending;
            createdAt = DateTime.Now.ToString("o");
        }

        public FriendRequest(string senderId, string senderUsername, string receiverId, string receiverUsername)
        {
            this.requestId = Guid.NewGuid().ToString();
            this.senderId = senderId;
            this.senderUsername = senderUsername;
            this.receiverId = receiverId;
            this.receiverUsername = receiverUsername;
            this.status = FriendRequestStatus.Pending;
            this.createdAt = DateTime.Now.ToString("o");
        }

        public DateTime GetCreatedAt()
        {
            if (DateTime.TryParse(createdAt, out DateTime result))
                return result;
            return DateTime.Now;
        }

        public DateTime GetRespondedAt()
        {
            if (DateTime.TryParse(respondedAt, out DateTime result))
                return result;
            return DateTime.MinValue;
        }

        public void SetRespondedAt(DateTime value)
        {
            respondedAt = value.ToString("o");
        }
    }

    /// <summary>
    /// Datos básicos de un amigo para mostrar en listas
    /// </summary>
    [Serializable]
    public class FriendInfo
    {
        public string odId;
        public string username;
        public string avatarUrl;
        public bool isOnline;
        public string lastSeen;
        public float winRate;
        public string favoriteGame;

        public DateTime GetLastSeen()
        {
            if (DateTime.TryParse(lastSeen, out DateTime result))
                return result;
            return DateTime.MinValue;
        }

        public void SetLastSeen(DateTime value)
        {
            lastSeen = value.ToString("o");
        }

        /// <summary>
        /// Obtiene texto amigable de última conexión
        /// </summary>
        public string GetLastSeenText()
        {
            if (isOnline) return AutoLocalizer.Get("status_online");

            var lastSeenDate = GetLastSeen();
            if (lastSeenDate == DateTime.MinValue) return AutoLocalizer.Get("status_unknown");

            var diff = DateTime.Now - lastSeenDate;

            if (diff.TotalMinutes < 1) return AutoLocalizer.Get("time_just_now");
            if (diff.TotalMinutes < 60) return AutoLocalizer.Get("time_ago_minutes", (int)diff.TotalMinutes);
            if (diff.TotalHours < 24) return AutoLocalizer.Get("time_ago_hours", (int)diff.TotalHours);
            if (diff.TotalDays < 7) return AutoLocalizer.Get("time_ago_days", (int)diff.TotalDays);

            return lastSeenDate.ToString("dd/MM/yyyy");
        }
    }

    /// <summary>
    /// Wrapper para serialización de lista de solicitudes
    /// </summary>
    [Serializable]
    public class FriendRequestsWrapper
    {
        public List<FriendRequest> requests = new List<FriendRequest>();
    }

    /// <summary>
    /// Wrapper para serialización de lista de amigos
    /// </summary>
    [Serializable]
    public class FriendsListWrapper
    {
        public List<string> friendIds = new List<string>();
    }
}
