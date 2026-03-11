using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Application.DTOs.Responses
{
    public record WebhookResponse(
        string ExternalId,
        string Message);
}
