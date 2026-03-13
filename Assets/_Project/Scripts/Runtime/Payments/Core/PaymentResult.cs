namespace DigitPark.Payments
{
    /// <summary>
    /// Resultado de una operación de pago cosmético.
    /// Nunca contiene datos de Triumph.
    /// </summary>
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string ProductId { get; set; }
        public PaymentProvider ProviderUsed { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public System.DateTime Timestamp { get; set; }
        public bool WasProviderSwitched { get; set; }

        public static PaymentResult Successful(string productId, string transactionId,
            PaymentProvider provider, bool wasSwitched = false)
        {
            return new PaymentResult
            {
                Success = true,
                ProductId = productId,
                TransactionId = transactionId,
                ProviderUsed = provider,
                Timestamp = System.DateTime.UtcNow,
                WasProviderSwitched = wasSwitched
            };
        }

        public static PaymentResult Failed(string productId, string errorCode,
            string errorMessage, PaymentProvider provider)
        {
            return new PaymentResult
            {
                Success = false,
                ProductId = productId,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                ProviderUsed = provider,
                Timestamp = System.DateTime.UtcNow
            };
        }
    }
}
