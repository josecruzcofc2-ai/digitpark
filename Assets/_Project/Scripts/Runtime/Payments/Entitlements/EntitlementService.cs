using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DigitPark.Payments.Entitlements
{
    /// <summary>
    /// Fuente de verdad de lo que el usuario posee (compras cosméticas).
    /// Almacena en PlayerPrefs (local) y sincroniza con Firebase + backend.
    /// </summary>
    public class EntitlementService : MonoBehaviour
    {
        private static EntitlementService _instance;
        public static EntitlementService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<EntitlementService>();
                return _instance;
            }
        }

        private const string PREFS_KEY_PREFIX = "dp_entitlements_";
        private List<EntitlementRecord> _localEntitlements = new List<EntitlementRecord>();
        private string _currentUserId;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _currentUserId = GetCurrentUserId();
            LoadFromLocal();
            Debug.Log($"[EntitlementService] Iniciado. {_localEntitlements.Count} entitlements cargados.");
        }

        private string GetCurrentUserId()
        {
            return PaymentBridge.GetCurrentUserId();
        }

        private string GetPrefsKey() => PREFS_KEY_PREFIX + _currentUserId;

        private void LoadFromLocal()
        {
            string json = PlayerPrefs.GetString(GetPrefsKey(), "[]");
            try
            {
                var wrapper = JsonUtility.FromJson<EntitlementListWrapper>("{\"items\":" + json + "}");
                _localEntitlements = wrapper?.items ?? new List<EntitlementRecord>();
            }
            catch
            {
                _localEntitlements = new List<EntitlementRecord>();
            }
        }

        private void SaveToLocal()
        {
            var jsonItems = new System.Text.StringBuilder("[");
            for (int i = 0; i < _localEntitlements.Count; i++)
            {
                jsonItems.Append(JsonUtility.ToJson(_localEntitlements[i]));
                if (i < _localEntitlements.Count - 1) jsonItems.Append(",");
            }
            jsonItems.Append("]");
            PlayerPrefs.SetString(GetPrefsKey(), jsonItems.ToString());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Otorga un entitlement tras compra exitosa.
        /// provider debe ser "stripe" o "apple_iap" — NUNCA "triumph".
        /// </summary>
        public async Task Grant(string productId, string provider, string transactionId)
        {
            // Validación de seguridad: nunca "triumph"
            if (provider.Contains("triumph"))
            {
                Debug.LogError("[EntitlementService] VIOLATION: Intentando otorgar entitlement con provider 'triumph'");
                return;
            }

            var record = new EntitlementRecord
            {
                userId = _currentUserId,
                productId = productId,
                provider = provider,
                transactionId = transactionId,
                grantedAt = System.DateTime.UtcNow.ToString("O"),
                appVersion = PaymentFeatureFlag.IsProVersion ? "pro" : "global",
                hasTournamentBenefit = false,
                isCosmetic = true
            };

            // Evitar duplicados
            bool exists = _localEntitlements.Exists(e =>
                e.productId == productId && e.transactionId == transactionId);
            if (!exists)
            {
                _localEntitlements.Add(record);
                SaveToLocal();
                Debug.Log($"[EntitlementService] Entitlement otorgado: {productId} via {provider}");
            }

            // Sincronizar con Firebase
            SyncToFirebase(record).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogWarning($"[EntitlementService] SyncToFirebase unhandled error: {t.Exception?.GetBaseException().Message}");
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
        }

        public bool HasEntitlement(string productId)
        {
            return _localEntitlements.Exists(e => e.productId == productId);
        }

        public List<EntitlementRecord> GetAllEntitlements()
            => new List<EntitlementRecord>(_localEntitlements);

        /// <summary>
        /// Sincroniza entitlements con Firebase Cloud Functions.
        /// Firebase Functions endpoints:
        ///   GET  https://us-central1-{project}.cloudfunctions.net/getEntitlements?userId={userId}
        ///   POST https://us-central1-{project}.cloudfunctions.net/syncEntitlements
        ///        Body: { userId, localEntitlements: EntitlementRecord[] }
        /// Configurar PaymentConfig.backendBaseUrl con la URL base del proyecto Firebase.
        /// </summary>
        public async Task SyncWithServer()
        {
            Debug.Log("[EntitlementService] Sincronizando entitlements con servidor...");
            // En produccion: GET {syncEntitlementsUrl}?userId={_currentUserId}
            // URL Firebase Functions: https://us-central1-{project}.cloudfunctions.net/getEntitlements
            // Por ahora solo recarga desde local
            LoadFromLocal();
            await Task.Yield();
            if (this == null) return;
            Debug.Log("[EntitlementService] Sync completado");
        }

        private async Task SyncToFirebase(EntitlementRecord record)
        {
            // Sincronizar con Firebase Database via delegate
            if (PaymentBridge.UpdatePlayerFields != null)
            {
                try
                {
                    var data = new Dictionary<string, object>
                    {
                        { "provider", record.provider },
                        { "transactionId", record.transactionId },
                        { "grantedAt", record.grantedAt },
                        { "appVersion", record.appVersion },
                        { "hasTournamentBenefit", false },
                        { "isCosmetic", true }
                    };
                    await PaymentBridge.UpdatePlayerFields(new Dictionary<string, object>
                    {
                        { $"entitlements/{record.productId}", data }
                    });
                    Debug.Log($"[EntitlementService] Sincronizado con Firebase: {record.productId}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[EntitlementService] Error sincronizando con Firebase: {e.Message}");
                }
            }
        }

        [System.Serializable]
        private class EntitlementListWrapper
        {
            public List<EntitlementRecord> items;
        }
    }
}
