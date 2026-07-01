using Application.Common.Dtos;

namespace Application.Services
{
    public interface IPaystackService
    {
        // Fund wallet
        Task<PaystackInitializeResponse> InitializePaymentAsync(string email, decimal amount, string reference);
        Task<PaystackVerifyResponse> VerifyPaymentAsync(string reference);

        // Withdrawal
        Task<PaystackAccountResponse> VerifyAccountNumberAsync(string accountNumber, string bankCode);
        Task<string> CreateTransferRecipientAsync(string accountName, string accountNumber, string bankCode);
        Task<PaystackTransferResponse> InitiateTransferAsync(string recipientCode, decimal amount, string reference, string reason);

        // Banks list
        Task<List<PaystackBank>> GetBanksAsync();
    }
}
