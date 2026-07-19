using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReadingProgressConfiguration : IEntityTypeConfiguration<ReadingProgress>
    {
        public void Configure(EntityTypeBuilder<ReadingProgress> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProgressPercentage)
                .IsRequired().
                HasMaxLength(100);


            builder.Property(x => x.CurrentChapter).
                HasMaxLength(20);


            builder.Property(x => x.CurrentPage)
                .IsRequired();


            builder.Property(x => x.IsCompleted);


            builder.Property(x => x.LastReadDate);


            builder.Property(x => x.IsDeleted);

            builder.HasOne(x => x.Reader)
                .WithMany(x => x.ReadingProgresses)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Book)
                .WithMany(x => x.ReadingProgresses)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
