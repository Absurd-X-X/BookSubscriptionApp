using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Command
{
    public class UpdateReadingProgress
    {
        public record UpdateReadingProgressCommand(
            Guid ReaderId,
            Guid BookId,
            int CurrentPage,
            string? Cfi,
            string? CurrentChapter,
            double ProgressPercentage,
            int MinutesRead,
            int PagesRead
        ) : IRequest<Result<string>>;

        public class UpdateReadingProgressHandler(
            IReadingProgressRepository readingProgressRepository,
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<UpdateReadingProgressCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                UpdateReadingProgressCommand request,
                CancellationToken cancellationToken)
            {
                var book = await bookRepository.GetByIdAsync(request.BookId);

                if (book == null)
                    return Result<string>.Failure("Book not found.");

                var progress = await readingProgressRepository
                    .GetAsync(request.ReaderId, request.BookId);

                if (progress == null)
                {
                    progress = new ReadingProgress
                    {
                        ReaderId = request.ReaderId,
                        BookId = request.BookId,
                        CurrentPage = request.CurrentPage,
                        Cfi = request.Cfi,
                        CurrentChapter = request.CurrentChapter,
                        ProgressPercentage = request.ProgressPercentage,
                        TotalMinutesRead = request.MinutesRead,
                        TotalPagesRead = request.PagesRead,
                        IsCompleted = request.ProgressPercentage >= 100,
                        CreatedBy = request.ReaderId.ToString()
                    };

                    await readingProgressRepository.AddAsync(progress);

                    book.NoOfTimeReadByPeople++;
                }
                else
                {
                    progress.CurrentPage = request.CurrentPage;
                    progress.Cfi = request.Cfi;
                    progress.CurrentChapter = request.CurrentChapter;
                    progress.ProgressPercentage = request.ProgressPercentage;
                    progress.TotalMinutesRead += request.MinutesRead;
                    progress.TotalPagesRead += request.PagesRead;
                    progress.IsCompleted = request.ProgressPercentage >= 100;
                    progress.DateModified = DateTime.UtcNow;
                }

                // ===============================
                // Reading Streak
                // ===============================

                var today = DateTime.UtcNow.Date;

                var progresses = await readingProgressRepository
                    .GetByReaderAsync(request.ReaderId);

                int streak = 1;
                int longestStreak = 1;

                var latest = progresses
                    .OrderByDescending(x => x.LastReadDate)
                    .FirstOrDefault();

                if (latest != null)
                {
                    streak = latest.ReadingStreak;
                    longestStreak = latest.LongestReadingStreak;

                    if (latest.LastReadDate.HasValue)
                    {
                        var lastDate = latest.LastReadDate.Value.Date;

                        if (lastDate == today)
                        {
                            // Already counted today
                        }
                        else if (lastDate == today.AddDays(-1))
                        {
                            streak++;

                            if (streak > longestStreak)
                                longestStreak = streak;
                        }
                        else
                        {
                            streak = 1;
                        }
                    }
                }

                progress.LastReadDate = today;
                progress.ReadingStreak = streak;
                progress.LongestReadingStreak = longestStreak;

                foreach (var item in progresses)
                {
                    item.LastReadDate = today;
                    item.ReadingStreak = streak;
                    item.LongestReadingStreak = longestStreak;
                    item.DateModified = DateTime.UtcNow;
                }

                await unitOfWork.SaveAsync();

                return Result<string>.Success(
                    "Updated",
                    "Reading progress updated successfully.");
            }
        }
    }
}