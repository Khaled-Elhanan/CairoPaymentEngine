using CairoPaymentEngine.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CairoPaymentEngine.Domain.Entities
{
    public class Order  : BaseEntity
    {
        public Guid Id { get; private set; }

        public decimal Amount { get; private set; }

        public string Currency { get; private set; }

        public OrderStatus Status { get; private set; }

        private Order() { } 

        public Order(decimal amount, string currency)
        {
            if (amount <= 0)
                throw new ArgumentException("Order amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.");

            Id = Guid.NewGuid();
            Amount = amount;
            Currency = currency.ToLower();
            Status = OrderStatus.Pending;
        }

        public void MarkAsPaid()
        {
            if (Status == OrderStatus.Paid)
                throw new InvalidOperationException("Order already paid.");

            if (Status == OrderStatus.Refunded)
                throw new InvalidOperationException("Refunded order cannot be paid.");

            Status = OrderStatus.Paid;
            AddDomainEvent(new OrderPaidEvent(Id));

        }

        public void MarkAsFailed()
        {
            if (Status == OrderStatus.Paid)
                throw new InvalidOperationException("Paid order cannot be marked as failed.");

            Status = OrderStatus.Failed;
        }

        public void MarkAsRefunded()
        {
            if (Status != OrderStatus.Paid)
                throw new InvalidOperationException("Only paid orders can be refunded.");

            Status = OrderStatus.Refunded;
        }
    }
}
