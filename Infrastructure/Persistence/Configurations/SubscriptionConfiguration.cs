using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Reader)
                .WithMany(r => r.Subscriptions)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Types)
                .WithMany(r => r.Subscriptions)
                .HasForeignKey(x => x.SubscriptionTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
