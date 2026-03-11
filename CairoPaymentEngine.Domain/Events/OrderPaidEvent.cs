using CairoPaymentEngine.Domain.Common;

namespace CairoPaymentEngine.Domain.Events
{
    public class OrderPaidEvent : DomainEvent
    {
        public Guid OrderId { get; }

        public OrderPaidEvent(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}
