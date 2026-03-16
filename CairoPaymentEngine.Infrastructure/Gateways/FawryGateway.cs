using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Infrastructure.Gateways
{
    public class FawryGateway : IPaymentGateway
    {
        public PaymentGateway GatewayType => PaymentGateway.Fawry;

        public Task<(string ExternalId, string IdempotencyKey)> CreatePaymentAsync(Order order)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyPaymentAsync(string externalId, string eventId)
        {
            throw new NotImplementedException();
        }
    }
}
