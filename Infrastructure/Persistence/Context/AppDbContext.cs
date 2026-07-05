using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<ReadingProgress> ReadingProgresses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserConversation> UserConversations { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);

            var user = new User
            {
                Id = Guid.Parse("c117635d-96e0-409b-9fae-72976ec9c42a"),
                Email = "admin@gmail.com",
                Role = "admin",
                CreatedBy = "system",
                UserName = "admin",
                IsVerified = true
            };

            string password = $"admin123";
            user.HashPassword = new PasswordHasher<User>().HashPassword(user, password);

            var wallet = new Wallet
            {
                UserId = user.Id,
                CreatedBy = user.Email,
                Balance = 0,
            };

            modelBuilder.Entity<User>().HasData(user);
            modelBuilder.Entity<Wallet>().HasData(wallet);
        }
    }
}
