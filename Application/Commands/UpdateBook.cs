using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class UpdateBook
    {
        public record UpdateBookCommand(
            Guid BookId,
            // Basic Information
            string Title,
            string? Subtitle,
            string Author,
            string Genre,
            string Language,
            string Isbn,
            string About,
            // Book Details
            string Publisher,
            int PublicationYear,
            string Edition,
            int Pages,
            // Book Status
            bool IsPublished,
            string AccessLevel,
            // Categories & Tags
            Guid CategoryId,
            List<string> AdditionalCategories,
            List<string> Tags,
            // Pricing Model
            string PricingType,
            decimal Price,
            decimal? CompareAtPrice,
            decimal Discount,
            // Membership & Access Control
            string MembershipAccess, // "AllUsers" | "SpecificPlans" | "VipOnly"
            bool RentalOption,
            // Access Settings
            string AccessType,
            bool AllowOnlineReading,
            bool AllowDownload,
            bool LimitToOneDevice,
            bool DripContent,
            // Chapters Preview Settings
            string PreviewType,
            int FreeChapterCount,
            // Reader Interaction
            bool AllowReviews,
            bool AllowRatings,
            bool AllowComments,
            bool ShowReviewCount,
            bool ShowReadCount,
            bool ShowAverageRating,
            bool DisableUserFeedback,
            // Discovery & Promotion
            string PromotionStatus, // "Standard" | "Featured" | "Sponsored"
            bool HomepageCarousel,
            bool TrendingSection,
            bool StaffPicks,
            bool EditorsChoice,
            // Visibility Settings
            bool ShowInSearch,
            bool FeaturedBook,
            bool ShowInRecommendations,
            bool ShowInCategoryListings,
            bool AllowSocialSharing,
            bool HideFromCatalog,
            bool AdultContentWarning,
            // SEO & Settings (mocked)
            string SeoTitle,
            string SeoSlug,
            string SeoDescription,
            List<string> SeoKeywords
            ) : IRequest<Result<string>>;

        public class UpdateBookHandler(
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository
            ) : IRequestHandler<UpdateBookCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.BookId);
                var userId = currentUser.GetCurrentUser();
                var user = await userRepository.GetAsync(userId);

                if (user == null)
                    return Result<string>.Failure("User not found");

                if (book == null)
                    return Result<string>.Failure("Book not found");

                if (Guid.Parse(book.CreatedBy) != user.Id)
                    return Result<string>.Failure("Unauthorized to do this");

                // ---- Fields that map to real Book entity properties ----
                book.Title = request.Title;
                book.Subtitle = request.Subtitle;
                book.Author = request.Author;
                book.Genre = request.Genre;
                book.Language = request.Language;
                book.Isbn = request.Isbn;
                book.About = request.About;
                book.Publisher = request.Publisher;
                book.PublicationYear = request.PublicationYear;
                book.Pages = request.Pages;
                book.IsPublished = request.IsPublished;
                book.AccessLevel = request.AccessLevel;
                book.CategoryId = request.CategoryId;
                book.PricingType = request.PricingType;
                book.Price = request.PricingType == "Free" ? 0 : request.Price;
                book.Discount = request.Discount;
                book.AllowDownload = request.AllowDownload;
                book.DateModified = DateTime.UtcNow;

                // ---- Fields below are MOCKED (not persisted) ----
                // Edition, AdditionalCategories, Tags, CompareAtPrice,
                // MembershipAccess, RentalOption, AccessType, AllowOnlineReading,
                // LimitToOneDevice, DripContent, PreviewType, FreeChapterCount,
                // AllowReviews, AllowRatings, AllowComments, ShowReviewCount,
                // ShowReadCount, ShowAverageRating, DisableUserFeedback,
                // PromotionStatus, HomepageCarousel, TrendingSection, StaffPicks,
                // EditorsChoice, ShowInSearch, FeaturedBook, ShowInRecommendations,
                // ShowInCategoryListings, AllowSocialSharing, HideFromCatalog,
                // AdultContentWarning, SeoTitle, SeoSlug, SeoDescription, SeoKeywords
                // These do not exist on the Book entity yet — add columns/tables
                // later if you want them persisted (e.g. Tags/AdditionalCategories
                // as many-to-many, SEO as a separate BookSeo table).

                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();

                var audit = new AuditLog
                {
                    ActionType = "Update",
                    Description = $"{book.Title} was updated successfully",
                    Icon = "📚",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.Book,
                    ResourceId = book.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");
            }
        }
    }
}