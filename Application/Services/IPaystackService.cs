using Application.Common.Dtos;

namespace Application.Services
{
    public interface IPaystackService
    {
        Task<Result<PaystackInitializeResponse>> InitializePaymentAsync(
            string email, decimal amount, string reference);

        Task<Result<PaystackVerifyResponse>> VerifyPaymentAsync(string reference);

        Task<Result<PaystackAccountResponse>> VerifyAccountNumberAsync(
            string accountNumber, string bankCode);

        Task<Result<string>> CreateTransferRecipientAsync(
            string accountName, string accountNumber, string bankCode);

        Task<Result<PaystackTransferResponse>> InitiateTransferAsync(
            string recipientCode, decimal amount, string reference, string reason);

        Task<Result<List<PaystackBank>>> GetBanksAsync();
    }
}