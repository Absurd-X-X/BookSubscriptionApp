using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetPersonalSettings
    {
        public record GetPersonalSettingsQuery(
            Guid UserId
            ) : IRequest<Result<GetPersonalSettingsResponse>>;

        public class GetPersonalSettingsHandler(
            IUserRepository userRepository,
            IReadingProgressRepository readingProgressRepository,
            ISubscriptionRepository subscriptionRepository
            ) : IRequestHandler<GetPersonalSettingsQuery, Result<GetPersonalSettingsResponse>>
        {
            async Task<Result<GetPersonalSettingsResponse>> IRequestHandler<GetPersonalSettingsQuery, Result<GetPersonalSettingsResponse>>.
                Handle(GetPersonalSettingsQuery request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user is null || user.Reader is null)
                {
                    return Result<GetPersonalSettingsResponse>.Failure("Reader not found");
                }

                var readerId = user.Reader.Id;

                var subscription = await subscriptionRepository.GetByReaderIdAsync(readerId, isActive: true);

                var completedCount = await readingProgressRepository.GetCompletedBookCountAsync(readerId);
                var currentStreak = await readingProgressRepository.GetMaxCurrentStreakAsync(readerId);

                const int annualGoalTarget = 35; 
                var booksLeft = Math.Max(0, annualGoalTarget - completedCount);
                var goalPercent = annualGoalTarget == 0 ? 0 : Math.Round((double)completedCount / annualGoalTarget * 100, 0);

                var profileDto = new ProfileDto(
                    request.UserId,
                    user.Reader.Name,
                    user.Email,
                    user.IsVerified,
                    user.UserName,
                    "A passionate reader exploring new ideas every day.",
                    user.ImageUrl,
                    user.Reader.DateCreated,
                    subscription != null,
                    subscription == null ? "Free Plan" : subscription.Types.TypeName
                );

                var preferencesDto = new PreferencesDto(
                    "dark",   
                    "en",    
                    "grid"    
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

                var accountOverviewDto = new AccountOverviewDto(
                    subscription != null,
                    subscription == null ? "Free Plan" : subscription.Types.TypeName,
                    user.Reader.DateCreated,
                    completedCount,
                    currentStreak
                );

                var readingStatsDto = new ReadingStatsDto(
                    annualGoalTarget,
                    completedCount,
                    booksLeft,
                    goalPercent
                );

                var response = new GetPersonalSettingsResponse(
                    profileDto,
                    preferencesDto,
                    securityDto,
                    notificationPrefsDto,
                    accountOverviewDto,
                    readingStatsDto
                );

                return Result<GetPersonalSettingsResponse>.Success(response, "Retrieved");
            }
        }

        public record ProfileDto(
            Guid UserId,
            string FullName,
            string Email,
            bool IsEmailVerified,
            string Username,
            string Bio,
            string? AvatarUrl,
            DateTime MemberSince,
            bool IsPremiumMember,
            string PlanName
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

        public record AccountOverviewDto(
            bool IsPremiumMember,
            string PlanName,
            DateTime MemberSince,
            int TotalBooksRead,
            int ReadingStreakDays
        );

        public record ReadingStatsDto(
            int BooksGoal,
            int BooksRead,
            int BooksLeft,
            double PercentComplete
        );

        public record GetPersonalSettingsResponse(
            ProfileDto Profile,
            PreferencesDto Preferences,
            SecurityDto Security,
            NotificationPreferencesDto NotificationPreferences,
            AccountOverviewDto AccountOverview,
            ReadingStatsDto ReadingStats
        );
    }
}