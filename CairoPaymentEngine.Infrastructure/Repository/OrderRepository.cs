using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CairoPaymentEngine.Infrastructure.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CairoPaymentDbContext _context;
        public OrderRepository(CairoPaymentDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
