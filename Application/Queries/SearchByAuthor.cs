using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class SearchByAythor
    {
        public record SearchByAuthorQuery(string Author) : IRequest<Result<IEnumerable<SearchByAuthorResponse>>>;

        public class SearchBookByTitleHandler(IBookRepository bookRepository) : IRequestHandler<SearchByAuthorQuery, Result<IEnumerable<SearchByAuthorResponse>>>
        {
            public async Task<Result<IEnumerable<SearchByAuthorResponse>>> Handle(SearchByAuthorQuery request, CancellationToken cancellationToken)
            {
                var books = await bookRepository.GetBooksAuthorAsync(request.Author);

                var booksData = books.Select(v => new SearchByAuthorResponse(
                    v.Id, v.Title
                    ));

                return Result<IEnumerable<SearchByAuthorResponse>>.Success(booksData, "Retrived");
            }
        }

        public record SearchByAuthorResponse(
            Guid BookId,
            string Title);
    }
}
