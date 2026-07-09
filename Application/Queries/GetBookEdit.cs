using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Query
{
    public class GetBookEdit
    {
        public record GetBookEditQuery(Guid BookId) : IRequest<Result<UpdateBookViewModel>>;

        public class GetBookEditHandler(
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository
            ) : IRequestHandler<GetBookEditQuery, Result<UpdateBookViewModel>>
        {
            public async Task<Result<UpdateBookViewModel>> Handle(GetBookEditQuery request, CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.BookId);

                if (book == null)
                    return Result<UpdateBookViewModel>.Failure("Book not found");

                var categories = await categoryRepository.GetAllCategoriesAsync();

                var model = new UpdateBookViewModel
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Subtitle = book.Subtitle,
                    Author = book.Author,
                    Genre = book.Genre,
                    Language = book.Language,
                    Isbn = book.Isbn,
                    About = book.About,
                    Publisher = book.Publisher,
                    PublicationYear = book.PublicationYear,
                    Edition = "1st Edition",
                    Pages = book.Pages,
                    FileType = book.FileType,
                    FileSize = "2.4 MB",
                    IsPublished = book.IsPublished,
                    AccessLevel = book.AccessLevel,
                    CategoryId = book.CategoryId,
                    AdditionalCategories = new List<string> { "Personal Development", "Habits", "Productivity" },
                    Tags = new List<string> { "habits", "self improvement", "productivity" },
                    PricingType = book.PricingType,
                    Price = book.Price,
                    CompareAtPrice = book.Price + 300,
                    Discount = book.Discount,
                    MembershipAccess = "AllUsers",
                    RentalOption = false,
                    AccessType = "AnyoneWithAccess",
                    AllowOnlineReading = true,
                    AllowDownload = book.AllowDownload,
                    AllowPrint = book.AllowPrint,
                    AllowCopyPaste = book.AllowCopyPaste,
                    LimitToOneDevice = false,
                    DripContent = false,
                    PreviewType = "FreePreviewChapters",
                    FreeChapterCount = 2,
                    PreviewChapters = new List<PreviewChapterDto>
                    {
                        new() { Number = 1, Title = "The Fundamentals" },
                        new() { Number = 2, Title = "The 1% Rule" }
                    },
                    AllowReviews = true,
                    AllowRatings = true,
                    AllowComments = true,
                    ShowReviewCount = true,
                    ShowReadCount = true,
                    ShowAverageRating = true,
                    DisableUserFeedback = false,
                    PromotionStatus = "Featured",
                    HomepageCarousel = true,
                    TrendingSection = true,
                    StaffPicks = false,
                    EditorsChoice = false,
                    ShowInSearch = true,
                    FeaturedBook = true,
                    ShowInRecommendations = true,
                    ShowInCategoryListings = true,
                    AllowSocialSharing = true,
                    HideFromCatalog = false,
                    AdultContentWarning = false,
                    SeoTitle = $"{book.Title} by {book.Author} | Best Selling {book.Genre} Book",
                    SeoSlug = book.Title.ToLower().Replace(" ", "-"),
                    SeoDescription = book.About.Length > 160 ? book.About.Substring(0, 160) : book.About,
                    SeoKeywords = new List<string> { book.Title.ToLower(), book.Author.ToLower(), book.Genre.ToLower() },
                    AverageRating = 4.9,
                    ReviewCount = 245,
                    TotalReads = 1245,
                    CompletionRate = 98,
                    AvgReadingTime = "2h 34m",
                    TotalRevenue = 98000,
                    CreatedOn = book.DateCreated,
                    DateModified = book.DateModified,
                    CreatedByName = "John Admin",
                    GenreOptions = new List<string> { "Self Help", "Fiction", "Non-Fiction", "Business", "Biography" },
                    LanguageOptions = new List<string> { "English", "French", "Spanish" },
                    Categories = categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList(),
                    Files = new List<BookFileDto>
                    {
                        new() { FileName = $"{book.Title} - Full Book.pdf", Version = "1.3", Type = "PDF", Size = "2.4 MB", UploadedOn = book.DateCreated, IsPublished = true, UploadedBy = "John Admin", Url = book.BookFileUrl }
                    },
                    CurrentFile = new BookFileDto
                    {
                        FileName = $"{book.Title}.{book.FileType.ToLower()}",
                        Version = "1.3",
                        Type = book.FileType,
                        Size = "2.4 MB",
                        UploadedOn = book.DateCreated,
                        IsPublished = true,
                        UploadedBy = "John Admin",
                        Url = book.BookFileUrl
                    },
                    FileStats = new FileStatsDto
                    {
                        TotalVersions = 5,
                        Downloads = 1245,
                        CurrentVersion = "1.3",
                        StorageUsed = "12.8 MB"
                    },
                    VersionHistory = new List<BookVersionDto>
                    {
                        new() { Version = "1.3", FileName = $"{book.Title}.epub", Type = "EPUB", Size = "2.4 MB", UploadedBy = "John Admin", UploadedOn = book.DateCreated, IsCurrent = true }
                    },
                    NextVersionNumber = "1.4"
                };

                return Result<UpdateBookViewModel>.Success(model, "Book fetched successfully");
            }
        }
    }
}