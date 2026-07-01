using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class DeleteLibrary
    {
        public record DeleteLibraryCommand(Guid LibraryId) : IRequest<Result<string>>;

        public class DeleteLibraryHandler(
            ILibraryRepository libraryRepository,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<DeleteLibraryCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteLibraryCommand request, CancellationToken cancellationToken)
            {
                var library = await libraryRepository.GetAsync(request.LibraryId);

                if (library is null)
                    return Result<string>.Failure("Library not found");

                library.IsDeleted = true;
                library.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Deleted", "Successfully");
            }
        }
    }
}
