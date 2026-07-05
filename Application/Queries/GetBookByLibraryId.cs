using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetBookByLibraryId
    {
        public record GetBookByLibraryIdQuery(Guid LibraryId, int Page, int PageSize) : IRequest<Result<PagenatedList<GetBookByLibraryIdResponse>>>;

        public class GetBookByLibraryIdHandler(
            IBookRepository bookRepository
            ) : IRequestHandler<GetBookByLibraryIdQuery, Result<PagenatedList<GetBookByLibraryIdResponse>>>
        {
            public async Task<Result<PagenatedList<GetBookByLibraryIdResponse>>> Handle(GetBookByLibraryIdQuery request, CancellationToken cancellationToken)
            {
                var books = await bookRepository.GetBookByLibraryIdAsync(request.LibraryId, new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize
                }, true);
                var bookData = books.Items.Where(x => !x.IsDeleted).Select(x => new GetBookByLibraryIdResponse(
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

                var pagedData = new PagenatedList<GetBookByLibraryIdResponse>
                {
                    Items = bookData,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = books.TotalCount
                };

                return Result<PagenatedList<GetBookByLibraryIdResponse>>.Success(pagedData, "Retrieved");
            }
        }

        public record GetBookByLibraryIdResponse(
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
