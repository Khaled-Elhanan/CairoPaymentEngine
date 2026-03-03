using CairoPaymentEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CairoPaymentEngine.Application.Abstractions
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByExternalIdAsync(string externalId);
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
    }
}
