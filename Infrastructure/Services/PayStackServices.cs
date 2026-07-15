using Application.Common.Dtos;
using Application.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Services
{
    public class PaystackException : Exception
    {
        public PaystackException(string message, Exception? inner = null)
            : base(message, inner) { }
    }

    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _settings;
        private readonly ILogger<PaystackService> _logger;

        public PaystackService(
            HttpClient httpClient,
            IOptions<PaystackSettings> settings,
            ILogger<PaystackService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {_settings.SecretKey}");
        }

        // ─── FUND WALLET ─────────────────────────────────────────────

        public async Task<PaystackInitializeResponse> InitializePaymentAsync(
            string email, decimal amount, string reference)
        {
            var payload = new
            {
                email,
                amount = (int)(amount * 100),
                reference,
                callback_url = _settings.CallbackUrl  // ← from settings now
            };

            var response = await PostAsync(
                $"{_settings.BaseUrl}/transaction/initialize", payload);

            return new PaystackInitializeResponse
            {
                Status = response["status"]!.Value<bool>(),
                Message = response["message"]!.Value<string>()!,
                AuthorizationUrl = response["data"]!["authorization_url"]!
                    .Value<string>()!,
                Reference = response["data"]!["reference"]!.Value<string>()!
            };
        }

        public async Task<PaystackVerifyResponse> VerifyPaymentAsync(string reference)
        {
            var response = await GetAsync(
                $"{_settings.BaseUrl}/transaction/verify/{reference}");

            return new PaystackVerifyResponse
            {
                Status = response["status"]!.Value<bool>(),
                PaymentStatus = response["data"]!["status"]!.Value<string>()!,
                Amount = response["data"]!["amount"]!.Value<decimal>() / 100,
                Reference = reference
            };
        }

        // ─── WITHDRAWAL ──────────────────────────────────────────────

        public async Task<PaystackAccountResponse> VerifyAccountNumberAsync(
            string accountNumber, string bankCode)
        {
            var response = await GetAsync(
                $"{_settings.BaseUrl}/bank/resolve?account_number={accountNumber}&bank_code={bankCode}");

            return new PaystackAccountResponse
            {
                Status = response["status"]!.Value<bool>(),
                AccountName = response["data"]!["account_name"]!.Value<string>()!,
                AccountNumber = response["data"]!["account_number"]!.Value<string>()!
            };
        }

        public async Task<string> CreateTransferRecipientAsync(
            string accountName, string accountNumber, string bankCode)
        {
            var payload = new
            {
                type = "nuban",
                name = accountName,
                account_number = accountNumber,
                bank_code = bankCode,
                currency = "NGN"
            };

            var response = await PostAsync(
                $"{_settings.BaseUrl}/transferrecipient", payload);

            return response["data"]!["recipient_code"]!.Value<string>()!;
        }

        public async Task<PaystackTransferResponse> InitiateTransferAsync(
            string recipientCode, decimal amount,
            string reference, string reason)
        {
            var payload = new
            {
                source = "balance",
                amount = (int)(amount * 100),
                reference,
                recipient = recipientCode,
                reason
            };

            var response = await PostAsync(
                $"{_settings.BaseUrl}/transfer", payload);

            return new PaystackTransferResponse
            {
                Status = response["status"]!.Value<bool>(),
                TransferStatus = response["data"]!["status"]!.Value<string>()!,
                Reference = reference
            };
        }

        public async Task<List<PaystackBank>> GetBanksAsync()
        {
            var response = await GetAsync(
                $"{_settings.BaseUrl}/bank?currency=NGN");

            var banks = response["data"]!.ToObject<List<JObject>>()!;

            return banks.Select(b => new PaystackBank
            {
                Name = b["name"]!.Value<string>()!,
                Code = b["code"]!.Value<string>()!
            }).ToList();
        }

        // ─── HELPERS ─────────────────────────────────────────────────

        private async Task<JObject> PostAsync(string url, object payload)
        {
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsync(url,
                    new StringContent(
                        JsonConvert.SerializeObject(payload),
                        System.Text.Encoding.UTF8,
                        "application/json"));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling Paystack POST {Url}", url);
                throw new PaystackException(
                    "Unable to reach the payment service. Please check your connection and try again.", ex);
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout calling Paystack POST {Url}", url);
                throw new PaystackException(
                    "The payment service timed out. Please try again.", ex);
            }

            return await ParseResponseAsync(response, url);
        }

        private async Task<JObject> GetAsync(string url)
        {
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling Paystack GET {Url}", url);
                throw new PaystackException(
                    "Unable to reach the payment service. Please check your connection and try again.", ex);
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout calling Paystack GET {Url}", url);
                throw new PaystackException(
                    "The payment service timed out. Please try again.", ex);
            }

            return await ParseResponseAsync(response, url);
        }

        private async Task<JObject> ParseResponseAsync(HttpResponseMessage response, string url)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Paystack returned {StatusCode} for {Url}. Body: {Content}",
                    response.StatusCode, url, content);
            }

            try
            {
                return JObject.Parse(content);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Invalid JSON from Paystack at {Url}. Body: {Content}", url, content);
                throw new PaystackException(
                    "Received an unexpected response from the payment service.", ex);
            }
        }
    }
}