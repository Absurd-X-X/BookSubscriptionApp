using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetCategories
    {
        public record GetCategoriesQuery() : IRequest<Result<List<GetCategoriesResponse>>>;

        public class GetCategoriesHandler(
            ICategoryRepository categoryRepository
            ) : IRequestHandler<GetCategoriesQuery, Result<List<GetCategoriesResponse>>>
        {
            public async Task<Result<List<GetCategoriesResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
            {
                var categories = await categoryRepository.GetAllCategoriesAsync();

                var data = categories
                    .Select(c => new GetCategoriesResponse(c.Id, c.Name))
                    .ToList();

                return Result<List<GetCategoriesResponse>>.Success(data, "Retrieved");
            }
        }

        public record GetCategoriesResponse(Guid Id, string Name);
    }
}