using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Repositories;
using Domain.Entities;
using MediatR;
using static Application.Commands.StartConversation.StartConversationHandler;

namespace Application.Commands
{
    public class StartConversation
    {
        public record StartConversationCommand(Guid SenderId, string UserName) : IRequest<Result<StartConversationResponse>>;

        public class StartConversationHandler(IConversationRepository conversationRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork) : IRequestHandler<StartConversationCommand, Result<StartConversationResponse>>
        {
            public async Task<Result<StartConversationResponse>> Handle(StartConversationCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetByUserNameAsync(request.UserName);

                user ??= await userRepository.GetAsync(request.UserName);

                var checkConversation = await conversationRepository.GetPrivateConversationAsync(request.SenderId, user.Id);

                if (checkConversation is not null)
                {
                    return Result<StartConversationResponse>.Failure("You already have a private chart with this user");
                }

                if (user is null)
                {
                    return Result<StartConversationResponse>.Failure("Inputed username not on the app");
                }

                var conversation = new Conversation
                {
                    Title = "Private chart",
                    CreatedBy = request.SenderId.ToString(),
                    LastMessageAt = DateTime.UtcNow,
                    UserConversations = new List<UserConversation>
                     {
                         new UserConversation
                         {
                             UserId = request.SenderId
                         },

                         new UserConversation
                         {
                             UserId = user.Id
                         }
                     },
                };

                await conversationRepository.AddAsync(conversation);
                await unitOfWork.SaveAsync();

                return Result<StartConversationResponse>.Success(new StartConversationResponse(conversation.Id), "You can now chart with your new friend");
            }

            public record StartConversationResponse(Guid ConversationId);
        }
    }
}
