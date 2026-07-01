using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class UpdateCategory
    {
        public record UpdateCategoryCommand(
            Guid CategoryId,
            string Name,
            string Description
            ) : IRequest<Result<string>>;

        public class UpdateCategoryHandler(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<UpdateCategoryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
            {
                var category = await categoryRepository.GetCategoryAsync(request.CategoryId);

                if (category == null)
                    return Result<string>.Failure("Not found");

                category.Name = request.Name;
                category.Description = request.Description;
                category.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");

            }
        }
    }
}
