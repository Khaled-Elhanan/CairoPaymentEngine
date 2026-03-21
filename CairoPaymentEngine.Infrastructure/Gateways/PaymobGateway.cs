using CairoPaymentEngine.Application.Abstractions;
using CairoPaymentEngine.Domain.Entities;
using CairoPaymentEngine.Domain.Enums;
using CairoPaymentEngine.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CairoPaymentEngine.Infrastructure.Gateways
{
    public class PaymobGateway : IPaymentGateway
    {
        public PaymentGateway GatewayType => PaymentGateway.Paymob;

        private readonly HttpClient _httpClient;
        private readonly PaymobSettings _settings;

        public PaymobGateway(HttpClient httpClient, IOptions<PaymobSettings> options)
        {
            _settings = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://accept.paymob.com/");
            _httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<(string ExternalId, string IdempotencyKey)> CreatePaymentAsync(Order order)
        {
            var idempotencyKey = $"paymob-{order.Id:N}";
            var amountInCents = (long)(order.Amount * 100);

            // Step 1 — Authenticate
            var authToken = await AuthenticateAsync();

            // Step 2 — Create Order on Paymob
            var paymobOrderId = await CreatePaymobOrderAsync(authToken, order, amountInCents);

            // Step 3 — Get Payment Key (token)
            var paymentToken = await GetPaymentKeyAsync(authToken, paymobOrderId, amountInCents, order.Currency);

            return (paymentToken, idempotencyKey);
        }

        public async Task<bool> VerifyPaymentAsync(string externalId, string eventId)
        {
            // externalId = Paymob transaction ID (sent in webhook)
            // Verify by checking transaction status via Paymob API

            var authToken = await AuthenticateAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.GetAsync(
                $"api/acceptance/transactions/{eventId}",
                new CancellationTokenSource().Token);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return false;

            var json = JsonDocument.Parse(body);
            var success = json.RootElement.GetProperty("success").GetBoolean();

            return success;
        }

        // ── Step 1: Authenticate ──────────────────────────────

        private async Task<string> AuthenticateAsync()
        {
            var payload = JsonSerializer.Serialize(new { api_key = _settings.ApiKey });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/tokens", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Paymob authentication failed: {body}");

            var json = JsonDocument.Parse(body);
            return json.RootElement.GetProperty("token").GetString()
                ?? throw new Exception("Paymob auth response missing token.");
        }

        // ── Step 2: Create Order ──────────────────────────────

        private async Task<string> CreatePaymobOrderAsync(
            string authToken, Order order, long amountInCents)
        {
            var payload = JsonSerializer.Serialize(new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = amountInCents,
                currency = order.Currency.ToUpper(),
                merchant_order_id = order.Id.ToString(),
                items = Array.Empty<object>()
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/ecommerce/orders", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Paymob order creation failed: {body}");

            var json = JsonDocument.Parse(body);
            return json.RootElement.GetProperty("id").GetInt64().ToString()
                ?? throw new Exception("Paymob order response missing id.");
        }

        // ── Step 3: Get Payment Key ───────────────────────────

        private async Task<string> GetPaymentKeyAsync(
            string authToken, string paymobOrderId, long amountInCents, string currency)
        {
            var payload = JsonSerializer.Serialize(new
            {
                auth_token = authToken,
                amount_cents = amountInCents,
                expiration = 3600,
                order_id = paymobOrderId,
                currency = currency.ToUpper(),
                integration_id = int.Parse(_settings.IntegrationId),
                billing_data = new
                {
                    first_name = "Cairo",
                    last_name = "Payment",
                    email = "customer@example.com",
                    phone_number = "+201000000000",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "Cairo",
                    country = "EG",
                    state = "Cairo"
                }
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/acceptance/payment_keys", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Paymob payment key request failed: {body}");

            var json = JsonDocument.Parse(body);
            return json.RootElement.GetProperty("token").GetString()
                ?? throw new Exception("Paymob payment key response missing token.");
        }

        // ── HMAC Verification (for webhook security) ──────────

        public bool VerifyHmac(string hmacReceived, string concatenatedString)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
            var msgBytes = Encoding.UTF8.GetBytes(concatenatedString);
            var hash = new HMACSHA512(keyBytes).ComputeHash(msgBytes);
            var computed = Convert.ToHexString(hash).ToLower();
            return computed == hmacReceived.ToLower();
        }
    }
}
