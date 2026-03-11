using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Domain.Exceptioins
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        
        }
    }
    // Order Exceptions 
    public class OrderNotFoundException : DomainException
    {
        public OrderNotFoundException(Guid orderId) 
            : base($"Order with id {orderId} not found.")
        {
        }
    }
    public class OrderNotPayableException : DomainException
    {
        public OrderNotPayableException(Guid orderId)
            : base($"Order '{orderId}' is not in a payable state.") { }
    }
    // Payment Exceptions
    public class PaymentNotFoundException : DomainException
    {
        public PaymentNotFoundException(string externalId)
            : base($"Payment with ExternalId '{externalId}' was not found.") { }
    }

    public class PaymentVerificationFailedException : DomainException
    {
        public PaymentVerificationFailedException(string externalId)
            : base($"Payment verification failed for ExternalId '{externalId}'.") { }
    }

    // Payment Gateway Exceptions
    public class GatewayNotSupportedException : DomainException
    {
        public GatewayNotSupportedException(string gateway)
            : base($"Payment gateway '{gateway}' is not supported.") { }
    }
}
