namespace DigitPark.Payments
{
    public enum AbortReason
    {
        StripeCheckoutFailure,
        StripeWebhookTimeout,
        StripeRemoteDisabled,
        ManualDeveloperTrigger,
        CrossContamination,
        StripeAccountSuspended,
        ComplianceViolation
    }
}
