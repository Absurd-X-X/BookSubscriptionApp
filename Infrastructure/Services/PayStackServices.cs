using Application.Common.Dtos;
using Application.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Services
{
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

        public async Task<Result<PaystackInitializeResponse>> InitializePaymentAsync(
            string email, decimal amount, string reference)
        {
            var payload = new
            {
                email,
                amount = (int)(amount * 100),
                reference,
                callback_url = _settings.CallbackUrl
            };

            var (response, error) = await PostAsync(
                $"{_settings.BaseUrl}/transaction/initialize", payload);

            if (error != null)
                return Result<PaystackInitializeResponse>.Failure(error);

            try
            {
                var data = new PaystackInitializeResponse
                {
                    Status = response!["status"]!.Value<bool>(),
                    Message = response["message"]!.Value<string>()!,
                    AuthorizationUrl = response["data"]!["authorization_url"]!
                        .Value<string>()!,
                    Reference = response["data"]!["reference"]!.Value<string>()!
                };

                return Result<PaystackInitializeResponse>.Success(data, "Done!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Paystack initialize response");
                return Result<PaystackInitializeResponse>.Failure(
                    "Received an unexpected response from the payment service.");
            }
        }

        public async Task<Result<PaystackVerifyResponse>> VerifyPaymentAsync(string reference)
        {
            var (response, error) = await GetAsync(
                $"{_settings.BaseUrl}/transaction/verify/{reference}");

            if (error != null)
                return Result<PaystackVerifyResponse>.Failure(error);

            try
            {
                var data = new PaystackVerifyResponse
                {
                    Status = response!["status"]!.Value<bool>(),
                    PaymentStatus = response["data"]!["status"]!.Value<string>()!,
                    Amount = response["data"]!["amount"]!.Value<decimal>() / 100,
                    Reference = reference
                };

                return Result<PaystackVerifyResponse>.Success(data, "Done!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Paystack verify response");
                return Result<PaystackVerifyResponse>.Failure(
                    "Received an unexpected response from the payment service.");
            }
        }

        // ─── WITHDRAWAL ──────────────────────────────────────────────

        public async Task<Result<PaystackAccountResponse>> VerifyAccountNumberAsync(
            string accountNumber, string bankCode)
        {
            var (response, error) = await GetAsync(
                $"{_settings.BaseUrl}/bank/resolve?account_number={accountNumber}&bank_code={bankCode}");

            if (error != null)
                return Result<PaystackAccountResponse>.Failure(error);

            try
            {
                var data = new PaystackAccountResponse
                {
                    Status = response!["status"]!.Value<bool>(),
                    AccountName = response["data"]!["account_name"]!.Value<string>()!,
                    AccountNumber = response["data"]!["account_number"]!.Value<string>()!
                };

                return Result<PaystackAccountResponse>.Success(data, "Done!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Paystack account resolve response");
                return Result<PaystackAccountResponse>.Failure(
                    "Received an unexpected response from the payment service.");
            }
        }

        public async Task<Result<string>> CreateTransferRecipientAsync(
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

            var (response, error) = await PostAsync(
                $"{_settings.BaseUrl}/transferrecipient", payload);

            if (error != null)
                return Result<string>.Failure(error);

            try
            {
                var recipientCode = response!["data"]!["recipient_code"]!.Value<string>()!;
                return Result<string>.Success(recipientCode, "Done!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Paystack transfer recipient response");
                return Result<string>.Failure(
                    "Received an unexpected response from the payment service.");
            }
        }

        public async Task<Result<PaystackTransferResponse>> InitiateTransferAsync(
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

            var (response, error) = await PostAsync(
                $"{_settings.BaseUrl}/transfer", payload);

            if (error != null)
                return Result<PaystackTransferResponse>.Failure (error);

            try
            {
                var data = new PaystackTransferResponse
                {
                    Status = response!["status"]!.Value<bool>(),
                    TransferStatus = response["data"]!["status"]!.Value<string>()!,
                    Reference = reference
                };

                return Result<PaystackTransferResponse>.Success(data, "Done!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Paystack transfer response");
                return Result<PaystackTransferResponse>.Failure(
                    "Received an unexpected response from the payment service.");
            }
        }

        public async Task<Result<List<PaystackBank>>> GetBanksAsync()
        {
            var (response, error) = await GetAsync(
                $"{_settings.BaseUrl}/bank?currency=NGN");

            if (error != null)
                return Result<List<PaystackBank>>.Failure(error);

            try
            {
                var banks = response!["data"]!.ToObject<List<JObject>>()!;

                var data = banks.Select(b => new PaystackBank
                {
                    Name = b["name"]!.Value<string>()!,
                    Code = b["code"]!.Value<string>()!
                }).ToList();

                return Result<List<PaystackBank>>.Success(data, "Done!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Paystack banks response");
                return Result<List<PaystackBank>>.Failure(
                    "Received an unexpected response from the payment service.");
            }
        }

        // ─── HELPERS ─────────────────────────────────────────────────
        // Both helpers return (response, error). error == null means the
        // HTTP call and JSON parsing succeeded — callers should still
        // check response.IsSuccessStatusCode-equivalent business fields
        // (e.g. "status") inside the JSON if Paystack signals failure
        // there instead of via HTTP status.

        private async Task<(JObject? Response, string? Error)> PostAsync(string url, object payload)
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
                return (null, "Unable to reach the payment service. Please check your connection and try again.");
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout calling Paystack POST {Url}", url);
                return (null, "The payment service timed out. Please try again.");
            }

            return await ParseResponseAsync(response, url);
        }

        private async Task<(JObject? Response, string? Error)> GetAsync(string url)
        {
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling Paystack GET {Url}", url);
                return (null, "Unable to reach the payment service. Please check your connection and try again.");
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Timeout calling Paystack GET {Url}", url);
                return (null, "The payment service timed out. Please try again.");
            }

            return await ParseResponseAsync(response, url);
        }

        private async Task<(JObject? Response, string? Error)> ParseResponseAsync(HttpResponseMessage response, string url)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Paystack returned {StatusCode} for {Url}. Body: {Content}",
                    response.StatusCode, url, content);
            }

            JObject parsed;

            try
            {
                parsed = JObject.Parse(content);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Invalid JSON from Paystack at {Url}. Body: {Content}", url, content);
                return (null, "Received an unexpected response from the payment service.");
            }

            if (!response.IsSuccessStatusCode)
            {
                // Paystack error responses usually carry a "message" field —
                // surface it if present, otherwise fall back to a generic message.
                var message = parsed["message"]?.Value<string>();
                return (parsed, message ?? "The payment service returned an error. Please try again.");
            }

            return (parsed, null);
        }
    }
}