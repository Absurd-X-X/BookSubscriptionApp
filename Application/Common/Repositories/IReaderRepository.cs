using Application.Common.Pagenation;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Repositories
{
    public interface IReaderRepository
    {
        Task AddAsync(Reader reader);
        Task<Reader?> GetByIdAsync(Guid id);
        Task<Reader?> GetByEmailAsync(string email);
        Task<PagenatedList<Reader>> GetReadersAsync(PageRequest request, bool usePageing);
        Task UpdateReadingGoalAsync(
            Guid readerId,
            ReadingGoalType type,
            int target,
            DateTime? deadline,
            string? motivation);
    }
}
