using CairoPaymentEngine.Domain.Enums;

namespace CairoPaymentEngine.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }

        public Guid OrderId { get; private set; }

        public PaymentGateway Gateway { get; private set; }

        public string ExternalId { get; private set; }

        public string IdempotencyKey { get; private set; }

        public PaymentStatus Status { get; private set; }

        public string? ProcessedEventId { get; private set; }

        public Order Order { get; private set; } = null!;

        private Payment() { }

        public Payment(
            Guid orderId,
            PaymentGateway gateway,
            string externalId,
            string idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("ExternalId is required.");

            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new ArgumentException("IdempotencyKey is required.");

            Id = Guid.NewGuid();
            OrderId = orderId;
            Gateway = gateway;
            ExternalId = externalId;
            IdempotencyKey = idempotencyKey;
            Status = PaymentStatus.Pending;
        }

        public void MarkSucceeded(string eventId)
        {
            if (ProcessedEventId == eventId)
                return; 

            if (Status == PaymentStatus.Succeeded)
                return;

            Status = PaymentStatus.Succeeded;
            ProcessedEventId = eventId;
        }

        public void MarkFailed()
        {
            if (Status == PaymentStatus.Succeeded)
                throw new InvalidOperationException("Cannot fail a succeeded payment.");

            Status = PaymentStatus.Failed;
        }
    }
    }
