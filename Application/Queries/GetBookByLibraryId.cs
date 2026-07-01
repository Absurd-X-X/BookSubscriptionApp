using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetBookByLibraryId
    {
        public record GetBookByLibraryIdQuery(Guid Id) : IRequest<Result<GetBookByLibraryIdResponse>>;

        public class GetBookByLibraryIdHandler(
            ILibraryRepository libraryRepository
            ) : IRequestHandler<GetBookByLibraryIdQuery, Result<GetBookByLibraryIdResponse>>
        {
            public async Task<Result<GetBookByLibraryIdResponse>> Handle(GetBookByLibraryIdQuery request, CancellationToken cancellationToken)
            {
                var library = await libraryRepository.GetAsync(request.Id);

                if (library == null)
                    return Result<GetBookByLibraryIdResponse>.Failure("Not found");

                return Result<GetBookByLibraryIdResponse>.Success(library.Adapt<GetBookByLibraryIdResponse>(), "Retrieved");
            }
        }

        public record GetBookByLibraryIdResponse(
            Guid LibraryId,
            string Email,
            string PhoneNumber,
            string RefNumber,
            DateTime DateCreated,
            IEnumerable<Book> Books
            );
    }
}
