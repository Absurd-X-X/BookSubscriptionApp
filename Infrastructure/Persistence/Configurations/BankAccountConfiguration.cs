using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AccountName)
                .IsRequired().
                HasMaxLength(100);


            builder.Property(x => x.AccountNumber)
                .IsRequired().
                HasMaxLength(20);


            builder.Property(x => x.RecipientCode)
                .HasMaxLength(50);


            builder.Property(x => x.BankName)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(x => x.BankCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(x => x.User)
                .WithMany(x => x.BankAccounts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
