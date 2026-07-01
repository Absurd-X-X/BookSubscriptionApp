using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ConversationRepository(AppDbContext context) : IConversationRepository
    {
        public async Task AddAsync(Conversation conversation)
            => await context.Conversations.AddAsync(conversation);

        public async Task<Conversation?> GetByIdAsync(Guid id)
            => await context.Conversations
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.UserConversations)
                    .ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        public async Task<ICollection<Conversation>> GetByUserIdAsync(Guid userId)
            => await context.Conversations
                .Include(c => c.Messages)
                .Include(c => c.UserConversations)
                .Where(c => c.UserConversations
                    .Any(uc => uc.UserId == userId) && !c.IsDeleted)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

        public async Task<ICollection<Conversation>> GetAllAsync()
            => await context.Conversations
                .Include(c => c.Messages)
                .Include(c => c.UserConversations)
                    .ThenInclude(uc => uc.User)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
    }
}