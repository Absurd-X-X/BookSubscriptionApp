using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface ICurrentUser
    {
        Guid GetCurrentUser();
    }
}
