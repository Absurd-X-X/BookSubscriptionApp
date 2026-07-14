using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class LibraryConfiguration : IEntityTypeConfiguration<Library>
    {
        public void Configure(EntityTypeBuilder<Library> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.PhoneNumber);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.RefNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(c => c.User)
                .WithOne(u => u.Library)
                .HasForeignKey<Library>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
