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
            string Author,
            string Isbn,
            string Genre,
            int PublicationYear,
            int Pages,
            string BookFileName,
            string BookCoverName,
            Stream BookFileStream,
            Stream BookCoverStream,
            Guid CategoryId) : IRequest<Result<string>>;

        
        public class AddBookHandler(
            IBookRepository _bookRepository,
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
                {
                    return Result<string>.Failure("Invalid file type. Only PDF, EPUB, and TXT files are allowed.");
                }

                var coverExtension = Path.GetExtension(request.BookCoverName).ToLower();

                var allowedCoverExtensions = new[] { ".jpg", ".jpeg", ".png" };

                if (!allowedCoverExtensions.Contains(coverExtension))
                {
                    return Result<string>.Failure("Invalid cover file type. Only JPG, JPEG, and PNG files are allowed.");
                }

                if (string.IsNullOrWhiteSpace(request.BookFileName) )
                {
                    return Result<string>.Failure("Book file is required.");
                }

                if (string.IsNullOrWhiteSpace(request.BookCoverName))
                {
                    return Result<string>.Failure("Book cover image is required.");
                }

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","uploads", "books");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                var bookExt = Path.GetExtension(request.BookFileName);

                var unique = $"{Guid.NewGuid()}_{request.Title}{bookExt}";

                var combine = Path.Combine(folder, unique);

                using(var fileStream = new FileStream(combine, FileMode.Create))
                {
                    await request.BookFileStream.CopyToAsync(fileStream, cToken);
                }

                var imgFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","uploads", "bookcovers");

                if (!Directory.Exists(imgFolder))
                {
                    Directory.CreateDirectory(imgFolder);
                }
                var imgExt = Path.GetExtension(request.BookCoverName);
                var imgUnique = $"{Guid.NewGuid()}_{request.Author}{imgExt}";

                var combineImg = Path.Combine(imgFolder, imgUnique);

                using(var imgStream = new FileStream(combineImg, FileMode.Create))
                {
                    await request.BookCoverStream.CopyToAsync(imgStream, cToken);
                }

                var book = new Book
                {
                    Author = request.Author,
                    BookCoverUrl = $"/uploads/bookcovers/{imgUnique}",
                    BookFileUrl = $"/uploads/books/{unique}",
                    CategoryId = request.CategoryId,
                    Isbn = request.Isbn,
                    Genre = request.Genre,
                    PublicationYear = request.PublicationYear,
                    Pages = request.Pages,
                    LibraryId = getLibrary.Id,
                    CreatedBy = getLibrary.Id.ToString(),
                    Title = request.Title,
                    FileType = extension
                };

                await _bookRepository.AddAsync(book);
                await _unitOfWork.SaveAsync();

                return Result<string>.Success(book.BookFileUrl, "Successfully created !");
            }
        }
    }
}

