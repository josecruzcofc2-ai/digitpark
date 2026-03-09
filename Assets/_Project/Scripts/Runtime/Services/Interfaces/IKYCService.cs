using System;
using System.Threading.Tasks;

namespace DigitPark.Services
{
    /// <summary>
    /// Estado de verificación KYC del usuario
    /// </summary>
    public enum KYCStatus
    {
        NotStarted,         // No ha iniciado verificación
        AgeVerified,        // Solo verificó edad (fecha nacimiento)
        DocumentPending,    // Documento enviado, esperando revisión
        DocumentVerified,   // Documento verificado
        FullyVerified,      // Completamente verificado (puede usar Cash Battle)
        Rejected,           // Verificación rechazada
        Expired             // Verificación expirada, debe renovar
    }

    /// <summary>
    /// Resultado de una operación de verificación
    /// </summary>
    public class KYCResult
    {
        public bool Success { get; set; }
        public KYCStatus Status { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public static KYCResult Successful(KYCStatus status, string message = null)
        {
            return new KYCResult { Success = true, Status = status, Message = message };
        }

        public static KYCResult Failed(string message, string errorCode = null)
        {
            return new KYCResult { Success = false, Message = message, ErrorCode = errorCode };
        }
    }

    /// <summary>
    /// Información del usuario verificado
    /// </summary>
    public class UserVerificationInfo
    {
        public string UserId { get; set; }
        public KYCStatus Status { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? Age { get; set; }
        public bool IsOver18 => Age.HasValue && Age.Value >= 18;
        public bool CanAccessCashBattle => Status == KYCStatus.FullyVerified;
        public DateTime? VerificationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    /// <summary>
    /// Servicio de verificación KYC (Know Your Customer)
    /// Maneja verificación de edad e identidad para Cash Battle
    /// </summary>
    public interface IKYCService
    {
        /// <summary>
        /// Estado actual de verificación
        /// </summary>
        KYCStatus CurrentStatus { get; }

        /// <summary>
        /// Si el usuario está completamente verificado para Cash Battle
        /// </summary>
        bool IsFullyVerified { get; }

        /// <summary>
        /// Si el usuario puede acceder a funciones de Cash Battle
        /// </summary>
        bool CanAccessCashBattle { get; }

        /// <summary>
        /// Información del usuario verificado
        /// </summary>
        UserVerificationInfo UserInfo { get; }

        /// <summary>
        /// Evento cuando cambia el estado de verificación
        /// </summary>
        event Action<KYCStatus> OnStatusChanged;

        /// <summary>
        /// Verifica la edad del usuario con fecha de nacimiento
        /// Este es el primer paso de verificación
        /// </summary>
        Task<KYCResult> VerifyAge(DateTime birthDate);

        /// <summary>
        /// Inicia el proceso de verificación de identidad completo
        /// Abre el flujo de Triumph/proveedor KYC (documento + selfie)
        /// </summary>
        Task<KYCResult> StartIdentityVerification();

        /// <summary>
        /// Obtiene el estado actual de verificación del servidor
        /// </summary>
        Task<KYCResult> RefreshVerificationStatus();

        /// <summary>
        /// Resetea el estado de verificación (para testing/desarrollo)
        /// </summary>
        Task<KYCResult> ResetVerification();
    }
}
