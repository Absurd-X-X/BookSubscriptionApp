using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IBookRepository
    {
        Task AddAsync(Book book);
        Task<Book?> GetByIdAsync(Guid id);
        Task<bool> IsExistAsync(string title, string author);
        Task<PagenatedList<Book>> GetBookByCategoryIdAsync(Guid categoryId, PageRequest request, bool usePaging);
        Task<PagenatedList<Book>> GetBookByLibraryIdAsync(Guid libraryId, PageRequest request, bool usePaging);
        Task<PagenatedList<Book>> GetAllAsync(PageRequest request, bool usePaging);
        Task<ICollection<Book>> GetBooksTitleAsync(string title);
        Task<ICollection<Book>> SearchBook(string searchText);
        Task<ICollection<Book>> GetBooksAuthorAsync(string author);
        void Update(Book book);
    }
}
