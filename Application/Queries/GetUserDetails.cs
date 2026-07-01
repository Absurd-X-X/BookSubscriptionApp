using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetUserDetails
    {
        public record GetUserDetailsQuery(Guid UserId) : IRequest<Result<GetUserDetailsResponse>>;

        public class GetUserDetailsHandler(IUserRepository userRepository) : IRequestHandler<GetUserDetailsQuery, Result<GetUserDetailsResponse>>
        {
            public async Task<Result<GetUserDetailsResponse>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user is null)
                    return Result<GetUserDetailsResponse>.Failure("User with the inserted id not found");

                return Result<GetUserDetailsResponse>.Success(new GetUserDetailsResponse(
                    user.Id,
                    user.Reader?.Name ?? user.Library?.Name ?? "Administrator",
                    user.ImageUrl ?? "none",
                    user.UserName ?? "User",
                    user.Role,
                    user.Email,
                    user.DateCreated,
                    user.IsDeleted
                    ), "Retrieved");
            }
        }

        public record GetUserDetailsResponse(Guid UserId,
            string? FullName,
            string ImageUrl,
            string UserName,
            string Role,
            string Email,
            DateTime DateJoined,
            bool Status);
    }
}
