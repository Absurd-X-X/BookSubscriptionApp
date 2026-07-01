using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class WalletRepository(AppDbContext context) : IWalletRepository
    {
        public async Task AddAsync(Wallet wallet)

            => await context.Wallets.AddAsync(wallet);

        public async Task<Wallet?> GetAsync(Guid id)

            => await context.Wallets
                .Include(x => x.User)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        public async Task<Wallet?> GetByUserIdAsync(Guid userId)
            => await context.Wallets
                .Include(x => x.User)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(User => User.UserId == userId && !User.IsDeleted);
    }
}
