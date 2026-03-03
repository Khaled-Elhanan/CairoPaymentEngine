using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CairoPaymentEngine.Infrastructure.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly CairoPaymentDbContext _context;

        public PaymentRepository(CairoPaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByExternalIdAsync(string externalId)
        {
            return await _context.Payments
            .FirstOrDefaultAsync(p => p.ExternalId == externalId);
        }
        public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    
}
}
