using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<User?> GetAsync(Guid id);
        Task<User?> GetAsync(string email);
        Task<User?> GetByUserNameAsync(string userName);
        Task<bool> IsExistAsync(string email);
        void Update(User user);
        Task<PagenatedList<User>> GetUsersAsync(PageRequest pageRequest, bool usePaging);
    }
}
