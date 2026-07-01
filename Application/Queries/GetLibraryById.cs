using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetLibraryById
    {
        public record GetLibraryByIdQuery(Guid Id) : IRequest<Result<GetLibraryByIdResponse>>;
    
            public class GetLibraryByIdHandler(ILibraryRepository libraryRepository,
                IUserRepository userRepo
                ) : IRequestHandler<GetLibraryByIdQuery, Result<GetLibraryByIdResponse>>
            {
                public async Task<Result<GetLibraryByIdResponse>> Handle(GetLibraryByIdQuery request, CancellationToken cancellationToken)
                {
                    var library = await libraryRepository.GetAsync(request.Id);
    
                    if (library == null || library.IsDeleted)
                    {
                        return Result<GetLibraryByIdResponse>.Failure("Library not found");
                    }

                var user = await userRepo.GetAsync(library.Email);
                if (user == null || user.IsDeleted)
                {
                    return Result<GetLibraryByIdResponse>.Failure("Library not found");
                }

                var res = new GetLibraryByIdResponse(library.Id, library.Name, user.UserName, user.ImageUrl ?? "none", library.Email
                    , library.PhoneNumber, library.RefNumber, library.DateCreated, library.Books);
                return Result<GetLibraryByIdResponse>.Success(res, "Retrieved");
                }
            }
    
            public record GetLibraryByIdResponse(
                Guid Id,
                string Name,
                string UserName,
                string ImageUrl,
                string Email,
                string PhoneNumber,
                string RefNumber,
                DateTime DateCreated,
                IEnumerable<Book> Books
                );
    }
}
