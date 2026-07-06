namespace Domain.Entities
{
    public class BookVersion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BookId { get; set; }
        public Book Book { get; set; } = default!;
        public string VersionNumber { get; set; } = default!; // e.g. "1.0"
        public string FileUrl { get; set; } = default!;
        public string FileType { get; set; } = default!;
        public long FileSizeBytes { get; set; }
        public string? ReleaseNote { get; set; }
        public string UploadedBy { get; set; } = default!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow; 
        public string MimeType { get; set; } = default!;
        public bool IsCurrent { get; set; }
    }
}