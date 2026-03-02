using System;
using System.Collections.Generic;
using System.Text;

namespace CairoPaymentEngine.Domain.Common
{
    public abstract class DomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
