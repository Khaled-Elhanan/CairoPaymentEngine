using CairoPaymentEngine.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

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
