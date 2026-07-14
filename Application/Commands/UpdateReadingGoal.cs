using Application.Common.Repositories;
using Application.Common.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class UpdateReadingGoal
    {
        public record UpdateReadingGoalCommand(
            Guid UserId,
            ReadingGoalType Type,
            int Target,
            DateTime? Deadline,
            string? Motivation
            ) : IRequest<Result<bool>>;

        public class UpdateReadingGoalHandler(
            IReaderRepository readerRepository,
            IUserRepository userRepository
            ) : IRequestHandler<UpdateReadingGoalCommand, Result<bool>>
        {
            async Task<Result<bool>> IRequestHandler<UpdateReadingGoalCommand, Result<bool>>.
                Handle(UpdateReadingGoalCommand request, CancellationToken cancellationToken)
            {
                var userId = request.UserId;

                var user = await userRepository.GetAsync(userId);

                if (user is null || user.Reader is null)
                {
                    return Result<bool>.Failure("Reader not found");
                }

                if (request.Target <= 0)
                {
                    return Result<bool>.Failure("Target must be greater than zero");
                }

                await readerRepository.UpdateReadingGoalAsync(
                    user.Reader.Id,
                    request.Type,
                    request.Target,
                    request.Deadline,
                    request.Motivation);

                return Result<bool>.Success(true, "Reading goal updated");
            }
        }
    }
}