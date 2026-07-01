using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Command.AutoRenewSubscription;
using static Application.Command.Subscribe;
using static Application.Commands.UploadProfilePIcs;
using static Application.Queries.GetAllBook;
using static Application.Queries.GetSubscriptionById;
using static Application.Queries.GetSubscriptionByUserId;
using static Application.Queries.GetUserDetails;

namespace Host.Controllers
{
    public class ReaderController(IMediator mediator) : Controller
    {
        public async Task<IActionResult> Index(GetAllBookQuery query)
        {
            var result = await mediator.Send(query);
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> AutoRenewSub(bool autoRenew, Guid subId)
        {
            var process = await mediator.Send(new AutoRenewCommand(autoRenew, subId));

            if (!process.Status)
            {
                TempData["Error"] = process.Message;
                return View(process.Data);
            }

            TempData["Success"] = process.Message;
            return View(process.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetMySubs()
        {
            var ReaderId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new GetSubscriptionByUserIdQuery(ReaderId));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }
            TempData["Error"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ReaderDashboard()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetMySubsById(Guid id)
        {
            var result = await mediator.Send(new GetSubscriptionByIdQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetMySubs));
            }
            TempData["Error"] = result.Message;
            return View(result.Data);
        }
        [HttpPost]
        public async Task<IActionResult> Subscribe(Guid userId, Guid subTypeId)
        {
            var result = await mediator.Send(new SubscribeCommand(userId, subTypeId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetMyDetails()
        {
            var userId = ClaimsHelper.GetUserId(User);

            var process = await mediator.Send(new GetUserDetailsQuery(userId));

            if (!process.Status)
            {
                TempData["Error"] = process.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = process.Message;
            return View(process.Data);
        }

        [HttpGet]

        public async Task<IActionResult> ReaderBooks()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> ReadingProgress()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImg(IFormFile file)
        {
            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new UploadProfilePicsCommand(userId, file.OpenReadStream(), file.FileName));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["Success"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]

        public async Task<IActionResult> ReaderCollection()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> ReaderEngagement()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> ReaderAccount()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> ReaderBilling()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> ReaderPersonal()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllReadingProgress()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManageSub()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> ReaderActivities()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> BookReadingProgress()
        {
            return View();
        }
    }
}
