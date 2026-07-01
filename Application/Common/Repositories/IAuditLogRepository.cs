using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);
        Task<AuditLog?> GetByIdAsync(Guid id);
        Task<PagenatedList<AuditLog>> GetAsync(Guid userId, PageRequest request, bool usePaging);
        Task<PagenatedList<AuditLog>> GetAllAsync(PageRequest request, bool usePaging);
        Task<PagenatedList<AuditLog>> GetByActionTypeAsync(string actionType, PageRequest request, bool usePaging);
        Task<PagenatedList<AuditLog>> GetByUserRoleAsync(string role, PageRequest request, bool usePaging);
    }
}
