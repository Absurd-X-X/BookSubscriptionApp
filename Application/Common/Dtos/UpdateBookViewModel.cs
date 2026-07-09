namespace Application.Common.Dtos
{
    public class UpdateBookViewModel
    {
        public Guid BookId { get; set; }
        public string Title { get; set; } = default!;
        public string? Subtitle { get; set; }
        public string Author { get; set; } = default!;
        public string Genre { get; set; } = default!;
        public string Language { get; set; } = default!;
        public string Isbn { get; set; } = default!;
        public string About { get; set; } = default!;
        public string Publisher { get; set; } = default!;
        public int PublicationYear { get; set; }
        public string Edition { get; set; } = default!;
        public int Pages { get; set; }
        public string FileType { get; set; } = default!;
        public string FileSize { get; set; } = default!;
        public bool IsPublished { get; set; }
        public string AccessLevel { get; set; } = default!;
        public Guid CategoryId { get; set; }
        public List<string> AdditionalCategories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string PricingType { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public decimal Discount { get; set; }
        public string MembershipAccess { get; set; } = default!;
        public bool RentalOption { get; set; }
        public string AccessType { get; set; } = default!;
        public bool AllowOnlineReading { get; set; }
        public bool AllowDownload { get; set; }
        public bool AllowPrint { get; set; }
        public bool AllowCopyPaste { get; set; }
        public bool LimitToOneDevice { get; set; }
        public bool DripContent { get; set; }
        public string PreviewType { get; set; } = default!;
        public int FreeChapterCount { get; set; }
        public List<PreviewChapterDto> PreviewChapters { get; set; } = new();
        public bool AllowReviews { get; set; }
        public bool AllowRatings { get; set; }
        public bool AllowComments { get; set; }
        public bool ShowReviewCount { get; set; }
        public bool ShowReadCount { get; set; }
        public bool ShowAverageRating { get; set; }
        public bool DisableUserFeedback { get; set; }
        public string PromotionStatus { get; set; } = default!;
        public bool HomepageCarousel { get; set; }
        public bool TrendingSection { get; set; }
        public bool StaffPicks { get; set; }
        public bool EditorsChoice { get; set; }
        public bool ShowInSearch { get; set; }
        public bool FeaturedBook { get; set; }
        public bool ShowInRecommendations { get; set; }
        public bool ShowInCategoryListings { get; set; }
        public bool AllowSocialSharing { get; set; }
        public bool HideFromCatalog { get; set; }
        public bool AdultContentWarning { get; set; }
        public string SeoTitle { get; set; } = default!;
        public string SeoSlug { get; set; } = default!;
        public string SeoDescription { get; set; } = default!;
        public List<string> SeoKeywords { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalReads { get; set; }
        public int CompletionRate { get; set; }
        public string AvgReadingTime { get; set; } = default!;
        public decimal TotalRevenue { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime DateModified { get; set; }
        public string CreatedByName { get; set; } = default!;
        public List<string> GenreOptions { get; set; } = new();
        public List<string> LanguageOptions { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<BookFileDto> Files { get; set; } = new();
        public BookFileDto CurrentFile { get; set; } = new();
        public FileStatsDto FileStats { get; set; } = new();
        public List<BookVersionDto> VersionHistory { get; set; } = new();
        public string NextVersionNumber { get; set; } = default!;
    }

    public class PreviewChapterDto
    {
        public int Number { get; set; }
        public string Title { get; set; } = default!;
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
    }

    public class BookFileDto
    {
        public string FileName { get; set; } = default!;
        public string Version { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Size { get; set; } = default!;
        public DateTime UploadedOn { get; set; }
        public bool IsPublished { get; set; }
        public string UploadedBy { get; set; } = default!;
        public string Url { get; set; } = default!;
    }

    public class FileStatsDto
    {
        public int TotalVersions { get; set; }
        public int Downloads { get; set; }
        public string CurrentVersion { get; set; } = default!;
        public string StorageUsed { get; set; } = default!;
    }

    public class BookVersionDto
    {
        public string Version { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Size { get; set; } = default!;
        public string UploadedBy { get; set; } = default!;
        public DateTime UploadedOn { get; set; }
        public bool IsCurrent { get; set; }
    }
}