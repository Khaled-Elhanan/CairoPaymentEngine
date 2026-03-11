using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CairoPaymentEngine.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(decimal amount  , string currency)
        {
            var order = new Order(amount , currency);   
            await _orderRepository.AddAsync(order);
            return Ok(new { order.Id });
        }
    }
}
