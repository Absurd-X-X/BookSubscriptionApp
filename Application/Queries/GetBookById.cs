using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetBookById
    {
        public record GetBookByIdQuery(Guid Id) : IRequest<Result<GetBookByIdResponse>>;

        public class GetBookByIdHandler(IBookRepository bookRepository, IUnitOfWork unitOfWork) : 
            IRequestHandler<GetBookByIdQuery, Result<GetBookByIdResponse>>
        {
            public async Task<Result<GetBookByIdResponse>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.Id);

                if (book is null)
                    return Result<GetBookByIdResponse>.Failure("Retrieved");

                book.NoOfTimeReadByPeople++;
                int engagementPercentage = book.NoOfTimeReadByPeople * 10;

                if (engagementPercentage >= 100)
                {
                    engagementPercentage = 100;
                }
                var bookData = new GetBookByIdResponse(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.PublicationYear,
                    book.Isbn,
                    book.Genre,
                    book.LibraryId,
                    book.Library.Name,
                    book.CategoryId,
                    book.Category.Name,
                    book.Category.Description,
                    book.BookCoverUrl,
                    book.BookFileUrl,
                    book.IsDeleted,
                    book.DateCreated,
                    engagementPercentage
                    );
                await unitOfWork.SaveAsync();
                return Result<GetBookByIdResponse>.Success(bookData, "gotten");
            }
        }

        public record GetBookByIdResponse(
            Guid BookId, string Title,
            string Author, int PublicationYear,
            string Isbn, string Genre,
            Guid LibraryId, string LibraryName,
            Guid CategoryId, string CategoryName,
            string CategoryDescription,
            string BookCoverUrl, string BookFileUrl,
            bool IsActive, DateTime AddedDate, int Engagement
            );

    }
}
