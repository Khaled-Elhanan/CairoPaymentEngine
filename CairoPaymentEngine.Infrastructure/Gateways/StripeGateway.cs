using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using CairoPaymentEngine.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CairoPaymentEngine.Infrastructure.Gateways
{
    public class StripeGateway : IPaymentGateway

    {
        public PaymentGateway GatewayType => PaymentGateway.Stripe;

        private readonly HttpClient _httpClient;
        private readonly StripeSettings _settings;

        public StripeGateway(HttpClient httpClient, IOptions<StripeSettings> options)
        {
            _settings = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.stripe.com/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
        }



        public async Task<(string ExternalId, string IdempotencyKey)> CreatePaymentAsync(Order order)
        {
            var idempotencyKey = $"order-{order.Id}-{DateTime.UtcNow.Ticks}";
            var amountInSamllestUnit =(long)(order.Amount * 100); 

            var formDate = new Dictionary<string, string>
            {
                ["amount"] = amountInSamllestUnit.ToString(),
                ["currency"] = order.Currency,
                ["metadata[order_id]"] = order.Id.ToString(),
                ["automatic_payment_methods[enabled]"] = "true",
                ["automatic_payment_methods[allow_redirects]"] = "never"

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
