namespace Application.Common.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> SaveAsync();
    }
}
