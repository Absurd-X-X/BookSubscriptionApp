using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetLibrarySettings
    {
        public record GetLibrarySettingsQuery(
            Guid UserId
            ) : IRequest<Result<GetLibrarySettingsResponse>>;

        public class GetLibrarySettingsHandler(
            IUserRepository userRepository
            ) : IRequestHandler<GetLibrarySettingsQuery, Result<GetLibrarySettingsResponse>>
        {
            async Task<Result<GetLibrarySettingsResponse>> IRequestHandler<GetLibrarySettingsQuery, Result<GetLibrarySettingsResponse>>.
                Handle(GetLibrarySettingsQuery request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user is null || user.Library is null)
                {
                    return Result<GetLibrarySettingsResponse>.Failure("User not found");
                }




                var profileDto = new LibraryAdminProfileDto(
                    request.UserId,
                    user.Library.Name, 
                    user.Email,
                    user.IsVerified,
                    user.UserName,
                    "Library administrator managing the catalog and readers.",
                    user.ImageUrl,
                    user.Library.DateCreated
                );

                var libraryDto = new LibraryDto(
                    user.Library.Id,
                    user.Library.Name,
                    user.Library.Email,
                    user.Library.PhoneNumber,
                    user.Library.RefNumber,
                    string.Empty,
                    string.Empty,
                    "A modern digital library focused on personal growth, productivity, and lifelong learning.", // MOCK — no Description field yet
                    user.Library.DateCreated
                );

                var preferencesDto = new PreferencesDto(
                    "dark",   // MOCK — no Theme field yet
                    "en",     // MOCK — no Language field yet
                    "grid"    // MOCK — no DefaultView field yet
                );

                var securityDto = new SecurityDto(
                    true, // MOCK — no 2FA field yet
                    3     // MOCK — no session-tracking table yet
                );

                var notificationPrefsDto = new NotificationPreferencesDto(
                    true,  // MOCK
                    true,  // MOCK
                    true,  // MOCK
                    false  // MOCK
                );

                var accountOverviewDto = new LibraryAccountOverviewDto(
                    user.Library.DateCreated
                );

                var response = new GetLibrarySettingsResponse(
                    profileDto,
                    libraryDto,
                    preferencesDto,
                    securityDto,
                    notificationPrefsDto,
                    accountOverviewDto
                );

                return Result<GetLibrarySettingsResponse>.Success(response, "Retrieved");
            }
        }

        public record LibraryAdminProfileDto(
            Guid UserId,
            string FullName,
            string Email,
            bool IsEmailVerified,
            string Username,
            string Bio,
            string? AvatarUrl,
            DateTime MemberSince
        );

        public record LibraryDto(
            Guid LibraryId,
            string Name,
            string Email,
            string PhoneNumber,
            string RefNumber,
            string Address,
            string Website,
            string Description,
            DateTime DateCreated
        );

        public record PreferencesDto(
            string Theme,
            string Language,
            string DefaultView
        );

        public record SecurityDto(
            bool TwoFactorEnabled,
            int ActiveSessionsCount
        );

        public record NotificationPreferencesDto(
            bool EmailEnabled,
            bool PushEnabled,
            bool NewBookAlertsEnabled,
            bool RemindersEnabled
        );

        public record LibraryAccountOverviewDto(
            DateTime MemberSince
        );


        public record GetLibrarySettingsResponse(
            LibraryAdminProfileDto Profile,
            LibraryDto Library,
            PreferencesDto Preferences,
            SecurityDto Security,
            NotificationPreferencesDto NotificationPreferences,
            LibraryAccountOverviewDto AccountOverview
        );
    }

    
}