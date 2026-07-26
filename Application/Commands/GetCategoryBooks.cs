using Application.Common.Dtos;
using Application.Common.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetCategoryBooks
    {
        public record GetCategoryBooksQuery(Guid CategoryId) : IRequest<Result<GetCategoryBooksResponse>>;

        public class GetBookByCategoryIdHandler(
            ICategoryRepository categoryRepository
        ) : IRequestHandler<GetCategoryBooksQuery, Result<GetCategoryBooksResponse>>
        {
            public async Task<Result<GetCategoryBooksResponse>> Handle(
                GetCategoryBooksQuery request,
                CancellationToken cancellationToken)
            {
                var category = await categoryRepository.GetCategoryAsync(request.CategoryId);
                if (category is null)
                    return Result<GetCategoryBooksResponse>.Failure("Category not found");

                return Result<GetCategoryBooksResponse>.Success(
                    category.Adapt<GetCategoryBooksResponse>(), "Retrieved");
            }
        }

        public record GetCategoryBooksResponse(
            Guid Id,
            string Name,
            string Description,
            IEnumerable<BookDto> Books
        );

        public record BookDto(
            Guid Id,
            string Title,
            string? Subtitle,
            string Author,
            string BookCoverUrl,
            string PricingType,
            decimal Price,
            string Genre,
            string About,
            string FileType,
            bool IsPublished,
            bool AllowDownload,
            int NoOfTimeReadByPeople,
            double AverageRating,
            int ReviewCount
        );
    }
}