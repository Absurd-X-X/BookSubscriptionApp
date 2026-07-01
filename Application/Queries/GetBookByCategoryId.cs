using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Mapster;
using MediatR;
using MySqlX.XDevAPI.Common;

namespace Application.Queries
{
    public class GetBookByCategoryId
    {
        public record GetBookByCategoryIdQuery(Guid CategoryId) : IRequest<Result<GetBookByCategoryIdResponse>>;

        public class GetBookByCategoryIdHandler(
            ICategoryRepository categoryRepository
            ) : IRequestHandler<GetBookByCategoryIdQuery, Result<GetBookByCategoryIdResponse>>
        {
            public async Task<Result<GetBookByCategoryIdResponse>> Handle(GetBookByCategoryIdQuery request, CancellationToken cancellationToken)
            {
                var category = await categoryRepository.GetCategoryAsync(request.CategoryId);
                if (category is null)
                    return Result<GetBookByCategoryIdResponse>.Failure("Not found");

                return Result<GetBookByCategoryIdResponse>.Success(category.Adapt<GetBookByCategoryIdResponse>(), "Retrieved");
            }
        }

        public record GetBookByCategoryIdResponse(Guid Id, string Name, string Description, IEnumerable<Book> Books);
    }
}
