using Infrastructure.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Web.Helpers;
using static Application.Commands.SendMessage;
using static Application.Commands.StartConversation;
using static Application.Queries.GetConversationByUserId;
using static Application.Queries.GetConversationMessages;

namespace Host.Controllers
{
    [Authorize]
    public class ChatController(
        IMediator mediator,
        IHubContext<ChatHub> chatHub) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(
                new GetConversationByUserIdQuery(userId));
            return View(result.Data);
        }

        public async Task<IActionResult> Conversation(Guid id)
        {
            var result = await mediator.Send(
                new GetConversationMessagesQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ConversationId = id;
            ViewBag.CurrentUserId = ClaimsHelper.GetUserId(User);
            ViewBag.CurrentUserName = ClaimsHelper.GetFullName(User);

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(string userName)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(
                new StartConversationCommand(userId, userName));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(
                nameof(Conversation),
                new { id = result.Data!.ConversationId });
        }

        [HttpPost]
        public async Task<IActionResult> Send(
            Guid conversationId, string content)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var fullName = ClaimsHelper.GetFullName(User);

            var result = await mediator.Send(
                new SendMessageCommand(conversationId, userId, content));

            if (!result.Status)
                return Json(new { success = false, message = result.Message });

            await chatHub.Clients
                .Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", new
                {
                    senderId = userId,
                    senderName = fullName,
                    content,
                    sentAt = DateTime.UtcNow,
                });

            return Json(new { success = true });
        }
    }
}