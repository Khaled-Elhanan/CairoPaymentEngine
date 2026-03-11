using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Application.DTOs.Requests;
using CairoPaymentEngine.Application.DTOs.Responses;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using CairoPaymentEngine.Domain.Exceptioins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Application.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly PaymentOrchestrator _orchestrator;
        public PaymentService(
           IOrderRepository orderRepository,
           PaymentOrchestrator orchestrator)
        {
            _orderRepository = orderRepository;
            _orchestrator = orchestrator;
        }
        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var order = new Order(request.Amount, request.Currency);
            await _orderRepository.AddAsync(order);

            return new CreateOrderResponse(
                order.Id,
                order.Amount,
                order.Currency,
                order.Status.ToString());
        }
        public async Task<InitiatePaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request)
        {
            if (!Enum.TryParse<PaymentGateway>(request.Gateway, ignoreCase: true, out var gatewayType))
                throw new GatewayNotSupportedException(request.Gateway);

            var externalId = await _orchestrator.CreatePaymentAsync(request.OrderId, gatewayType);

            return new InitiatePaymentResponse(
                request.OrderId,
                externalId,
                request.Gateway,
                "Payment initiated. Awaiting gateway confirmation.");
        }
        public async Task<WebhookResponse> HandleWebhookAsync(WebhookRequest request)
        {
            if (!Enum.TryParse<PaymentGateway>(request.Gateway, ignoreCase: true, out var gatewayType))
                throw new GatewayNotSupportedException(request.Gateway);

            await _orchestrator.HandlePaymentSuccessAsync(
                request.ExternalId,
                request.EventId,
                gatewayType);

            return new WebhookResponse(
                request.ExternalId,
                "Payment processed successfully.");
        }
    } 
    }
