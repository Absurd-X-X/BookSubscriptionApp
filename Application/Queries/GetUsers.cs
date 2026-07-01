using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetUsers
    {
        public record GetUsersQuery(int Page, int PageSize, bool UsePaging) : IRequest<Result<PagenatedList<GetUsersResponse>>>;

        public class GetUsersHandler(IUserRepository userRepository) : IRequestHandler<GetUsersQuery, Result<PagenatedList<GetUsersResponse>>>
        {
            public async Task<Result<PagenatedList<GetUsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
            {
                var users = await userRepository.GetUsersAsync(new PageRequest{
                    Page = request.Page,
                    PageSize = request.PageSize
                }, request.UsePaging);

                var response = users.Items.Select(u => new GetUsersResponse(
                    u.Id,
                    u.ImageUrl ?? "none",
                    u.UserName,
                    u.Reader?.Name ?? u.Library?.Name! ?? "Administator",
                    u.Email,
                    u.Role,
                    u.DateCreated,
                    u.IsDeleted
                ));

                var responses = new PagenatedList<GetUsersResponse>
                {
                    Items = response,
                    TotalCount = users.TotalCount,
                    Page = users.Page,
                    PageSize = users.PageSize
                };

                return Result<PagenatedList<GetUsersResponse>>.Success(responses, "Retrieved successfully");
            }
        }
        public record GetUsersResponse(
            Guid UserId,
            string ImageUrl,
            string UserName,
            string FullName,
            string Email,
            string Role,
            DateTime DateJoined,
            bool Status
            );
    }
}
