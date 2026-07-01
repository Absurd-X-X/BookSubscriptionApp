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
    public class ReminderBackgroundService(IServiceScopeFactory serviceScopeFactory,
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

                if (item.Types.ExpiryDate < DateTime.UtcNow && item.AutoRenewal)
                {
                    if (user!.Wallet!.Balance >= item.Types.Cost)
                    {
                        item.IsActive = true;
                        user.Wallet.Balance -= item.Types.Cost;
                        item.Types.SubscriptionDate = DateTime.UtcNow;

                        Notification notification = new Notification
                        {
                            UserId = user.Id,
                            Title = "Auto-subscription",
                            Message = "Your previous subscription expired and about to renew subscription",
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


                    else
                    {
                        item.IsActive = false;
                        Notification notification = new Notification
                        {
                            UserId = user.Id,
                            Title = "Auto-subscription",
                            Message = "Your previous subscription expired and about to renew subscription " +
                                 "but due to insufficient balance this can't be successful",
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
                    context.SaveChanges();
                    await context.SaveChangesAsync();
                }

                else if (item.Types.ExpiryDate < DateTime.UtcNow && !item.AutoRenewal)
                {
                    item.IsActive = false; Notification notification = new Notification
                    {
                        UserId = user!.Id,
                        Title = "Expired subscription",
                        Message = "Your previous subscription expired and about to renew subscription",
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

                await context.SaveChangesAsync();
            }
        }
    }
}