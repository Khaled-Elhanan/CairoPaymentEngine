using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Application.DTOs.Requests
{
    public record WebhookRequest(
         string ExternalId,
         string EventId,
         string Gateway);
}
