using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Commands.FundWallet;
using static Application.Commands.VerifyFunding;
using static Application.Queries.GetTransactionHistory;
using static Application.Queries.GetWalletBalance;

namespace Host.Controllers
{
    [Authorize]
    public class WalletController(IMediator mediator) : Controller
    {
        public async Task<IActionResult> Index(int page = 1, int PageSize = 7)
        {
            var customerId = ClaimsHelper.GetCustomerId(User);

            var balance = await mediator.Send(
                new GetWalletBalanceQuery(customerId));

            var transactions = await mediator.Send(
                new GetTransactionHistoryQuery(customerId, page, PageSize));

            ViewBag.Balance = balance.Data?.Balance ?? 0;
            ViewBag.WalletId = balance.Data?.WalletId;

            return View(transactions.Data);
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
    }
}