using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetByLibraryId
    {
        public record GetByLibraryIdQuery(
            Guid LibraryId,
            int Page,
            int PageSize,
            string? Search = null,
            Guid? CategoryId = null,
            bool? IsPublished = null,
            string? SortBy = null
            ) : IRequest<Result<PagenatedList<GetByLibraryIdResponse>>>;

        public class GetByLibraryIdHandler(
            IBookRepository bookRepository
            ) : IRequestHandler<GetByLibraryIdQuery, Result<PagenatedList<GetByLibraryIdResponse>>>
        {
            public async Task<Result<PagenatedList<GetByLibraryIdResponse>>> Handle(GetByLibraryIdQuery request, CancellationToken cancellationToken)
            {
                var books = await bookRepository.GetBookByLibraryIdAsync(
                    request.LibraryId,
                    new PageRequest
                    {
                        Page = request.Page,
                        PageSize = request.PageSize
                    },
                    true,
                    request.Search,
                    request.CategoryId,
                    request.IsPublished,
                    request.SortBy);

                var bookData = books.Items.Select(x => new GetByLibraryIdResponse(
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
                    x.NoOfTimeReadByPeople,
                    x.IsPublished
                    )).ToList();

                var pagedData = new PagenatedList<GetByLibraryIdResponse>
                {
                    Items = bookData,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = books.TotalCount
                };

                return Result<PagenatedList<GetByLibraryIdResponse>>.Success(pagedData, "Retrieved");
            }
        }

        public record GetByLibraryIdResponse(
            Guid BookId, string Title,
            string Author, int PublicationYear,
            string Isbn, string Genre,
            Guid LibraryId, string LibraryName,
            Guid CategoryId, string CategoryName,
            string CategoryDescription,
            string BookCoverUrl, string BookFileUrl, DateTime DateAdded,
            int EngagementPercent,
            int NoOfTimeReadByPeople,
            bool IsPublished
            );
    }
}