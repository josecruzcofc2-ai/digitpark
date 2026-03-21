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
                    _instance = FindFirstObjectByType<EntitlementService>();
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

        /// <summary>
        /// Call after login/logout to refresh the userId and reload entitlements.
        /// Wire this to AuthenticationService.OnLoginSuccess / OnLogout.
        /// </summary>
        public void RefreshUserId()
        {
            string newId = GetCurrentUserId();
            if (newId != _currentUserId)
            {
                _currentUserId = newId;
                LoadFromLocal();
                Debug.Log($"[EntitlementService] UserId refreshed to {_currentUserId}, {_localEntitlements.Count} entitlements loaded.");
            }
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
        /// GET {getEntitlementsUrl}?userId={userId}
        /// Merge: los entitlements del servidor que no están en local se añaden (nunca se eliminan locales).
        /// Si el backend no responde, se opera con la cache local (fail-open).
        /// </summary>
        public async Task SyncWithServer()
        {
            Debug.Log("[EntitlementService] Sincronizando entitlements con servidor...");

            var config = PaymentManager.Instance?.Config;
            if (config == null || string.IsNullOrEmpty(config.getEntitlementsUrl))
            {
                Debug.LogWarning("[EntitlementService] SyncWithServer: getEntitlementsUrl no configurada. Usando cache local.");
                return;
            }

            // Obtener Firebase ID token para Authorization header
            string idToken = null;
            if (PaymentBridge.GetFirebaseIdToken != null)
            {
                try { idToken = await PaymentBridge.GetFirebaseIdToken(); }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[EntitlementService] No se pudo obtener ID token: {e.Message}");
                }
            }

            string url = $"{config.getEntitlementsUrl}?userId={_currentUserId}";

            using (var request = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(idToken))
                    request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                request.timeout = 10;

                var op = request.SendWebRequest();
                while (!op.isDone) await Task.Yield();
                if (this == null) return;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    MergeServerEntitlements(request.downloadHandler.text);
                    Debug.Log("[EntitlementService] Sync completado desde servidor.");
                }
                else
                {
                    // Fail-open: operar con cache local, reintentar en la próxima sesión
                    Debug.LogWarning($"[EntitlementService] SyncWithServer falló ({request.error}). Usando cache local.");
                }
            }
        }

        /// <summary>
        /// Merge de entitlements del servidor en la lista local.
        /// Añade los que faltan, nunca elimina los locales (evitar pérdida por race condition).
        /// </summary>
        private void MergeServerEntitlements(string responseJson)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<EntitlementListWrapper>(
                    "{\"items\":" + responseJson + "}");
                if (wrapper?.items == null) return;

                int added = 0;
                foreach (var serverRecord in wrapper.items)
                {
                    bool exists = _localEntitlements.Exists(e =>
                        e.productId == serverRecord.productId &&
                        e.transactionId == serverRecord.transactionId);
                    if (!exists)
                    {
                        _localEntitlements.Add(serverRecord);
                        added++;
                    }
                }

                if (added > 0)
                {
                    SaveToLocal();
                    Debug.Log($"[EntitlementService] Merge: {added} entitlement(s) restaurados desde servidor.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EntitlementService] Error parseando respuesta del servidor: {e.Message}");
            }
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
