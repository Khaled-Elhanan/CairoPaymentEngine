using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;

namespace CairoPaymentEngine.Infrastructure.Gateways
{
    public class FakeStripeGateway : IPaymentGateway
    {
        public PaymentGateway GatewayType => PaymentGateway.Stripe;

        public Task<(string ExternalId, string IdempotencyKey)> CreatePaymentAsync(Order order)
        {
            var externalId = $"pi_{Guid.NewGuid():N}";
            var idempotencyKey = Guid.NewGuid().ToString();

            return Task.FromResult((externalId, idempotencyKey));
        }

        public Task<bool> VerifyPaymentAsync(string externalId, string eventId)
        {
            throw new NotImplementedException();
        }
    }
}
