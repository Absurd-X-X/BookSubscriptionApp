using Application.Command;
using Application.Commands;
using Application.Features.ReaderBooks.Commands.ToggleBookListMembership;
using Application.Features.ReaderBooks.Queries.GetReaderBooksPage;
using Application.Features.ReaderEngagement.Commands.DeleteReview;
using Application.Features.ReaderEngagement.Queries.GetReaderEngagementDashboard;
using Application.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Command.AutoRenewSubscription;
using static Application.Command.Subscribe;
using static Application.Commands.ChangePassword;
using static Application.Commands.FundWallet;
using static Application.Commands.MarkAllNotificationsRead;
using static Application.Commands.MarkNotificationRead;
using static Application.Commands.UploadProfilePIcs;
using static Application.Commands.VerifyFunding;
using static Application.Queries.GetAllBook;
using static Application.Queries.GetBookById;
using static Application.Queries.GetReaderDetails;
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
                return RedirectToAction(nameof(ManageSub));
            }

            TempData["Success"] = process.Message;
            return RedirectToAction(nameof(ManageSub));
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

        public class ToggleReadRequest { public Guid Id { get; set; } public bool Read { get; set; } }
        public class IdRequest { public Guid Id { get; set; } }

        [HttpPost]
        public async Task<IActionResult> ToggleNotificationRead([FromBody] ToggleReadRequest req)
        {
            var userId = ClaimsHelper.GetUserId(User);
            await mediator.Send(new MarkNotificationReadCommand(req.Id, userId, req.Read));
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotification([FromBody] IdRequest req)
        {
            await mediator.Send(new DeleteNotification.DeleteNotificationCommand(req.Id));
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveNotification([FromBody] IdRequest req)
        {
            await mediator.Send(new ArchiveNotification.ArchiveNotificationCommand(req.Id));
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var userId = ClaimsHelper.GetUserId(User);
            await mediator.Send(new MarkAllNotificationsReadCommand(userId));
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ReaderDashboard(CancellationToken cancellationToken)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(
                new GetReaderDashboard.GetReaderDashboardQuery(userId),
                cancellationToken);

            if (!result.Status)
            {
                return NotFound(result.Message);
            }

            return View(result.Data);
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
        public async Task<IActionResult> Subscribe(Guid subTypeId)
        {
            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new SubscribeCommand(userId, subTypeId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(ReaderDashboard));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(ReaderDashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveReadingProgress(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new RemoveReadingProgress.RemoveReadingProgressCommand(ClaimsHelper.GetUserId(User), id),
                cancellationToken);

            if (!result.Status)
            {
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "danger";
                TempData["ToastIcon"] = "!";
            }
            else
            {
                TempData["ToastMessage"] = "Book removed from your list";
                TempData["ToastType"] = "danger";
                TempData["ToastIcon"] = "🗑";
            }

            return RedirectToAction("ViewAllReadingProgress", "Reader");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReadingGoal(
    ReadingGoalType type,
    int target,
    DateTime? deadline,
    string? motivation,
    CancellationToken cancellationToken)
        {

            var result = await mediator.Send(
                new UpdateReadingGoal.UpdateReadingGoalCommand(ClaimsHelper.GetUserId(User), type, target, deadline, motivation),
                cancellationToken);

            if (!result.Status)
            {
                TempData["ToastMessage"] = result.Message;
                TempData["ToastType"] = "danger";
                TempData["ToastIcon"] = "!";
            }
            else
            {
                TempData["ToastMessage"] = "Reading goal updated";
                TempData["ToastType"] = "success";
                TempData["ToastIcon"] = "🎯";
            }

            return RedirectToAction(nameof(ReaderDashboard));
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
        public async Task<IActionResult> ReaderCollection(CancellationToken cancellationToken)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);

            var bookmarks = await mediator.Send(new GetAllBookmarks.GetAllBookmarksQuery(readerId), cancellationToken);
            var readingList = await mediator.Send(new GetReadingList.GetReadingListQuery(readerId), cancellationToken);
            var favorites = await mediator.Send(new GetFavorites.GetFavoritesQuery(readerId), cancellationToken);

            ViewData["Bookmarks"] = bookmarks.Data;
            ViewData["ReadingList"] = readingList.Data;
            ViewData["Favorites"] = favorites.Data;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBookmark([FromBody] AddBookmark.AddBookmarkCommand command)
        {
            var result = await mediator.Send(command);
            if (!result.Status) return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBookmark(Guid bookmarkId, CancellationToken ct)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);
            var result = await mediator.Send(new RemoveBookmark.RemoveBookmarkCommand(readerId, bookmarkId), ct);
            return Json(new { success = result.Status, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookmarkNote(Guid bookmarkId, string note, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateBookmarkNote.UpdateBookmarkNoteCommand(bookmarkId, note), ct);
            return Json(new { success = result.Status, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> ReaderEngagement(string range = "8w", CancellationToken ct = default)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);
            var vm = await mediator.Send(new GetReaderEngagementDashboardQuery(readerId, range), ct);
            return View(vm);
        }

        [HttpGet]

        public async Task<IActionResult> ReaderAccount()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReaderBilling(CancellationToken cancellationToken)
        {
            var readerId = ClaimsHelper.GetUserId(User);

            var wallet = await mediator.Send(new GetWalletDashboard.GetWalletDashboardQuery(readerId), cancellationToken);
            var subscription = await mediator.Send(new GetSubscriptionByUserIdQuery(readerId), cancellationToken);

            ViewData["Wallet"] = wallet.Data;
            ViewData["Subscription"] = subscription.Data;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReaderDetails()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new GetReaderDetailsQuery(userId));

            if (!result.Status)
            {
                return NotFound(result.Message);
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ReaderPersonal()
        {
            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new GetPersonalSettings.GetPersonalSettingsQuery(userId));

            if (!result.Status)
            {
                return NotFound(result.Message);
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllFavorites(
            int page = 1,
            int pageSize = 12,
            string? search = null,
            Guid? categoryId = null,
            string? sortBy = null,
            CancellationToken cancellationToken = default)
        {
            var readerId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(
                new GetPagenatedFavorites.GetPagenatedFavoritesQuery(readerId, page, pageSize, search, categoryId, sortBy),
                cancellationToken);

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookmark(
            int page = 1,
            int pageSize = 12,
            string? search = null,
            string? sortBy = null,
            CancellationToken cancellationToken = default)
        {
            var readerId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(
                new GetAllPagenatedBookmarks.GetAllPagenatedBookmarksQuery(readerId, page, pageSize, search, sortBy),
                cancellationToken);

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> BookmarksForBook(Guid bookId)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);

            var result = await mediator.Send(
                new GetAllBookmarks.GetAllBookmarksQuery(readerId));

            if (!result.Status)
                return Json(new { status = false, data = (object?)null });

            var filtered = result.Data.Where(b => b.BookId == bookId);
            return Json(new { status = true, data = filtered });
        }

        [HttpGet]
        public async Task<IActionResult> Reviews(
            int page = 1, int pageSize = 6, string? search = null,
            string? sortBy = null, int? rating = null, Guid? bookId = null,
            CancellationToken ct = default)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);
            var vm = await mediator.Send(new GetReaderReviewsPageQuery(
                readerId, page, pageSize, search, sortBy, rating, bookId), ct);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ReadBook(Guid id)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);
            ViewBag.ReaderId = readerId; 

            var result = await mediator.Send(
                new GetBookForReading.GetBookForReadingQuery(
                    readerId,
                    id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(ReaderBooks));
            }

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress([FromBody] UpdateReadingProgress.UpdateReadingProgressCommand command)
        {
            var result = await mediator.Send(command);
            if (!result.Status) return BadRequest(result.Message);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(Guid reviewId, CancellationToken ct)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);
            var success = await mediator.Send(new DeleteReviewCommand(reviewId, readerId), ct);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(
        string fullName, string email, string username, string? bio, IFormFile? avatarFile)
        {
            var userId = ClaimsHelper.GetUserId(User);

            Stream? avatarStream = avatarFile != null ? avatarFile.OpenReadStream() : null;

            var command = new UpdatePersonalSettings.UpdatePersonalSettingsCommand(
                userId, fullName, email, username, bio,
                avatarFile?.FileName, avatarStream);

            var result = await mediator.Send(command);

            return Json(new { success = result.Status, message = result.Message, avatarUrl = result.Data });
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllReadingProgress(
            int page = 1,
            int pageSize = 5,
            string? search = null,
            string? sortBy = null,
            string? filter = null,
            CancellationToken cancellationToken = default)
        {
            var userId = ClaimsHelper.GetCustomerId(User);
            var result = await mediator.Send(
                new GetCurrentlyReading.GetCurrentlyReadingQuery(userId, page, pageSize, search, sortBy, filter),
                cancellationToken);

            if (!result.Status)
            {
                return NotFound(result.Message);
            }

            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> ReaderBooks(CancellationToken ct)
        {
            var readerId = ClaimsHelper.GetUserId(User);
            var vm = await mediator.Send(new GetReaderBooksPageQuery(readerId), ct);
            return View(vm);
        }

        [HttpPost]

        public async Task<IActionResult> ChangePassword(string initialPassword, string newPassword)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var res = await mediator.Send(new ChangePasswordCommand(userId, initialPassword, newPassword));

            if (!res.Status)
            {
                TempData["Error"] = res.Message;
                return RedirectToAction(nameof(ReaderDetails));
            }

            TempData["Success"] = res.Message;
            return RedirectToAction(nameof(ReaderDetails));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBookList(Guid bookId, BookListType listType, CancellationToken ct)
        {
            var readerId = ClaimsHelper.GetCustomerId(User);
            var isNowInList = await mediator.Send(
                new ToggleBookListMembershipCommand(readerId, bookId, listType), ct);

            return Json(new { success = true, isInList = isNowInList });
        }

        [HttpGet]
        public async Task<IActionResult> ManageSub(CancellationToken cancellationToken)
        {
            var readerId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new GetSubscriptionByUserIdQuery(readerId), cancellationToken);

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(ReaderDashboard));
            }

            return View(result.Data);
        }

        [HttpGet]

        public async Task<IActionResult> ReaderActivities()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReadingProgress()
        {
            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new GetMyReadingDashboard.GetMyReadingDashboardQuery(userId));

            if (!result.Status)
            {
                return NotFound(result.Message);
            }

            return View(result.Data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fund(decimal amount)
        {
            var customerId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(
                new FundWalletCommand(customerId, amount));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(ReaderDashboard));
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
            return RedirectToAction(nameof(ReaderDashboard));
        }

        [HttpGet]
        public async Task<IActionResult> ReaderBook(Guid bookId)
        {
            var result = await mediator.Send(new GetBookByIdQuery(bookId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(ReaderBooks));
            }
            TempData["Success"] = result.Message;
            return View(result.Data);
        }
    }
}