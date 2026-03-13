using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CairoPaymentEngine.Infrastructure.Gateways
{
    public class StripeGateway : IPaymentGateway

    {
        public PaymentGateway GatewayType => PaymentGateway.Stripe;

        private readonly HttpClient _httpClient;
        private readonly string _secretKey;   
        private readonly string _webhookSecret;

        public StripeGateway(HttpClient httpClient , IConfiguration configuration)
        {
          _httpClient= httpClient;
            _secretKey = configuration["Gateways:Stripe:SecretKey"]
                 ?? throw new InvalidOperationException("Stripe SecretKey is not configured.");
            _webhookSecret = configuration["Gateways:Stripe:WebhookSecret"]
                ?? throw new InvalidOperationException("Stripe WebhookSecret is not configured.");

            _httpClient.BaseAddress = new Uri("https://api.stripe.com/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _secretKey);
        }


        public async Task<(string ExternalId, string IdempotencyKey)> CreatePaymentAsync(Order order)
        {
            var idempotencyKey = $"order-{order.Id}-{DateTime.UtcNow.Ticks}";
            var amountInSamllestUnit =(long)(order.Amount * 100); 

            var formDate = new Dictionary<string, string>
            {
                { "amount", amountInSamllestUnit.ToString() },
                { "currency", order.Currency },
                { "description", $"Payment for Order {order.Id}" }
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "v1/payment_intents")
            {
                Content = new FormUrlEncodedContent(formDate)
            };
            request.Headers.Add("Idempotency-Key", idempotencyKey);
            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Stripe CreatePayment failed : {body}");
            var json = JsonDocument.Parse(body);
            var paymentIntentId = json.RootElement.GetProperty("id").GetString()
               ?? throw new Exception("Stripe response missing payment intent ID.");

            return (paymentIntentId, idempotencyKey);
        }

        public async  Task<bool> VerifyPaymentAsync(string externalId, string eventId)
        {
            var response = await _httpClient.GetAsync($"v1/payment_intents/{externalId}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return false;

            var json = JsonDocument.Parse(body);
            var status = json.RootElement.GetProperty("status").GetString();

            return status == "succeeded";
        }
    }
}
