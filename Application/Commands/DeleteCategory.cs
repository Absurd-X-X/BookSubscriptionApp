using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Commands
{
    public class DeleteCategory
    {
        public record DeleteCategoryCommand(Guid Id) : IRequest<Result<string>>;

        public class DeleteCategoryHandler(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
            {
                var category = await categoryRepository.GetCategoryAsync(request.Id);

                if (category == null)
                    return Result<string>.Failure("Category not found");
                    
                category.IsDeleted = true;
                category.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();

                return Result<string>.Success("Category", "Deleted");
            }
        }
    }
}
