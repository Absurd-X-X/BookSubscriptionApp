using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IWalletRepository
    {
        Task AddAsync(Wallet wallet);
        Task<Wallet?> GetAsync(Guid id);
        Task<Wallet?> GetByUserIdAsync(Guid userId);
    }
}
