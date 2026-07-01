using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class SearchBookByTitle
    {
        public record SearchByTitleQuery(string Title) : IRequest<Result<IEnumerable<SearchBookByTitleResponse>>>;

        public class SearchBookByTitleHandler(IBookRepository bookRepository) : IRequestHandler<SearchByTitleQuery, Result<IEnumerable<SearchBookByTitleResponse>>>
        {
            public async Task<Result<IEnumerable<SearchBookByTitleResponse>>> Handle(SearchByTitleQuery request, CancellationToken cancellationToken)
            {
                var books = await bookRepository.GetBooksTitleAsync(request.Title);

                var booksData = books.Select(v => new SearchBookByTitleResponse(
                    v.Id, v.Title
                    ));

                return Result<IEnumerable <SearchBookByTitleResponse>>.Success(booksData, "Retrived");
            }
        }

        public record SearchBookByTitleResponse(
            Guid BookId,
            string Title);
    }
}
