namespace CairoPaymentEngine.Infrastructure.Settings
{
    public class StripeSettings
    {
        public const string SectionName = "Gateways:Stripe";

        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
   
}
