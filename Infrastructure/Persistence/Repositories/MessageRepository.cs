using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class MessageRepository(AppDbContext context) : IMessageRepository
    {
        public async Task AddAsync(Message message)
            => await context.Messages.AddAsync(message);

        public async Task<Message?> GetByIdAsync(Guid id)
            => await context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
    }
}