using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Balance)
                .IsRequired();

            builder.Property(x => x.BalanceAfter)
                .IsRequired();

            builder.Property(x => x.BalanceBefore)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(100);

            builder.Property(x => x.PaystackReference)
                .HasMaxLength(100);

            builder.Property(c => c.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(x => x.Wallet)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
