using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Infrastructure.Settings
{
    public class PaymobSettings
    {
        public const string SectionName = "Gateways:Paymob";

        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string IntegrationId { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string HmacSecret { get; set; } = string.Empty;
    }
}

