using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class AuditLogRepository(AppDbContext context) : IAuditLogRepository
    {
        public async Task AddAsync(AuditLog auditLog)
        {
            await context.AuditLogs.AddAsync(auditLog);
        }

        public async Task<PagenatedList<AuditLog>> GetAllAsync(PageRequest request, bool usePaging)
        {
            var query = context.AuditLogs.Where(x => !x.IsDeleted).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.Timestamp)
                    .Include(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<AuditLog>
                {
                    Items = await set.Include(x => x.User).ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<AuditLog>
            {
                Items = await query.Include(x => x.User).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<AuditLog>> GetAsync(Guid userId, PageRequest request, bool usePaging)
        {
            var query = context.AuditLogs.Where(x => !x.IsDeleted && x.UserId == userId).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.Timestamp)
                    .Include(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<AuditLog>
                {
                    Items = await set.Include(x => x.User).ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<AuditLog>
            {
                Items = await query.Include(x => x.User).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<AuditLog>> GetByActionTypeAsync(string actionType, PageRequest request, bool usePaging)
        {
            var query = context.AuditLogs.Where(x => !x.IsDeleted && x.ActionType == actionType).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.Timestamp)
                    .Include(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<AuditLog>
                {
                    Items = await set.Include(x => x.User).ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<AuditLog>
            {
                Items = await query.Include(x => x.User).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id)
        {
            return await context.AuditLogs
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<PagenatedList<AuditLog>> GetByUserRoleAsync(string role, PageRequest request, bool usePaging)
        {
            var query = context.AuditLogs.Where(x => !x.IsDeleted && x.UserRole == role).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .OrderByDescending(x => x.Timestamp)
                    .Include(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<AuditLog>
                {
                    Items = await set.Include(x => x.User).ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<AuditLog>
            {
                Items = await query.Include(x => x.User).ToListAsync(),
                TotalCount = totalCount
            };
        }
    }
}
