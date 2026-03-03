using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
namespace CairoPaymentEngine.Application.Abstractions
{
    public interface IPaymentGateway
    {
        PaymentGateway GatewayType { get; }

        Task<(string ExternalId, string IdempotencyKey)>
         CreatePaymentAsync(Order order);

        Task<bool>VerifyPaymentAsync(string externalId, string eventId);

    }
}
