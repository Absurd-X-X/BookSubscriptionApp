using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface ILibraryRepository
    {
        Task AddAsync(Library library);
        Task<Library?> GetAsync(Guid id);
        Task<Library?> GetLibraryAsync(string email);
        Task<PagenatedList<Library>> GetAllAsync(PageRequest request, bool usePaging);
    }
}
