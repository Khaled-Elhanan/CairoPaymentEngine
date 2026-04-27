using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Application.Service;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using CairoPaymentEngine.Domain.Exceptioins;
using FluentAssertions;
using Moq;

namespace CairoPaymentEngine.Tests.Application
{
    public class PaymentOrchestratorTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
        private readonly Mock<IPaymentGateway> _gatewayMock = new();
        private readonly PaymentOrchestrator _sut;

        public PaymentOrchestratorTests()
        {
            _gatewayMock.Setup(g => g.GatewayType).Returns(PaymentGateway.Stripe);

            _sut = new PaymentOrchestrator(
                _orderRepoMock.Object,
                _paymentRepoMock.Object,
                new[] { _gatewayMock.Object });
        }

        // CreatePaymentAsync 

        [Fact]
        public async Task CreatePaymentAsync_ValidOrder_CreatesPaymentAndReturnsExternalId()
        {
            var order = new Order(200m, "EGP");
            _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            _gatewayMock.Setup(g => g.CreatePaymentAsync(order))
                        .ReturnsAsync(("ext-123", "idem-key-123", (string?)null));

            var result = await _sut.CreatePaymentAsync(order.Id, PaymentGateway.Stripe);

            result.ExternalId.Should().Be("ext-123");
            result.PaymentUrl.Should().BeNull();
            _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
        }

        [Fact]
        public async Task CreatePaymentAsync_OrderNotFound_ThrowsOrderNotFoundException()
        {
            _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                          .ReturnsAsync((Order?)null);

            var act = () => _sut.CreatePaymentAsync(Guid.NewGuid(), PaymentGateway.Stripe);

            await act.Should().ThrowAsync<OrderNotFoundException>();
        }

        [Fact]
        public async Task CreatePaymentAsync_AlreadyPaidOrder_ThrowsOrderNotPayableException()
        {
            var order = new Order(200m, "EGP");
            order.MarkAsPaid();
            _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

            var act = () => _sut.CreatePaymentAsync(order.Id, PaymentGateway.Stripe);

            await act.Should().ThrowAsync<OrderNotPayableException>();
        }

        [Fact]
        public async Task CreatePaymentAsync_UnsupportedGateway_ThrowsGatewayNotSupportedException()
        {
            var order = new Order(200m, "EGP");
            _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

            var act = () => _sut.CreatePaymentAsync(order.Id, PaymentGateway.Paymob);

            await act.Should().ThrowAsync<GatewayNotSupportedException>();
        }
    
        // HandlePaymentSuccessAsync
 
        [Fact]
        public async Task HandlePaymentSuccessAsync_ValidPayment_MarksOrderAsPaid()
        {
            var order = new Order(200m, "EGP");
            var payment = new Payment(order.Id, PaymentGateway.Stripe, "ext-123", "idem-key");

            _paymentRepoMock.Setup(r => r.GetByExternalIdAsync("ext-123")).ReturnsAsync(payment);
            _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            _gatewayMock.Setup(g => g.VerifyPaymentAsync("ext-123", "evt-123")).ReturnsAsync(true);

            await _sut.HandlePaymentSuccessAsync("ext-123", "evt-123", PaymentGateway.Stripe);

            order.Status.Should().Be(OrderStatus.Paid);
            payment.Status.Should().Be(PaymentStatus.Succeeded);
            _paymentRepoMock.Verify(r => r.UpdateAsync(payment), Times.Once);
            _orderRepoMock.Verify(r => r.UpdateAsync(order), Times.Once);
        }

        [Fact]
        public async Task HandlePaymentSuccessAsync_SameEventId_IsIdempotent()
        {
            var order = new Order(200m, "EGP");
            var payment = new Payment(order.Id, PaymentGateway.Stripe, "ext-123", "idem-key");
            payment.MarkSucceeded("evt-123"); 

            _paymentRepoMock.Setup(r => r.GetByExternalIdAsync("ext-123")).ReturnsAsync(payment);

            await _sut.HandlePaymentSuccessAsync("ext-123", "evt-123", PaymentGateway.Stripe);

            
            _gatewayMock.Verify(
                g => g.VerifyPaymentAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            _orderRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<Order>()),
                Times.Never);
        }

        [Fact]
        public async Task HandlePaymentSuccessAsync_VerificationFailed_ThrowsPaymentVerificationFailedException()
        {
            var order = new Order(200m, "EGP");
            var payment = new Payment(order.Id, PaymentGateway.Stripe, "ext-123", "idem-key");

            _paymentRepoMock.Setup(r => r.GetByExternalIdAsync("ext-123")).ReturnsAsync(payment);
            _gatewayMock.Setup(g => g.VerifyPaymentAsync("ext-123", "evt-bad")).ReturnsAsync(false);

            var act = () => _sut.HandlePaymentSuccessAsync("ext-123", "evt-bad", PaymentGateway.Stripe);

            await act.Should().ThrowAsync<PaymentVerificationFailedException>();
        }
    }
}
