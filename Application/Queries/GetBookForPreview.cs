using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetBookForPreview
    {
        public record GetBookForPreviewQuery(Guid BookId)
            : IRequest<Result<GetBookForPreviewResponse>>;

        public class GetBookForPreviewHandler(IBookRepository bookRepository)
            : IRequestHandler<GetBookForPreviewQuery, Result<GetBookForPreviewResponse>>
        {
            public async Task<Result<GetBookForPreviewResponse>> Handle(
                GetBookForPreviewQuery request,
                CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.BookId);

                if (book == null)
                    return Result<GetBookForPreviewResponse>.Failure("Book not found.");

                var response = new GetBookForPreviewResponse(
                    book.Id,
                    book.Title,
                    book.Subtitle,
                    book.Author,
                    book.BookCoverUrl,
                    book.BookFileUrl,
                    book.FileType
                );

                return Result<GetBookForPreviewResponse>.Success(response, "Book retrieved for preview");
            }
        }

        public record GetBookForPreviewResponse(
            Guid Id,
            string Title,
            string? Subtitle,
            string Author,
            string BookCoverUrl,
            string BookFileUrl,
            string FileType
        );
    }
}