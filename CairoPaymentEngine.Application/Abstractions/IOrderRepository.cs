using CairoPaymentEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CairoPaymentEngine.Application.Abstractions
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
