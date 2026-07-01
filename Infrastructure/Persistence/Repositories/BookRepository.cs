using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BookRepository(AppDbContext context) : IBookRepository
    {
        public async Task AddAsync(Book book)

            => await context.Books.AddAsync(book);

        public async Task<PagenatedList<Book>> GetAllAsync(PageRequest request, bool usePaging)
        {
            var query = context.Books.Where(x => !x.IsDeleted).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.NoOfTimeReadByPeople)
                    .Include(x => x.Category)
                    .Include(x => x.Library)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<Book>
                {
                    Items = await set.ToListAsync(), 
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Book>
            {
                Items = await query.Include(x => x.Category).Include(x => x.Library).ToListAsync(),
                TotalCount = totalCount
            };
        }


        public async Task<PagenatedList<Book>> GetBookByCategoryIdAsync(Guid categoryId, PageRequest request, bool usePaging)
        {
            var books = await context.Books.
                Where(x => x.CategoryId == categoryId && !x.IsDeleted).ToListAsync();

            var query = books.AsQueryable();
            var totalCount = await query.CountAsync(); 
            
            if (usePaging)
            {
                var set = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

                return new PagenatedList<Book>
                {
                    Items = set.Include(x => x.Category).Include(x => x.Library),
                    Page = request.Page,
                    PageSize= request.PageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Book>
            {
                Items = query.Include(x => x.Category).Include(x => x.Library),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<Book>> GetBookByLibraryIdAsync(Guid libraryId, PageRequest request, bool usePaging)
        {
            var books = await context.Books.
                Where(x => x.LibraryId == libraryId && !x.IsDeleted).ToListAsync();

            var query = books.AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                var set = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

                return new PagenatedList<Book>
                {
                    Items = set.Include(x => x.Category).Include(x => x.Library),
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Book>
            {
                Items = query.Include(x => x.Category).Include(x => x.Library),
                TotalCount = totalCount
            };
        }

        public async Task<Book?> GetByIdAsync(Guid id)

            => await context.Books.
                FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        public async Task<bool> IsExistAsync(string title, string author)

            => await context.Books
                .AnyAsync(x => x.Title == title && x.Author == author && !x.IsDeleted);

        public async Task<ICollection<Book>> GetBooksAuthorAsync(string author)
        {
            if (string.IsNullOrWhiteSpace(author)) return [];

            string search = author.Trim().ToLower();
            return await context.Books.Where(a => a.Author.Contains(search) && !a.IsDeleted).ToListAsync();
        }

        public async Task<ICollection<Book>> GetBooksTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return [];

            string search = title.Trim().ToLower();
            return await context.Books.Where(p => p.Title.Contains(search) && !p.IsDeleted).ToListAsync();
        }

        public void Update(Book book)

            => context.Update(book);

        public async Task<ICollection<Book>> SearchBook(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return [];

            string search = searchText.Trim().ToLower();
            return await context.Books.Where(p => (p.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) 

            || p.Author.Contains(search, StringComparison.CurrentCultureIgnoreCase) 

            || p.Category.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)) 

            && !p.IsDeleted).ToListAsync();
        }
    }
}
