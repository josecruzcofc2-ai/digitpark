using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DigitPark.Payments.AppleIAP
{
    /// <summary>
    /// Validación server-side de receipts de Apple IAP contra el backend.
    /// Esta es una capa ADICIONAL sobre la validación client-side de PremiumManager.
    /// Si el backend no responde, la compra sigue siendo válida (ya se cobró).
    /// </summary>
    public class AppleReceiptValidator
    {
        public struct ReceiptValidationResult
        {
            public bool IsValid;
            public string TransactionId;
            public string ProductId;
            public string ErrorMessage;
        }

        public async Task<ReceiptValidationResult> ValidateReceipt(
            string receiptData, string productId, string userId, string backendUrl)
        {
            if (string.IsNullOrEmpty(receiptData))
            {
                Debug.LogError($"[AppleReceiptValidator] DENIED: receiptData is empty — cannot validate purchase without receipt.");
                return new ReceiptValidationResult
                {
                    IsValid = false,
                    ProductId = productId,
                    ErrorMessage = "No receipt data to validate"
                };
            }

            if (string.IsNullOrEmpty(backendUrl))
            {
                Debug.LogError($"[AppleReceiptValidator] DENIED: backendUrl not configured — server validation required. Fix PaymentConfig.");
                return new ReceiptValidationResult
                {
                    IsValid = false,
                    ProductId = productId,
                    ErrorMessage = "Backend URL not configured"
                };
            }

            // Firebase Functions URL: PaymentConfig.iapValidateReceiptUrl
            // e.g. https://us-central1-{project}.cloudfunctions.net/iapValidateReceipt
            string url = backendUrl;
            var body = new ValidationRequest
            {
                receiptData = receiptData,
                productId = productId,
                userId = userId,
                appVersion = "global",
                bundleId = Application.identifier
            };

            string json = JsonUtility.ToJson(body);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Obtain Firebase ID token for Authorization header
            string idToken = null;
            if (PaymentBridge.GetFirebaseIdToken != null)
            {
                try { idToken = await PaymentBridge.GetFirebaseIdToken(); }
                catch (System.Exception e) { Debug.LogWarning($"[AppleReceiptValidator] Failed to get ID token: {e.Message}"); }
            }

            using (var request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-App-Version",
                    Compliance.VersionGuard.GetRequiredAppVersionHeader());
                if (!string.IsNullOrEmpty(idToken))
                    request.SetRequestHeader("Authorization", $"Bearer {idToken}");
                request.timeout = 15;

                var op = request.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<ValidationResponse>(request.downloadHandler.text);
                    return new ReceiptValidationResult
                    {
                        IsValid = response?.valid ?? false,
                        TransactionId = response?.transactionId ?? "",
                        ProductId = productId
                    };
                }

                // Backend no responde: denegar y dejar que Unity IAP restaure la compra en la próxima sesión
                Debug.LogError($"[ReceiptValidator] Backend no respondió: {request.error}. Compra denegada hasta que el servidor esté disponible.");
                return new ReceiptValidationResult
                {
                    IsValid = false,
                    ProductId = productId,
                    ErrorMessage = $"Backend unreachable: {request.error}"
                };
            }
        }

        [System.Serializable]
        private class ValidationRequest
        {
            public string receiptData;
            public string productId;
            public string userId;
            public string appVersion;
            public string bundleId;
        }

        [System.Serializable]
        private class ValidationResponse
        {
            public bool valid;
            public string transactionId;
            public string error;
        }
    }
}
