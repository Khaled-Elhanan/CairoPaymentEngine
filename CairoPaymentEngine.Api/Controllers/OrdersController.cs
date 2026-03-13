using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Application.DTOs.Requests;
using CairoPaymentEngine.Application.DTOs.Responses;
using CairoPaymentEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CairoPaymentEngine.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public OrdersController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var response = await _paymentService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(CreateOrder), new { id = response.OrderId }, response);
        }
        [HttpPost("{id}/pay")]
        [ProducesResponseType(typeof(InitiatePaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitiatePayment(Guid id, [FromBody] InitiatePaymentRequest request)
        {
            var response = await _paymentService.InitiatePaymentAsync(request with { OrderId = id });
            return Ok(response);
        }

    }
}
