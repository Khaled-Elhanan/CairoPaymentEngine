using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using CairoPaymentEngine.Domain.Exceptioins;

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
        public async Task<(string ExternalId, string? PaymentUrl)> CreatePaymentAsync(Guid orderId, PaymentGateway gatewayType)
        {
            var order = await _orderRepository.GetByIdAsync(orderId)
                ?? throw new OrderNotFoundException(orderId);

            if (order.Status != OrderStatus.Pending)
                throw new OrderNotPayableException(orderId);

            var gateway = _gateways.FirstOrDefault(g => g.GatewayType == gatewayType)
                ?? throw new GatewayNotSupportedException(gatewayType.ToString());

            var (externalId, idempotencyKey, paymentUrl) = await gateway.CreatePaymentAsync(order);

            var payment = new Payment(order.Id, gatewayType, externalId, idempotencyKey);
            await _paymentRepository.AddAsync(payment);

            return (externalId, paymentUrl);
        }

        public async Task HandlePaymentSuccessAsync(
            string externalId,
            string eventId,
            PaymentGateway gatewayType)
        {
            var payment = await _paymentRepository.GetByExternalIdAsync(externalId)
                ?? throw new PaymentNotFoundException(externalId);

            // Already processed — idempotency guard
            if (payment.ProcessedEventId == eventId)
                return;

            var gateway = _gateways.FirstOrDefault(g => g.GatewayType == gatewayType)
                ?? throw new GatewayNotSupportedException(gatewayType.ToString());

            var verified = await gateway.VerifyPaymentAsync(externalId, eventId);
            if (!verified)
                throw new PaymentVerificationFailedException(externalId);

            payment.MarkSucceeded(eventId);

            var order = await _orderRepository.GetByIdAsync(payment.OrderId)
                ?? throw new OrderNotFoundException(payment.OrderId);

            order.MarkAsPaid();

            await _paymentRepository.UpdateAsync(payment);
            await _orderRepository.UpdateAsync(order);
        }
    }  
}
