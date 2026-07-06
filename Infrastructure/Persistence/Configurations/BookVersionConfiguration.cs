using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookVersionConfiguration : IEntityTypeConfiguration<BookVersion>
    {
        public void Configure(EntityTypeBuilder<BookVersion> builder)
        {
            builder.HasKey(x => x.Id);


            builder.Property(x => x.FileType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.FileUrl)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.UploadedBy)
                .IsRequired();

            builder.Property(c => c.FileSizeBytes);

            builder.HasOne(x => x.Book)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
