using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Command.AddBook;
using static Application.Commands.UploadProfilePIcs;
using static Application.Queries.GetConversationByUserId;
using static Application.Queries.GetLibraryById;
using static Application.Queries.GetLibraryDashboard;

namespace Host.Controllers
{
    public class LibraryController(IMediator mediator) : Controller
    {

        [HttpPost]
        public async Task<IActionResult> AddBook(
            [FromForm] IFormFile bookFile,
            [FromForm] IFormFile bookCover,
            [FromForm] string title, string isbn, string genre, int publishYear,
            int page, Guid categoryId, string author)

        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new AddBookCommand(
                userId,
                author,
                title,
                isbn,
                genre,
                publishYear,
                page,
                bookFile.FileName,
                bookCover.FileName,
                bookFile.OpenReadStream(),
                bookCover.OpenReadStream(),
                categoryId
                ));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }


        [HttpGet]

        public async Task<IActionResult> AddBook()
        {
            return View();
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
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetLibraryBooks()
        {
            return View();
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
        public async Task<IActionResult> GetRevenue()
        {
            return View();
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
