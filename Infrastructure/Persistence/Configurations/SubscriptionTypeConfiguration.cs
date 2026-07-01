using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SubscriptionTypeConfiguration : IEntityTypeConfiguration<SubscriptionType>
    {
        public void Configure(EntityTypeBuilder<SubscriptionType> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TypeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Cycle)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Cost)
                .IsRequired();

        }
    }
}
