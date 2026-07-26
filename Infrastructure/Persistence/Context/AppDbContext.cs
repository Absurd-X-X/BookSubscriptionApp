using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.RegularExpressions;

namespace Infrastructure.Persistence.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<BookVersion> BookVersions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReadingListItem> ReadingListItems { get; set; }
        public DbSet<ReadingProgress> ReadingProgresses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserConversation> UserConversations { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }


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

            var conversationId = Guid.Parse("b235f2ed-bb4e-4bd2-a03d-0e3c17aaf2e2");

            var conversation = new Conversation
            {
                Id = conversationId,
                Title = "Soulshelf Group Chat",
                CreatedBy = user.Id.ToString()
            };

            var userConversation = new UserConversation
            {
                ConversationId = conversationId,
                UserId = user.Id
            };

            modelBuilder.Entity<User>().HasData(user);
            modelBuilder.Entity<Wallet>().HasData(wallet);
            modelBuilder.Entity<Conversation>().HasData(conversation);
            modelBuilder.Entity<UserConversation>().HasData(userConversation);
        }
    }
}
