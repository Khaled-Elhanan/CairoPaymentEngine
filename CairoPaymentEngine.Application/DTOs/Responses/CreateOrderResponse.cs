using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Application.DTOs.Responses
{
    public record CreateOrderResponse(
        Guid OrderId,
        decimal Amount,
        string Currency,
        string Status);
}
