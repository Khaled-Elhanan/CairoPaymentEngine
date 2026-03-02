using CairoPaymentEngine.Domain.Common;
using CairoPaymentEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CairoPaymentEngine.Domain.Events
{
    public class OrderPaidEvent :DomainEvent
    {
        public OrderPaidEvent(Guid orderId)
        {
            OrderId = orderId;
        }
    }             

}
