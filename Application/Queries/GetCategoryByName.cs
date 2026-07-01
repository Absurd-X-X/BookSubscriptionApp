using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetCategoryByName
    {
        public record GetCategoryByNameQuery(string Name) : IRequest<Result<IEnumerable<GetCategoryByNameResponse>>>;

        public class GetCategoryByNameHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoryByNameQuery, Result<IEnumerable<GetCategoryByNameResponse>>>
        {
            public async Task<Result<IEnumerable<GetCategoryByNameResponse>>> Handle(GetCategoryByNameQuery request, CancellationToken cancellationToken)
            {
                var categories = await categoryRepository.GetByNameAsync(request.Name);

                var cateData = categories.Select(c => new GetCategoryByNameResponse(
                    c.Id,
                    c.Name,
                    c.Description
                    ));

                return Result<IEnumerable<GetCategoryByNameResponse>>.Success(cateData, "Retrieved");
            }
        }

        public record GetCategoryByNameResponse(Guid CategoryId,
            string Name,
            string Description);
    }
}
