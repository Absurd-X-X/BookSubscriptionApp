using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.HelpfulCount);

            builder.Property(x => x.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Rating);

            builder.Property(x => x.Status);
            
            builder.HasOne(x => x.Reader)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Book)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}