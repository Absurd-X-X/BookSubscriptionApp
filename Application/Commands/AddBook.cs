using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Command
{
    public class AddBook
    {
        public record AddBookCommand(
            Guid UserId,
            string Title,
            string? Subtitle,
            string Author,
            string Publisher,
            int PublicationYear,
            string Language,
            string Isbn,
            string Genre,
            string About,
            int Pages,
            Guid CategoryId,
            string PricingType,
            decimal Price,
            decimal Discount,
            string AccessLevel,
            bool RequireLogin,
            bool AllowDownload,
            bool AllowPrint,
            bool AllowCopyPaste,
            string BookFileName,
            string BookCoverName,
            Stream BookFileStream,
            Stream BookCoverStream) : IRequest<Result<string>>;

        public class AddBookHandler(
            IBookRepository _bookRepository,
            IBookVersionRepository _bookVersionRepository,
            ILibraryRepository _libraryRepository,
            IUnitOfWork _unitOfWork,
            IUserRepository userRepository
            ) : IRequestHandler<AddBookCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddBookCommand request, CancellationToken cToken)
            {
                var check = await _bookRepository.IsExistAsync(request.Title, request.Author);

                if (check)
                    return Result<string>.Failure("Book intended to add already exist");

                var user = await userRepository.GetAsync(request.UserId);

                if (user == null)
                    return Result<string>.Failure("You've not logged in");

                var getLibrary = await _libraryRepository.GetLibraryAsync(user.Email);

                if (getLibrary == null)
                    return Result<string>.Failure("No library found");

                var extension = Path.GetExtension(request.BookFileName).ToLower();
                var allowedExtensions = new[] { ".pdf", ".epub", ".txt" };

                if (!allowedExtensions.Contains(extension))
                    return Result<string>.Failure("Invalid file type. Only PDF, EPUB, and TXT files are allowed.");

                var coverExtension = Path.GetExtension(request.BookCoverName).ToLower();
                var allowedCoverExtensions = new[] { ".jpg", ".jpeg", ".png" };

                if (!allowedCoverExtensions.Contains(coverExtension))
                    return Result<string>.Failure("Invalid cover file type. Only JPG, JPEG, and PNG files are allowed.");

                if (string.IsNullOrWhiteSpace(request.BookFileName))
                    return Result<string>.Failure("Book file is required.");

                if (string.IsNullOrWhiteSpace(request.BookCoverName))
                    return Result<string>.Failure("Book cover image is required.");

                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "books");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var bookExt = Path.GetExtension(request.BookFileName);
                var unique = $"{Guid.NewGuid()}_{request.Title}{bookExt}";
                var combine = Path.Combine(folder, unique);
                long fileSizeBytes;

                using (var fileStream = new FileStream(combine, FileMode.Create))
                {
                    await request.BookFileStream.CopyToAsync(fileStream, cToken);
                    fileSizeBytes = fileStream.Length;
                }

                var imgFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "bookcovers");
                if (!Directory.Exists(imgFolder))
                    Directory.CreateDirectory(imgFolder);

                var imgExt = Path.GetExtension(request.BookCoverName);
                var imgUnique = $"{Guid.NewGuid()}_{request.Author}{imgExt}";
                var combineImg = Path.Combine(imgFolder, imgUnique);

                using (var imgStream = new FileStream(combineImg, FileMode.Create))
                {
                    await request.BookCoverStream.CopyToAsync(imgStream, cToken);
                }

                string mimeType = extension switch
                {
                    ".epub" => "application/epub+zip",
                    ".pdf" => "application/pdf",
                    ".txt" => "text/plain",
                    _ => "application/octet-stream"
                };

                var book = new Book
                {
                    Title = request.Title,
                    Subtitle = request.Subtitle,
                    Author = request.Author,
                    Publisher = request.Publisher,
                    PublicationYear = request.PublicationYear,
                    Language = request.Language,
                    Isbn = request.Isbn,
                    Genre = request.Genre,
                    About = request.About,
                    Pages = request.Pages,
                    CategoryId = request.CategoryId,
                    PricingType = request.PricingType,
                    Price = request.PricingType == "Free" ? 0 : request.Price,
                    Discount = request.Discount,
                    AccessLevel = request.AccessLevel,
                    RequireLogin = request.RequireLogin,
                    AllowDownload = request.AllowDownload,
                    AllowPrint = request.AllowPrint,
                    MimeType = mimeType,
                    AllowCopyPaste = request.AllowCopyPaste,
                    BookCoverUrl = $"/uploads/bookcovers/{imgUnique}",
                    BookFileUrl = $"/uploads/books/{unique}",
                    FileType = extension.TrimStart('.').ToUpper(),
                    LibraryId = getLibrary.Id,
                    CreatedBy = getLibrary.Id.ToString(),
                    IsPublished = true
                };

                await _bookRepository.AddAsync(book);

                var version = new BookVersion
                {
                    BookId = book.Id,
                    VersionNumber = "1.0",
                    FileUrl = book.BookFileUrl,
                    FileType = extension.TrimStart('.').ToUpper(),
                    FileSizeBytes = fileSizeBytes,
                    ReleaseNote = "Initial version of the book.",
                    MimeType = mimeType,
                    UploadedBy = getLibrary.Id.ToString(),
                    IsCurrent = true
                };

                await _bookVersionRepository.AddAsync(version);
                await _unitOfWork.SaveAsync();

                return Result<string>.Success(book.BookFileUrl, "Successfully created !");
            }
        }
    }
}