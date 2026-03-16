using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Infrastructure.Settings
{
    public class StripeSettings
    {
        public const string SectionName = "Gateways:Stripe";

        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
    public class FawrySettings
    {
        public const string SectionName = "Gateways:Fawry";

        public string MerchantCode { get; set; } = string.Empty;
        public string SecurityKey { get; set; } = string.Empty;
        public bool Sandbox { get; set; } = true;
    }
}
