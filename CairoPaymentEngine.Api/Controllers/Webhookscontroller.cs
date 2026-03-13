using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Application.DTOs.Requests;
using CairoPaymentEngine.Application.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CairoPaymentEngine.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public WebhooksController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [HttpPost("{gateway}")]
        [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Handle(string gateway, [FromBody] WebhookRequest request)
        {
            var response = await _paymentService.HandleWebhookAsync(request with { Gateway = gateway });
            return Ok(response);
        }
    }
    }
