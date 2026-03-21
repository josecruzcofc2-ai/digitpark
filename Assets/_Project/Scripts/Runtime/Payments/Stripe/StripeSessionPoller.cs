using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DigitPark.Payments.Stripe
{
#if HAS_STRIPE || UNITY_EDITOR
    public enum StripeSessionStatus
    {
        Pending,
        Completed,
        Expired,
        Canceled,
        Error
    }

    /// <summary>
    /// Hace polling al backend para verificar el estado de una sesión de Stripe Checkout.
    /// Usa coroutines de Unity para compatibilidad con iOS.
    /// </summary>
    public class StripeSessionPoller : MonoBehaviour
    {
        private static StripeSessionPoller _instance;
        // Track active TCS so OnDestroy can resolve them to avoid hanging Tasks
        private TaskCompletionSource<StripeSessionStatus> _activeTcs;

        public static StripeSessionPoller GetOrCreate()
        {
            if (_instance == null)
            {
                var go = new GameObject("StripeSessionPoller");
                _instance = go.AddComponent<StripeSessionPoller>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }

        private void OnDestroy()
        {
            // If the poller is destroyed while waiting, resolve the TCS to avoid hanging Tasks
            _activeTcs?.TrySetResult(StripeSessionStatus.Error);
            _activeTcs = null;
            if (_instance == this) _instance = null;
        }

        public async Task<StripeSessionStatus> PollUntilComplete(
            string sessionId,
            string backendUrl,
            float timeoutMinutes = 5f,
            float intervalSeconds = 2f)
        {
            var tcs = new TaskCompletionSource<StripeSessionStatus>();
            _activeTcs = tcs;
            StartCoroutine(PollCoroutine(sessionId, backendUrl, timeoutMinutes, intervalSeconds, tcs));
            var result = await tcs.Task;
            _activeTcs = null;
            return result;
        }

        private string _cachedIdToken;

        private IEnumerator PollCoroutine(
            string sessionId,
            string backendUrl,
            float timeoutMinutes,
            float intervalSeconds,
            TaskCompletionSource<StripeSessionStatus> tcs)
        {
            float elapsed = 0f;
            float timeout = timeoutMinutes * 60f;
            string url = $"{backendUrl}?sessionId={sessionId}";

            // Obtain Firebase ID token once before polling loop
            if (PaymentBridge.GetFirebaseIdToken != null)
            {
                var tokenTask = PaymentBridge.GetFirebaseIdToken();
                while (!tokenTask.IsCompleted) yield return null;
                try { _cachedIdToken = tokenTask.Result; }
                catch { _cachedIdToken = null; }
            }

            Debug.Log($"[StripePoller] Iniciando polling para sesión: {sessionId}");

            while (elapsed < timeout)
            {
                using (var request = UnityWebRequest.Get(url))
                {
                    request.SetRequestHeader("X-App-Version", Compliance.VersionGuard.GetRequiredAppVersionHeader());
                    if (!string.IsNullOrEmpty(_cachedIdToken))
                        request.SetRequestHeader("Authorization", $"Bearer {_cachedIdToken}");
                    request.timeout = 10;

                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var response = JsonUtility.FromJson<SessionStatusResponse>(request.downloadHandler.text);
                        if (response != null)
                        {
                            Debug.Log($"[StripePoller] Status: {response.status}");
                            switch (response.status?.ToLower())
                            {
                                case "complete":
                                case "completed":
                                    tcs.TrySetResult(StripeSessionStatus.Completed);
                                    yield break;
                                case "expired":
                                    tcs.TrySetResult(StripeSessionStatus.Expired);
                                    yield break;
                                case "canceled":
                                case "cancelled":
                                    tcs.TrySetResult(StripeSessionStatus.Canceled);
                                    yield break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[StripePoller] Error de red: {request.error}");
                    }
                }

                yield return new WaitForSeconds(intervalSeconds);
                elapsed += intervalSeconds;
            }

            Debug.LogWarning("[StripePoller] Timeout alcanzado");
            tcs.TrySetResult(StripeSessionStatus.Expired);
        }

        [System.Serializable]
        private class SessionStatusResponse
        {
            public string status;
            public string sessionId;
            public string error;
        }
    }
#endif
}
