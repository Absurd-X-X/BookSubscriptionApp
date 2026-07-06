using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Command.AddBook;
using static Application.Commands.UploadProfilePIcs;
using static Application.Queries.GetBookByLibraryId;
using static Application.Queries.GetByLibraryId;
using static Application.Queries.GetCategories;
using static Application.Queries.GetConversationByUserId;
using static Application.Queries.GetLibraryById;
using static Application.Queries.GetLibraryDashboard;
using static Application.Queries.GetTransactionHistory;

namespace Host.Controllers
{
    public class LibraryController(IMediator mediator) : Controller
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
                return RedirectToAction(nameof(AddBook));
            }

            if (bookCover is null || bookCover.Length == 0)
            {
                TempData["Error"] = "Please select a book cover image to upload.";
                return RedirectToAction(nameof(AddBook));
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

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(GetByLibrary));
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

        public async Task<IActionResult> GetLibraryBook()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> EditBook()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> PreviewBook()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenue(int page = 1, int PageSize = 5)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new GetTransactionHistoryQuery(userId, page, PageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Data);
            }
            TempData["Success"] = result.Message;

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Analysis()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> GetReviews()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> GetReview()
        {
            return View();
        }
        [HttpGet]

        public async Task<IActionResult> GetActivity()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> GetNotifications()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> GetNotification()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> GetWallet()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> LibrarySettings()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> TopPerformingBooks()
        {
            return View();
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

            return View(result.Data);
        }
    }
}
