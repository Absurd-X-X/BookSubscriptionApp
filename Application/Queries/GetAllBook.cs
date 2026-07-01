using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllBook
    {
        public record GetAllBookQuery(
            int Page,
            int PageSize
            ) : IRequest<Result<PagenatedList<GetAllBookResponse>>>;

        public class GetAllBookHandler(
            IBookRepository bookRepository
            ) : IRequestHandler<GetAllBookQuery, Result<PagenatedList<GetAllBookResponse>>>
        {
            async Task<Result<PagenatedList<GetAllBookResponse>>> IRequestHandler<GetAllBookQuery, Result<PagenatedList<GetAllBookResponse>>>.
                Handle(GetAllBookQuery request, CancellationToken cancellationToken)
            {
                PageRequest page = new PageRequest
                {
                    Page = request.Page,
                    PageSize = 10
                };

                var books = await bookRepository.GetAllAsync(page, true);

                var bookData = books.Items.Where(x => !x.IsDeleted).Select(x => new GetAllBookResponse(
                    x.Id,
                    x.Title,
                    x.Author,
                    x.PublicationYear,
                    x.Isbn,
                    x.Genre,
                    x.Library.Id,
                    x.Library.Name,
                    x.Category.Id,
                    x.Category.Name,
                    x.Category.Description,
                    x.BookCoverUrl,
                    x.BookFileUrl,
                    x.DateCreated,
                    x.NoOfTimeReadByPeople * 5,
                    x.NoOfTimeReadByPeople
                    )).ToList();

                var pagedData = new PagenatedList<GetAllBookResponse>
                {
                    Items = bookData,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = bookData.Count
                };

                return Result<PagenatedList<GetAllBookResponse>>.Success(pagedData, "Retrieved");
            }
        }

        public record GetAllBookResponse(
            Guid BookId, string Title,
            string Author, int PublicationYear,
            string Isbn, string Genre,
            Guid LibraryId, string LibraryName,
            Guid CategoryId, string CategoryName,
            string CategoryDescription,
            string BookCoverUrl, string BookFileUrl, DateTime DateAdded,
            int EngagementPercent ,
            int NoOfTimeReadByPeople
            );
    }
}
