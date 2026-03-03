using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;

namespace CairoPaymentEngine.Application.Service
{
    public class PaymentOrchestrator
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentOrchestrator(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IEnumerable<IPaymentGateway> gateways)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _gateways = gateways;
        }
        public async Task<string> CreatePaymentAsync(Guid orderId, PaymentGateway gatewayType)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new Exception("Order is not in a payable state");

            if (order.Status != OrderStatus.Pending)
                throw new Exception("Order is not in a payable state");

            var gateway = _gateways.FirstOrDefault(g => g.GatewayType == gatewayType)
            ?? throw new Exception("Gateway not supported");
            var (externalId, idempotencyKey) =
                await gateway.CreatePaymentAsync(order);
            var payment = new Payment(
            order.Id,
            gatewayType,
            externalId,
            idempotencyKey);

            await _paymentRepository.AddAsync(payment);

            return externalId;
        }

        public async Task HandlePaymentSuccessAsync(
            string externalId,
            string eventId,
            PaymentGateway gatewayType)
        {
            var payment = await _paymentRepository.GetByExternalIdAsync(externalId)
                ?? throw new Exception("Payment not found");

            var gateway = _gateways.First(g => g.GatewayType == gatewayType);

            var verified = await gateway.VerifyPaymentAsync(externalId, eventId);

            if (!verified)
                throw new Exception("Payment verification failed");

            payment.MarkSucceeded(eventId);

            var order = await _orderRepository.GetByIdAsync(payment.OrderId)
                ?? throw new Exception("Order not found");

            order.MarkAsPaid();

            await _paymentRepository.UpdateAsync(payment);
            await _orderRepository.UpdateAsync(order);
        }
    }  
}
