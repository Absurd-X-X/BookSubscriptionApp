using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
    {
        public void Configure(EntityTypeBuilder<Bookmark> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Reader)
                .WithMany(x => x.Bookmarks)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Book)
                .WithMany(x => x.Bookmarks)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
