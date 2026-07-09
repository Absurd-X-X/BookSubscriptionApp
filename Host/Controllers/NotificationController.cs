using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Commands.MarkAllNotificationsRead;
using static Application.Queries.GetMyNotifications;

namespace Host.Controllers
{
    [Authorize]
    public class NotificationController(IMediator mediator) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(
                new GetMyNotificationsQuery(userId));
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Unread()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(
                new GetMyNotificationsQuery(userId));
            return Json(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead()
        {
            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(
                new MarkAllNotificationsReadCommand(userId));

            return Json(new { success = result.Status });
        }
    }
}