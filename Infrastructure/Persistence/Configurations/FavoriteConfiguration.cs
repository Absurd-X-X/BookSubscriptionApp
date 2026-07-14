using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Reader)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Book)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
