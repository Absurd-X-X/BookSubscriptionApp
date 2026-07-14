using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReadingListItemConfiguration : IEntityTypeConfiguration<ReadingListItem>
    {
        public void Configure(EntityTypeBuilder<ReadingListItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Reader)
                .WithMany(x => x.Readings)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Book)
                .WithMany(x => x.ReadingListItems)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
