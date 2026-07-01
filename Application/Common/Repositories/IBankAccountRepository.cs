using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IBankAccountRepository
    {
        Task AddAsync(BankAccount bankAccount);
        Task<BankAccount?> GetByIdAsync(Guid id);
        Task<BankAccount?> GetByUserIdAsync(Guid userId);
        Task<ICollection<BankAccount>> GetAllAccountByUserAsync(Guid userId);
    }
}
