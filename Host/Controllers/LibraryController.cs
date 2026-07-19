using Application.Command;
using Application.Commands;
using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Queries;
using Application.Query;
using Application.ViewModels;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Web.Helpers;
using static Application.Command.AddBook;
using static Application.Commands.ArchiveBook;
using static Application.Commands.MarkAllNotificationsRead;
using static Application.Commands.MarkNotificationRead;
using static Application.Commands.SendMessage;
using static Application.Commands.UploadProfilePIcs;
using static Application.Queries.GetBookById;
using static Application.Queries.GetBookByLibraryId;
using static Application.Queries.GetByLibraryId;
using static Application.Queries.GetCategories;
using static Application.Queries.GetConversationByUserId;
using static Application.Queries.GetLibraryAnalytics;
using static Application.Queries.GetLibraryAnalytics.GetLibraryAnalyticsHandler;
using static Application.Queries.GetLibraryAuditLogs;
using static Application.Queries.GetLibraryById;
using static Application.Queries.GetLibraryDashboard;
using static Application.Queries.GetLibrarySettings;
using static Application.Queries.GetPendingTransaction;
using static Application.Queries.GetReviewById;
using static Application.Queries.GetTransactionByTransactionStatus;
using static Application.Queries.GetTransactions;
using static Application.Queries.GetWalletBalance;

namespace Host.Controllers
{
    public class LibraryController(IMediator mediator, IHubContext<Hub> chatHub) : Controller
    {

        [HttpGet]
        public async Task<IActionResult> AddBook()
        {
            var result = await mediator.Send(new GetCategoriesQuery());
            ViewBag.Categories = result.Data ?? new List<GetCategoriesResponse>();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(
            [FromForm] IFormFile bookFile,
            [FromForm] IFormFile bookCover,
            [FromForm] string title,
            [FromForm] string? subtitle,
            [FromForm] string author,
            [FromForm] string publisher,
            [FromForm] int publicationYear,
            [FromForm] string language,
            [FromForm] string isbn,
            [FromForm] string genre,
            [FromForm] string about,
            [FromForm] int pages,
            [FromForm] Guid categoryId,
            [FromForm] string pricingType,
            [FromForm] decimal price,
            [FromForm] decimal discount,
            [FromForm] string accessLevel,
            [FromForm] bool requireLogin,
            [FromForm] bool allowDownload,
            [FromForm] bool allowPrint,
            [FromForm] bool allowCopyPaste)
        {
            if (bookFile is null || bookFile.Length == 0)
            {
                TempData["Error"] = "Please select a book file to upload.";
                return RedirectToAction(nameof(LibraryDashboard));
            }

            if (bookCover is null || bookCover.Length == 0)
            {
                TempData["Error"] = "Please select a book cover image to upload.";
                return RedirectToAction(nameof(LibraryDashboard));
            }

            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new AddBookCommand(
                userId,
                title,
                subtitle,
                author,
                publisher,
                publicationYear,
                language,
                isbn,
                genre,
                about,
                pages,
                categoryId,
                pricingType,
                price,
                discount,
                accessLevel,
                requireLogin,
                allowDownload,
                allowPrint,
                allowCopyPaste,
                bookFile.FileName,
                bookCover.FileName,
                bookFile.OpenReadStream(),
                bookCover.OpenReadStream()
                ));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(AddBook));
            }

            TempData["Success"] = "Book Added Successfully";
            return RedirectToAction(nameof(LibraryDashboard));
        }


        [HttpGet]
        public async Task<IActionResult> GetByLibrary(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        Guid? categoryId = null,
        bool? isPublished = null,
        string? sortBy = null)
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);
            var userId = ClaimsHelper.GetUserId(User);

            var response = await mediator.Send(new GetByLibraryIdQuery(
                libraryId, page, pageSize, search, categoryId, isPublished, sortBy));

            if (!response.Status)
            {
                TempData["Error"] = response.Message;
                return View(response.Data);
            }

            TempData["Success"] = response.Message;

            return View(response.Data);
        }

        public async Task<IActionResult> Index(GetLibraryDashboardQuery query)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var libraryId = ClaimsHelper.GetCustomerId(User);

            var result = await mediator.Send(query with { LibraryId = libraryId } with { UserId = userId });
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(GetLibraryByIdQuery query)
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);

            var result = await mediator.Send(query with { Id = libraryId });

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
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

        [HttpPost]
        public async Task<IActionResult> ArchiveBook(Guid bookId)
        {
            var res = await mediator.Send(new ArchiveBookCommand(bookId));

            if (!res.Status)
            {
                TempData["Error"] = res.Message;
                return RedirectToAction(nameof(GetLibraryBooks));
            }

            TempData["Success"] = res.Message;
            return RedirectToAction(nameof(GetLibraryBooks));
        }

        [HttpGet]
        public async Task<IActionResult> LibrarySettings()
        {
            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new GetLibrarySettingsQuery(userId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(LibraryDashboard));
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> TopPerformingBooks(int page = 1, int pageSize = 10)
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);

            var result = await mediator.Send(new GetTopPerformingBooks.GetTopPerformingBooksQuery(libraryId, page, pageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(new PagenatedList<GetTopPerformingBooks.TopPerformingBookDto>
                {
                    Items = new List<GetTopPerformingBooks.TopPerformingBookDto>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0
                });
            }

            return View(result.Data);
        }

        [HttpGet]

        public async Task<IActionResult> LibraryDashboard()
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);
            var userId = ClaimsHelper.GetUserId(User);

            var response = await mediator.Send(new GetLibraryDashboardQuery(libraryId, userId));

            if (!response.Status)
            {
                TempData["Error"] = response.Message;
                return View();
            }

            TempData["Success"] = response.Message;

            return View(response.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetLibraryBooksById(int page = 1, int pageSize = 10)
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);
            var userId = ClaimsHelper.GetUserId(User);

            var response = await mediator.Send(new GetBookByLibraryIdQuery(libraryId, page, pageSize));

            if (!response.Status)
            {
                TempData["Error"] = response.Message;
                return View(response.Data);
            }

            TempData["Success"] = response.Message;

            return View(response.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetLibraryBooks(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        Guid? categoryId = null,
        bool? isPublished = null,
        string? sortBy = null)
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);
            var userId = ClaimsHelper.GetUserId(User);

            var response = await mediator.Send(new GetByLibraryIdQuery(
                libraryId, page, pageSize, search, categoryId, isPublished, sortBy));

            if (!response.Status)
            {
                TempData["Error"] = response.Message;
                return View(response.Data);
            }

            TempData["Success"] = response.Message;

            return View(response.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetLibraryBook(Guid bookId)
        {
            var result = await mediator.Send(new GetBookByIdQuery(bookId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetLibraryBooks));
            }
            TempData["Success"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> EditBook(Guid id)
        {
            var result = await mediator.Send(new GetBookEdit.GetBookEditQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("GetLibraryBooks", "Library");
            }

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(UpdateBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("EditBook", new { id = model.BookId });
            }

            var command = new UpdateBook.UpdateBookCommand(
                model.BookId,
                model.Title,
                model.Subtitle,
                model.Author,
                model.Genre,
                model.Language,
                model.Isbn,
                model.About,
                model.Publisher,
                model.PublicationYear,
                model.Edition,
                model.Pages,
                model.IsPublished,
                model.AccessLevel,
                model.CategoryId,
                model.AdditionalCategories,
                model.Tags,
                model.PricingType,
                model.Price,
                model.CompareAtPrice,
                model.Discount,
                model.MembershipAccess,
                model.RentalOption,
                model.AccessType,
                model.AllowOnlineReading,
                model.AllowDownload,
                model.LimitToOneDevice,
                model.DripContent,
                model.PreviewType,
                model.FreeChapterCount,
                model.AllowReviews,
                model.AllowRatings,
                model.AllowComments,
                model.ShowReviewCount,
                model.ShowReadCount,
                model.ShowAverageRating,
                model.DisableUserFeedback,
                model.PromotionStatus,
                model.HomepageCarousel,
                model.TrendingSection,
                model.StaffPicks,
                model.EditorsChoice,
                model.ShowInSearch,
                model.FeaturedBook,
                model.ShowInRecommendations,
                model.ShowInCategoryListings,
                model.AllowSocialSharing,
                model.HideFromCatalog,
                model.AdultContentWarning,
                model.SeoTitle,
                model.SeoSlug,
                model.SeoDescription,
                model.SeoKeywords
            );

            var result = await mediator.Send(command);

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("EditBook", new { id = model.BookId });
            }

            TempData["Success"] = "Book updated successfully.";
            return RedirectToAction("EditBook", new { id = model.BookId });
        }

        [HttpGet]
        public async Task<IActionResult> PreviewBook(Guid id)
        {
            var result = await mediator.Send(new GetBookForPreview.GetBookForPreviewQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetLibraryBooks));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenue(int page = 1, int pageSize = 5)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var libraryId = ClaimsHelper.GetCustomerId(User);

            var result = await mediator.Send(
                new GetRevenueDashboard.GetRevenueDashboardQuery(userId, page, pageSize, libraryId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Data);
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Analysis(string? start, string? end, int page = 1, int pageSize = 10)
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);
            var userId = ClaimsHelper.GetUserId(User);

            var now = DateTime.UtcNow;
            var defaultStart = new DateTime(now.Year, now.Month, 1);
            var defaultEnd = defaultStart.AddMonths(1).AddDays(-1);

            var startDate = DateTime.TryParse(start, out var parsedStart) ? parsedStart : defaultStart;
            var endDate = DateTime.TryParse(end, out var parsedEnd) ? parsedEnd : defaultEnd;

            if (endDate < startDate)
            {
                (startDate, endDate) = (endDate, startDate);
            }

            var result = await mediator.Send(new GetLibraryAnalyticsQuery(
                userId, libraryId, startDate, endDate, page, pageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(new LibraryAnalyticsResponse(
                    startDate, endDate,
                    0, 0, 0, 0, 0, 0, 0, 0,
                    new List<string>(), new List<int>(),
                    null, 0, 0, null, 0, 0, 0, null, 0,
                    0, 0, 0, 0, 0, 0,
                    new ReadingTimeDistributionDto(0, 0, 0, 0, 0),
                    new List<FunnelStepDto>(),
                    new List<TopBookDto>(),
                    new DemographicsDto(0, 0, 0, 0),
                    new DeviceBreakdownDto(0, 0, 0),
                    0, 0, 0, 0, 0, 0, 0, 0,
                    new RatingsDistributionDto(0, 0, 0, 0, 0),
                    new List<LocationStatDto>()
                ));
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews(string? start, string? end, int page = 1, int pageSize = 10)
        {
            var libraryId = ClaimsHelper.GetCustomerId(User);

            var now = DateTime.UtcNow;
            var defaultStart = new DateTime(now.Year, now.Month, 1);
            var defaultEnd = defaultStart.AddMonths(1).AddDays(-1);

            var startDate = DateTime.TryParse(start, out var parsedStart) ? parsedStart : defaultStart;
            var endDate = DateTime.TryParse(end, out var parsedEnd) ? parsedEnd : defaultEnd;

            if (endDate < startDate)
            {
                (startDate, endDate) = (endDate, startDate);
            }

            var result = await mediator.Send(new GetLibraryReviews.GetLibraryReviewsQuery(
                libraryId, startDate, endDate, page, pageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(new GetLibraryReviews.LibraryReviewsResponse(
                    startDate, endDate,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    new GetLibraryReviews.RatingDistributionDto(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                    new List<GetLibraryReviews.TopReviewedBookDto>(),
                    new PagenatedList<GetLibraryReviews.ReviewRowDto> { Items = new List<GetLibraryReviews.ReviewRowDto>(), Page = page, PageSize = pageSize, TotalCount = 0 }
                ));
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetReview(Guid id)
        {
            var result = await mediator.Send(new GetReviewByIdQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Reviews");
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetActivity(int page = 1, int pageSize = 10)
        {
            var result = await mediator.Send(new GetLibraryAuditLogsQuery(page, pageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new GetAllNotificationByUserId.GetAllNotificationQuery(userId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(new GetAllNotificationByUserId.NotificationPageResponse(
                0, 0, 0, 0, 0,
                new List<GetAllNotificationByUserId.GetAllNotificationResponse>(),
                new List<GetAllNotificationByUserId.CategoryCountDto>()
));
            }

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
        public async Task<IActionResult> GetNotification(Guid id)
        {
            var result = await mediator.Send(new GetNotificationById.GetNotificationByIdQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Notifications");
            }

            return View(result.Data);
        }

        public async Task<IActionResult> GetWallet(
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

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new GetConversationByUserIdQuery(userId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Data);
            }

            ViewBag.CurrentUserId = userId;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetConversation(Guid conversationId)
        {
            var userId = ClaimsHelper.GetUserId(User);

            var listResult = await mediator.Send(new GetConversationByUserIdQuery(userId));
            if (!listResult.Status)
            {
                TempData["Error"] = listResult.Message;
                return View(nameof(GetConversations), listResult.Data);
            }

            ViewBag.CurrentUserId = userId;
            ViewBag.SelectedConversationId = conversationId;
            return View(nameof(GetConversations), listResult.Data);
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
