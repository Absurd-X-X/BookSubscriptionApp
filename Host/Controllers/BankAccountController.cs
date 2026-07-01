using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Command.DeleteBankAccount;
using static Application.Command.UpdateBankAccount;
using static Application.Commands.AddBankAccount;
using static Application.Commands.SetDefaultBankAccount;
using static Application.Queries.GetBankAccountById;
using static Application.Queries.GetBankAccountByUserId;

namespace Host.Controllers
{
    public class BankAccountController(IMediator mediator) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var customerId = ClaimsHelper.GetCustomerId(User);

            var result = await mediator.Send(
                new GetBankAccountByUserIdQuery(customerId));

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(AddBankAccountCommand command)
        {
            var customerId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(
                command with { UserId = customerId });

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]

        public async Task<IActionResult> UpdateAccount(UpdateAcountCommand command)
        {
            var result = await mediator.Send(command);

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetUserAccount));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]

        public async Task<IActionResult> DeleteAccount(Guid bankAccountId)
        {
            var result = await mediator.Send(
                new DeleteBankAccountCommand(bankAccountId));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetUserAccount));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]

        public async Task<IActionResult> SetDefault(SetDefaultBankAccountCommand command)
        {
            var customerId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(
                command with { UserId = customerId });

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetUserAccount));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUserAccount(GetBankAccountByUserIdQuery query)
        {
            var result = await mediator.Send(query with { UserId = ClaimsHelper.GetUserId(User) });
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountById(GetBankAccountByIdQuery query)
        {
            var result = await mediator.Send(query);
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetUserAccount));
            }
            return View(result.Data);
        }
    }
}
