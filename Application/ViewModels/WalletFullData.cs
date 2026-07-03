using Application.Common.Pagenation;
using Application.Queries;
using static Application.Queries.GetTransactions;
using static Application.Queries.GetWalletBalance;

namespace Application.ViewModels
{
    public class WalletFullData
    {
        public PagenatedList<GetTransactionsResponse> Transactions { get; set; } = default!;
        public PagenatedList<GetTransactionByTransactionStatusResponse> TransactionStatusDebit { get; set; } = default!;
        public PagenatedList<GetTransactionByTransactionStatusResponse> TransactionStatusCredit { get; set; } = default!;
        public PagenatedList<GetPendingTransactionResponse> PendingTransactionStatus { get; set; } = default!;
        public GetWalletBalanceResponse WalletBalance { get; set; } = default!;
    }
}
