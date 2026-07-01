using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Hubs;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BackGroundApi.BackgroundServices
{
    public class ExpirationBackgroundService(IServiceScopeFactory serviceScopeFactory,
        IReaderRepository readerRepository, IHubContext<NotificationHub> hubContext) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await GetExpirations(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }

        private async Task GetExpirations(CancellationToken token)
        {
            var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var subScope = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
            var userScope = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var notificationScope = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var subscriptions = await subScope.GetSubscriptionsAsync(false, new PageRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });
            foreach (var item in subscriptions.Items)
            {
                var reader = await readerRepository.GetByIdAsync(item.ReaderId);
                var user = await userScope.GetAsync(reader!.Email);

                DateTime target = DateTime.UtcNow.AddDays(3);

                if (item.Types.ExpiryDate == target)
                {
                    Notification notification = new Notification
                    {
                        UserId = user!.Id,
                        Title = "Subscription Expired Reminder",
                        Message = "Your present subscription will outdated in the next three days",
                        CreatedBy = user.Email,
                        Type = NotificationType.Others,
                        Ref = user.Id.ToString()
                    };

                    await notificationScope.AddAsync(notification);
                    await context.SaveChangesAsync();

                    await hubContext.Clients.User(user.Id.ToString()).SendAsync("ReceiveNotification", new
                    {
                        notification.Title,
                        notification.Message,
                        notification.DateCreated
                    });
                }

                subScope.Update(item);
            }
            await context.SaveChangesAsync();
        }
    }
}