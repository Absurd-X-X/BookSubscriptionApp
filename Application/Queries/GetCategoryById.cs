using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetCategoryById
    {
        public record GetCategoryByIdQuery(Guid Id) : IRequest<Result<GetCategoryByIdResponse>>;

        public class GetCategoryByIdHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoryByIdQuery, Result<GetCategoryByIdResponse>>
        {
            public async Task<Result<GetCategoryByIdResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
            {
                var category = await categoryRepository.GetCategoryAsync(request.Id);

                if (category == null)
                {
                    return Result<GetCategoryByIdResponse>.Failure("Category not found.");
                }

                return Result<GetCategoryByIdResponse>.Success(category.Adapt<GetCategoryByIdResponse>(), "Retrieved");
            }
        }

        public record GetCategoryByIdResponse(
            Guid CategoryId,
            string Name,
            string Description,
            string CreatedBy,
            DateTime DateCreated,
            DateTime DateModified,
            IEnumerable<Book> Books
            );
    }
}
