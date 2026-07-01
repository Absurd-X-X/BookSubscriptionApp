using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetAllLibrary
    {
        public record GetAllLibraryQuery(
            int Page,
            int PageSize
            ) : IRequest<Result<PagenatedList<GetAllLibraryResponse>>>;

        public class GetAllLibraryHandler(
            ILibraryRepository libraryRepository
            ) : IRequestHandler<GetAllLibraryQuery, Result<PagenatedList<GetAllLibraryResponse>>>
        {
            public async Task<Result<PagenatedList<GetAllLibraryResponse>>> Handle(GetAllLibraryQuery request, CancellationToken cancellationToken)
            {
                PageRequest page = new()
                {
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                var libraries = await libraryRepository.GetAllAsync(page, true);

                var libraryData = libraries.Items.Where(x => !x.IsDeleted)
                    .Select(x => new GetAllLibraryResponse(
                        x.Id,
                        x.Name,
                        x.Email,
                        x.PhoneNumber,
                        x.RefNumber,
                        x.DateCreated,
                        x.IsDeleted,
                        x.Books.Where(a => !a.IsDeleted && a.LibraryId == x.Id)
                        )).ToList();

                var paginatedList = new PagenatedList<GetAllLibraryResponse>
                {
                    Items = libraryData,
                    TotalCount = libraries.TotalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                return Result<PagenatedList<GetAllLibraryResponse>>.Success(paginatedList, "Retrieved Successfully");
            }
        }

        public record GetAllLibraryResponse(
            Guid LibraryId,
            string Name,
            string Email,
            string PhoneNumber,
            string RefNumber,
            DateTime DateCreated,
            bool IsActive,
            IEnumerable<Book> Books
            );
    }
}
