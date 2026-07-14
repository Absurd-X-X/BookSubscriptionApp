using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task AddAsync(User user)

            => await context.Users.AddAsync(user);
        public async Task<User?> GetAsync(Guid id)

            => await context.Users
                .Include(x => x.Wallet)
                .Include(x => x.Reader)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<User?> GetAsync(string email)

            => await context.Users
                .Include(x => x.Wallet)
                .FirstOrDefaultAsync(x => x.Email == email);

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await context.Users
                .Include(x => x.Reader)
                .FirstOrDefaultAsync(c => c.UserName == userName && c.IsDeleted);
        }


        public async Task<PagenatedList<User>> GetUsersAsync(PageRequest pageRequest, bool usePaging)
        {
            var query = context.Users.Include(x => x.Wallet)
                .Include(x => x.Reader)
                .AsQueryable();


            if (usePaging)
            {
                var offset = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

                return new PagenatedList<User>
                {
                    Items = await offset.ToListAsync(),
                    TotalCount = await query.CountAsync(),
                    Page = pageRequest.Page,
                    PageSize = pageRequest.PageSize
                };
            }

            return new PagenatedList<User>
            {
                Items = await query.ToListAsync(),
                TotalCount = await query.CountAsync(),
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<bool> IsExistAsync(string email)

            => await context.Users
                .AnyAsync(x => x.Email == email);

        public void Update(User user)
        {
            context.Update(user);
        }
    }
}
