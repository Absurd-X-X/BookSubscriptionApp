using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllCategories
    {
        public record GetAllCategoryQuery(int Page, int PageSize) : IRequest<Result<PagenatedList<GetAllCategoryResponse>>>;

        public class GetAllCategoryHandler(ICategoryRepository categoryRepository) : 
            IRequestHandler<GetAllCategoryQuery, Result<PagenatedList<GetAllCategoryResponse>>>
        {
            public async Task<Result<PagenatedList<GetAllCategoryResponse>>> Handle(GetAllCategoryQuery request, CancellationToken cancellationToken)
            {
                var categories = await categoryRepository.GetCategoriesAsync(true, new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                });

                var categoryData = categories.Items.Where(x => !x.IsDeleted).
                    Select(x => new GetAllCategoryResponse(
                        x.Id,
                        x.Name,
                        x.Description,
                        x.DateCreated,
                        x.Books.Count,
                        x.Books.Count * 2
                        )).ToList();

                var pagedCategories = new PagenatedList<GetAllCategoryResponse>
                {
                    Items = categoryData,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = categoryData.Count
                };

                return Result<PagenatedList<GetAllCategoryResponse>>.Success(pagedCategories, "Retrieved");
            }
        }

        public record GetAllCategoryResponse(
            Guid CategoryId,
            string Name,
            string Description,
            DateTime DateCreated,
            int BooksCount,
            int Engagement
            );
    }
}
