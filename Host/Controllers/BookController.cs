using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using static Application.Command.AddBook;
using static Application.Queries.GetAllBook;
using static Application.Queries.GetBookByCategoryId;
using static Application.Queries.GetBookById;
using static Application.Queries.GetByLibraryId;
using static Application.Queries.GetCategories;
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
