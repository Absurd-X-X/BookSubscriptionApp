using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetConversationByUserId
    {
        public record GetConversationByUserIdQuery(Guid UserId)
            : IRequest<Result<List<GetConversationByUserIdResponse>>>;
        public class GetMyConversationsHandler(
            IConversationRepository conversationRepository, IUserRepository userRepository)

            : IRequestHandler<GetConversationByUserIdQuery,
                Result<List<GetConversationByUserIdResponse>>>
        {
            public async Task<Result<List<GetConversationByUserIdResponse>>> Handle(
                GetConversationByUserIdQuery request,
                CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);
                if (user is null)
                {
                    return Result<List<GetConversationByUserIdResponse>>.Failure("Unknown user");
                }

                var conversations = await conversationRepository
                    .GetByUserIdAsync(request.UserId);


                var response = conversations.Select(c =>
                    new GetConversationByUserIdResponse(
                        c.Id,
                        c.Messages.Where(x => !x.IsRead && !x.IsDeleted && x != null).Count(),
                        c.Title,
                        c.LastMessageAt,
                        c.Messages.Count,
                        c.Messages,
                        user.ImageUrl ?? "none",
                        c.Messages
                            .OrderByDescending(m => m.SentAt)
                            .FirstOrDefault()?.Content ?? ""))
                    .ToList();

                return Result<List<GetConversationByUserIdResponse>>
                    .Success(response, "Success");
            }
        }

        public record GetConversationByUserIdResponse(
            Guid ConversationId,
            int UnreadCount,
            string Title,
            DateTime LastMessageAt,
            int TotalMessages,
            IEnumerable<Message> Messages,
            string ImageUrl,
            string LastMessage
            );
    }
}