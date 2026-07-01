using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BankAccountRepository(AppDbContext context) : IBankAccountRepository
    {
        public async Task AddAsync(BankAccount bankAccount)

            => await context.BankAccounts.AddAsync(bankAccount);

        public async Task<ICollection<BankAccount>> GetAllAccountByUserAsync(Guid userId)

            => await context.BankAccounts
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.IsDefault)
                .ToListAsync();

        public async Task<BankAccount?> GetByIdAsync(Guid id)

            => await context.BankAccounts
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        public async Task<BankAccount?> GetByUserIdAsync(Guid userId)

            => await context.BankAccounts
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
        
    }
}
