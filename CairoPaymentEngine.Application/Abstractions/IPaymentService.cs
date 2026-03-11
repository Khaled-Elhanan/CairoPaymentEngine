using CairoPaymentEngine.Application.DTOs.Requests;
using CairoPaymentEngine.Application.DTOs.Responses;
using CairoPaymentEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Application.Abstractions
{
    public interface IPaymentService
    {
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<InitiatePaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request);
        Task<WebhookResponse> HandleWebhookAsync(WebhookRequest request);
    }
}
