using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using CairoPaymentEngine.Domain.Events;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Tests.Domain
{
    public class OrderTests
    {

        [Fact]
        public void Constructor_ValidInputs_CreatesOrderWithPendingStatus()
        {
            var order = new Order(100m, "EGP");

            order.Amount.Should().Be(100m);
            order.Currency.Should().Be("egp");
            order.Status.Should().Be(OrderStatus.Pending);
            order.Id.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Constructor_InvalidAmount_ThrowsArgumentException(decimal amount)
        {
            var act = () => new Order(amount, "EGP");

            act.Should().Throw<ArgumentException>()
               .WithMessage("*amount*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Constructor_EmptyCurrency_ThrowsArgumentException(string? currency)
        {
            var act = () => new Order(100m, currency!);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*currency*");
        }

        //  MarkAsPaid 

        [Fact]
        public void MarkAsPaid_PendingOrder_ChangesStatusAndRaisesDomainEvent()
        {
            var order = new Order(100m, "EGP");

            order.MarkAsPaid();

            order.Status.Should().Be(OrderStatus.Paid);
            order.DomainEvents.Should().ContainSingle()
                 .Which.Should().BeOfType<OrderPaidEvent>();
        }

        [Fact]
        public void MarkAsPaid_AlreadyPaidOrder_ThrowsInvalidOperationException()
        {
            var order = new Order(100m, "EGP");
            order.MarkAsPaid();

            var act = () => order.MarkAsPaid();

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*already paid*");
        }

        [Fact]
        public void MarkAsPaid_RefundedOrder_ThrowsInvalidOperationException()
        {
            var order = new Order(100m, "EGP");
            order.MarkAsPaid();
            order.MarkAsRefunded();

            var act = () => order.MarkAsPaid();

            act.Should().Throw<InvalidOperationException>();
        }

        // MarkAsFailed 

        [Fact]
        public void MarkAsFailed_PendingOrder_ChangesStatusToFailed()
        {
            var order = new Order(100m, "EGP");

            order.MarkAsFailed();

            order.Status.Should().Be(OrderStatus.Failed);
        }

        [Fact]
        public void MarkAsFailed_PaidOrder_ThrowsInvalidOperationException()
        {
            var order = new Order(100m, "EGP");
            order.MarkAsPaid();

            var act = () => order.MarkAsFailed();

            act.Should().Throw<InvalidOperationException>();
        }

        // MarkAsRefunded

        [Fact]
        public void MarkAsRefunded_PaidOrder_ChangesStatusToRefunded()
        {
            var order = new Order(100m, "EGP");
            order.MarkAsPaid();

            order.MarkAsRefunded();

            order.Status.Should().Be(OrderStatus.Refunded);
        }

        [Fact]
        public void MarkAsRefunded_PendingOrder_ThrowsInvalidOperationException()
        {
            var order = new Order(100m, "EGP");

            var act = () => order.MarkAsRefunded();

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*Only paid orders*");
        }
    }
}
