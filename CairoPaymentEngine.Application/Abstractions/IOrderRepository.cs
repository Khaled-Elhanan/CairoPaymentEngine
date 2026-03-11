using CairoPaymentEngine.Domain.Entities;
namespace CairoPaymentEngine.Application.Abstractions
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
