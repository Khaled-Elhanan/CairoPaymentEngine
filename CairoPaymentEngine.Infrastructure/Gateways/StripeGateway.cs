using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using CairoPaymentEngine.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Globalization;

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



        public async Task<(string ExternalId, string IdempotencyKey, string? PaymentUrl)> CreatePaymentAsync(Order order)
        {
            var idempotencyKey = $"order-{order.Id}-{DateTime.UtcNow.Ticks}";

            // Stripe accepts amount in smallest currency unit and enforces a hard max.
            var amountInSmallestUnit = (long)decimal.Round(order.Amount * 100m, 0, MidpointRounding.AwayFromZero);
            if (amountInSmallestUnit > 99_999_999)
                throw new ArgumentException("Stripe amount must be <= 999999.99 for USD.");
            if (amountInSmallestUnit <= 0)
                throw new ArgumentException("Stripe amount must be greater than zero.");

            var formDate = new Dictionary<string, string>
            {
                ["amount"] = amountInSmallestUnit.ToString(CultureInfo.InvariantCulture),
                ["currency"] = order.Currency,
                ["metadata[order_id]"] = order.Id.ToString(),
                ["automatic_payment_methods[enabled]"] = "true",
                ["automatic_payment_methods[allow_redirects]"] = "never",
                // For local simulation, auto-confirm with a Stripe test payment method
                // so webhook verification can transition the order to Paid.
                ["payment_method"] = "pm_card_visa",
                ["confirm"] = "true"

            };
            var request = new HttpRequestMessage(HttpMethod.Post, "v1/payment_intents")
            {
                Content = new FormUrlEncodedContent(formDate)
            };
            request.Headers.Add("Idempotency-Key", idempotencyKey);
            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException($"Stripe CreatePayment failed: {ExtractStripeMessage(body)}");
            var json = JsonDocument.Parse(body);
            var paymentIntentId = json.RootElement.GetProperty("id").GetString()
               ?? throw new Exception("Stripe response missing payment intent ID.");

            return (paymentIntentId, idempotencyKey, null);
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

        private static string ExtractStripeMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var errorElement) &&
                    errorElement.TryGetProperty("message", out var messageElement))
                {
                    var message = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                        return message;
                }
            }
            catch
            {
                // Fallback to raw body below when payload is not JSON.
            }

            return body;
        }
    }
}
