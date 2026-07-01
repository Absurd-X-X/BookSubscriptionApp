using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Command.AddBook;
using static Application.Commands.UploadProfilePIcs;
using static Application.Queries.GetAllBook;
using static Application.Queries.GetBookByCategoryId;
using static Application.Queries.GetBookById;
using static Application.Queries.GetBookByLibraryId;
using static Application.Queries.GetCategoryByName;
using static Application.Queries.SearchBookByTitle;

namespace Host.Controllers
{
    public class BookController(IMediator mediator) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
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

        [HttpGet]
        public async Task<IActionResult> GetBooks(int page = 1, int pageSize = 10)
        {
            var result = await mediator.Send(new GetAllBookQuery(page, pageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Data);
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetByCategory(Guid categoryId)
        {
            var result = await mediator.Send(new GetBookByCategoryIdQuery(categoryId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetBook(Guid bookId)
        {
            var result = await mediator.Send(new GetBookByIdQuery(bookId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetBooks));
            }

            TempData["success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetByLIbrary(Guid libraryId)
        {
            var result = await mediator.Send(new GetBookByLibraryIdQuery(libraryId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetBooks));
            }

            TempData["success"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> SearchByTitle(string title)
        {
            var result = await mediator.Send(new SearchByTitleQuery(title));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetBooks));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> SearchByAuthor(string author)
        {
            var result = await mediator.Send(new SearchByTitleQuery(author));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetBooks));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> SearchByCategory(string name)
        {
            var result = await mediator.Send(new GetCategoryByNameQuery(name));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetBooks));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
    }
}
