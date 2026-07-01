using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Author)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Isbn)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Pages)
                .IsRequired();
                        builder.Property(c => c.BookFileUrl)
                .IsRequired();

            builder.HasOne(x => x.Library)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.LibraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
