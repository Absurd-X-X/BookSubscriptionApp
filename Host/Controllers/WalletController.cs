using Application.ViewModels;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Commands.FundWallet;
using static Application.Commands.VerifyFunding;
using static Application.Queries.GetPendingTransaction;
using static Application.Queries.GetTransactionByTransactionStatus;
using static Application.Queries.GetTransactions;
using static Application.Queries.GetWalletBalance;

namespace Host.Controllers
{
    public class WalletController(IMediator mediator) : Controller
    {
        public async Task<IActionResult> Index(
        int txPage = 1,
        int creditPage = 1,
        int debitPage = 1,
        int pendingPage = 1,
        int pageSize = 3)
        {
            var customerId = ClaimsHelper.GetCustomerId(User);

            var balance = await mediator.Send(new GetWalletBalanceQuery(customerId));

            var transactionHistory = await mediator.Send(new GetTransactionsQuery(txPage, pageSize));

            var transactionStatus = await mediator.Send(new GetTransactionByTypeQuery(creditPage, pageSize, TransactionType.Credit));

            var transactionStatusDebit = await mediator.Send(new GetTransactionByTypeQuery(debitPage, pageSize, TransactionType.Debit));

            var transactionTypes = await mediator.Send(new GetPendingTransactionQuery(pendingPage, pageSize, WalletTransactionStatus.Pending));

            var walletFullDetails = new WalletFullData
            {
                Transactions = transactionHistory.Data!,
                TransactionStatusDebit = transactionStatusDebit.Data!,
                TransactionStatusCredit = transactionStatus.Data!,
                PendingTransactionStatus = transactionTypes.Data!,
                WalletBalance = balance.Data!
            };

            ViewBag.Balance = balance.Data?.Balance ?? 0;
            ViewBag.WalletId = balance.Data?.WalletId;

            return View(walletFullDetails);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fund(decimal amount)
        {
            var customerId = ClaimsHelper.GetCustomerId(User);
            var result = await mediator.Send(
                new FundWalletCommand(customerId, amount));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return Redirect(result.Data!.AuthorizationUrl);
        }
        [HttpPost]
        public async Task<IActionResult> VerifyFunding(string reference)
        {
            var result = await mediator.Send(
                new VerifyFundingCommand(reference));

            TempData[result.Status ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Callback(string reference)
        {
            var result = await mediator.Send(new VerifyFundingCommand(reference));
            TempData[result.Status ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}