using CairoPaymentEngine.Application.Service;
using CairoPaymentEngine.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CairoPaymentEngine.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentOrchestrator _orchestrator;

        public PaymentsController(PaymentOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        [HttpPost("{orderId}")]
        public async Task<IActionResult> CreatePayment(Guid orderId)
        {
            var externalId = await _orchestrator
                .CreatePaymentAsync(orderId, PaymentGateway.Stripe);

            return Ok(externalId);
        }
    }
}
