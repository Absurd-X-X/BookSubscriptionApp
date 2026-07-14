using Application.Common.Repositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Infrastructure.Persistence.Repositories
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public async Task<int> SaveAsync()
        {
                return await context.SaveChangesAsync();
            
        }
    }
}
