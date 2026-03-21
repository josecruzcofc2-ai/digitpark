using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DigitPark.Payments
{
    /// <summary>
    /// Protocolo de emergencia si Stripe cae o hay una violación de compliance.
    /// Ejecuta switch inmediato a Apple IAP y notifica al backend.
    /// </summary>
    public static class StripeAbortProtocol
    {
        private static bool _abortExecuted = false;

        public static async void ExecuteAbort(AbortReason reason)
        {
            if (_abortExecuted)
            {
                Debug.Log("[AbortProtocol] Ya ejecutado previamente");
                return;
            }

            try
            {
                Debug.LogError($"[AbortProtocol] EJECUTANDO ABORT: {reason}");
                _abortExecuted = true;

                // Paso 1: Switch INMEDIATO a AppleIAP (local, sin depender de red)
                PaymentFeatureFlag.ForceSwitch(PaymentProvider.AppleIAP, reason.ToString());
                Debug.Log("[AbortProtocol] Paso 1: Switch a Apple IAP completado");

                // Paso 2: Notificar backend (fuera de await para no bloquear)
                _ = NotifyBackend(reason);

                // Paso 3: Log analytics
                PaymentBridge.LogCustomEvent?.Invoke(
                    "stripe_abort_executed",
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "reason", reason.ToString() },
                        { "timestamp", System.DateTime.UtcNow.ToString("O") }
                    });
                Debug.Log("[AbortProtocol] Paso 3: Analytics logueado");

                // Paso 4: Verificar Apple IAP
                await Task.Delay(500);
                var iapProvider = PaymentManager.Instance?.GetIAPProvider();
                if (iapProvider != null)
                {
                    bool iapHealthy = await iapProvider.HealthCheck();
                    if (!iapHealthy)
                    {
                        Debug.LogError("[AbortProtocol] Apple IAP TAMBIÉN caído. Tienda deshabilitada.");
                        PaymentFeatureFlag.ForceSwitch(PaymentProvider.None, "both_providers_down");
                    }
                    else
                    {
                        Debug.Log("[AbortProtocol] Apple IAP saludable. Continuando operación.");
                    }
                }

                // Paso 5: Notificar a UIs activas
                PaymentEvents.EmitAbortExecuted(reason);

                Debug.Log($"[AbortProtocol] Abort completado. Razón: {reason}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AbortProtocol] Error crítico durante abort: {e.Message}");
            }
        }

        public static void Reset()
        {
            _abortExecuted = false;
            Debug.Log("[AbortProtocol] Reset completado");
        }

        private static async Task NotifyBackend(AbortReason reason)
        {
            var paymentManager = PaymentManager.Instance;
            if (paymentManager == null) return;

            // Usar la config del PaymentManager para obtener la URL del Cloud Function
            try
            {
                string url = paymentManager.Config?.adminForceSwitchUrl;
                if (string.IsNullOrEmpty(url)) return;
                var body = new AbortNotification
                {
                    provider = "apple_iap",
                    reason = reason.ToString(),
                    timestamp = System.DateTime.UtcNow.ToString("O")
                };

                string json = UnityEngine.JsonUtility.ToJson(body);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                // Obtain Firebase ID token for auth
                string idToken = null;
                if (PaymentBridge.GetFirebaseIdToken != null)
                {
                    try { idToken = await PaymentBridge.GetFirebaseIdToken(); }
                    catch { }
                }

                using (var request = new UnityWebRequest(url, "POST"))
                {
                    request.uploadHandler = new UploadHandlerRaw(bytes);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    if (!string.IsNullOrEmpty(idToken))
                        request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                    request.timeout = 5;

                    var op = request.SendWebRequest();
                    float elapsed = 0f;
                    while (!op.isDone && elapsed < 5f)
                    {
                        await Task.Yield();
                        elapsed += 0.1f;
                    }

                    Debug.Log($"[AbortProtocol] Backend notificado: {request.result}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AbortProtocol] No se pudo notificar backend: {e.Message}");
            }
        }

        [System.Serializable]
        private class AbortNotification
        {
            public string provider;
            public string reason;
            public string timestamp;
        }
    }
}
