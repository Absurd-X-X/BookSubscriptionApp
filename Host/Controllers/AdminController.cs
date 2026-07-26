using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Queries;
using Application.ViewModels;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Web.Helpers;
using static Application.Command.AddCategory;
using static Application.Command.AddLibrary;
using static Application.Command.DeleteLibrary;
using static Application.Command.UpdateCategory;
using static Application.Command.UpdateLibraryDetails;
using static Application.Commands.AddSubscriptionType;
using static Application.Commands.DeleteCategory;
using static Application.Commands.MarkConversationAsRead;
using static Application.Commands.SendMessage;
using static Application.Commands.UploadProfilePIcs;
using static Application.Queries.GetActivityById;
using static Application.Queries.GetAdminDashBoardStat;
using static Application.Queries.GetAllAuditLog;
using static Application.Queries.GetAllBook;
using static Application.Queries.GetAllCategories;
using static Application.Queries.GetAllLibrary;
using static Application.Queries.GetAllReader;
using static Application.Queries.GetAuditLog;
using static Application.Queries.GetBookById;
using static Application.Queries.GetCategoryById;
using static Application.Queries.GetConversationByUserId;
using static Application.Queries.GetConversationMessages;
using static Application.Queries.GetLibraryById;
using static Application.Queries.GetMyNotifications;
using static Application.Queries.GetNotificationById;
using static Application.Queries.GetPendingTransaction;
using static Application.Queries.GetSubscriptionById;
using static Application.Queries.GetSubscriptionByUserId;
using static Application.Queries.GetSubscriptions;
using static Application.Queries.GetTransactionById;
using static Application.Queries.GetTransactionByTransactionStatus;
using static Application.Queries.GetTransactions;
using static Application.Queries.GetUserDetails;
using static Application.Queries.GetUsers;
using static Application.Queries.GetWalletBalance;

namespace Host.Controllers
{
    public class AdminController(IMediator mediator, IHubContext<Hub> chatHub) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var result = await mediator.Send(new GetAdminDashBoardStatQuery());
            return View(result.Data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryCommand command)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message =
                    string.Join(",", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                });
            }

            var result = await mediator.Send(command);

            if (!result.Status)
            {
                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Json(new
            {
                success = false,
                message = result.Message,
                redirectUrl = Url.Action(nameof(GetCategories))
            });
        }

        [HttpGet]
        public async Task<IActionResult> AddCategory()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks(int page = 1, int pageSize = 10)
        {
            var result = await mediator.Send(new GetAllBookQuery(page, pageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result);
            }

            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var result = await mediator.Send(new GetBookByIdQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetBooks));
            }

            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetMessages(Guid senderId)
        {
            var result = await mediator.Send(new GetConversationMessagesQuery(senderId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Data);
            }
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new GetConversationByUserIdQuery(userId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Data);
            }

            ViewBag.CurrentUserId = userId;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetConversation(Guid conversationId)
        {
            var userId = ClaimsHelper.GetUserId(User);

            await mediator.Send(new MarkConversationAsReadCommand(conversationId, userId));

            var listResult = await mediator.Send(new GetConversationByUserIdQuery(userId));
            if (!listResult.Status)
            {
                TempData["Error"] = listResult.Message;
                return View(nameof(GetConversations), listResult.Data);
            }

            ViewBag.CurrentUserId = userId;
            ViewBag.SelectedConversationId = conversationId;
            return View(nameof(GetConversations), listResult.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Send(
        Guid conversationId, string content)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var fullName = ClaimsHelper.GetFullName(User);

            var result = await mediator.Send(
                new SendMessageCommand(conversationId, userId, content));

            if (!result.Status)
                return Json(new { success = false, message = result.Message });

            await chatHub.Clients
                .Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", new
                {
                    senderId = userId,
                    senderName = fullName,
                    content,
                    sentAt = DateTime.UtcNow,
                });

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetReaders(int page)
        {
            var result = await mediator.Send(new GetAllReaderQuery(page));
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(int page = 1, int pageSize = 7)
        {
            var result = await mediator.Send(new GetAllCategoryQuery(page, pageSize));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetLibraries(int page = 1, int pageSize = 7 )
        {
            Result<PagenatedList<GetAllLibraryResponse>> libraries = await mediator.Send(new GetAllLibraryQuery(page, pageSize));

            if (!libraries.Status)
            {
                TempData["Error"] = libraries.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = libraries .Message;

            return View(libraries.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetLibrary(Guid libraryId)
        {
            var result = await mediator.Send(new GetLibraryByIdQuery(libraryId));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetLibraries));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }


        [HttpGet]
        public async Task<IActionResult> GetNotifications(Guid userId)
        {
            var result = await mediator.Send(new GetMyNotificationsQuery(userId));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result);
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetMyNotification(Guid id)
        {
            var result = await mediator.Send(new GetNotificationByIdQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetNotifications));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetCategory(Guid categoryId)
        {
            var result = await mediator.Send(new GetCategoryByIdQuery(categoryId ));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetCategories));
            }
            TempData["Success"] = result.Message;
            return View(result.Data);
        }




        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new DeleteCategoryCommand(id, userId));

            TempData[result.Status ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(GetCategories));
        }
        [HttpGet]
        public async Task<IActionResult> GetSubscriptions(GetSubscriptionsQuery subscriptions)
        {
            var result = await mediator.Send(subscriptions);
            return View(result.Data);
        }
        [HttpGet]
        public async Task<IActionResult> GetUserSubscription(Guid id)
        {
            var result = await mediator.Send(new GetSubscriptionByUserIdQuery(id));

            //if (!result.Status)
            //{
            //    TempData["Error"] = result.Message;
            //    return RedirectToAction(nameof(GetSubscriptions));
            //}

            return View(result.Data);
        }
        [HttpGet]

        public async Task<IActionResult> GetInvoices(Guid id)
        {
            var result = await mediator.Send(new GetSubscriptionByIdQuery(id));

            //if (!result.Status)
            //{
            //    TempData["Error"] = result.Message;
            //    return RedirectToAction(nameof(GetSubscriptions));
            //}

            return View(result.Data);
        }
        [HttpGet]

        public async Task<IActionResult> GetSubscription(Guid id)
        {
            var result = await mediator.Send(new GetSubscriptionByIdQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetSubscriptions));
            }

            return View(result.Data);
        }


        [HttpGet]
        //public async Task<IActionResult> GetTransactionsByUserId(Guid UserId)
        //{
        //    var result = await mediator.Send(new GetTransactionHistoryQuery(UserId));

        //    if (!result.Status)
        //    {
        //        TempData["Error"] = result.Message;
        //        return View(result);
        //    }

        //    return View(result.Data);
        //}


        [HttpGet]
        public async Task<IActionResult> GetTransactions(int page = 1, int pageSize = 10)
        {
            var result = await mediator.Send(new GetTransactions.GetTransactionsQuery(page, pageSize));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Data);
            }

            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            var result = await mediator.Send(new GetTransactionByIdQuery(id));

            //if (!result.Status)
            //{
            //    TempData["Error"] = result.Message;
            //    return RedirectToAction(nameof(GetTransactions));
            //}

            return View(result.Data);
        }
        //[HttpGet]
        //public async Task<IActionResult> GetWallet(Guid userId)
        //{
        //    var result = await mediator.Send(new GetWalletBalanceQuery(userId));

        //    if (!result.Status)
        //    {
        //        TempData["Error"] = result.Message;
        //        return View(result);
        //    }

        //    return View(result.Data);
        //}

        [HttpPost]
        public async Task<IActionResult> AddLibrary(AddLibraryCommand command)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message =
                    string.Join(",", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                });
            }

            var result = await mediator.Send(command);

            if (!result.Status)
            {
                var libraries = await mediator.Send(new GetAllLibraryQuery(1, 7));
                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }


            return Json(new
            {
                success = true,
                message = result.Message,
                redirectUrl = Url.Action(nameof(GetLibraries))
            });
        }


        [HttpPost]

        public async Task<IActionResult> AddSubType(string typeName, decimal cost, BillingCycle cycle)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message =
                    string.Join(",", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                });
            }

            var userId = ClaimsHelper.GetUserId(User);

            var response = await mediator.Send(new AddSubscriptionTypeCommand(userId, typeName, cycle, cost));

            if (!response.Status)
            {
                TempData["Error"] = response.Message;

                return Json(new
                {
                    success = false,
                    message = response.Message
                });
            }

            TempData["Success"] = response.Message; 
            return Json(new
            {
                success = true,
                message = response.Message,
                redirectUrl = Url.Action(nameof(GetSubscriptions))
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLibrary(Guid id)
        {
            var result = await mediator.Send(new DeleteLibraryCommand(id));

            TempData[result.Status ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(GetLibraries));
        }

        [HttpGet]

        public async Task<IActionResult> EditLibrary(Guid id)
        {
            var result = await mediator.Send(new GetLibraryByIdQuery(id));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetLibraries));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetWallet(
        int txPage = 1,
        int creditPage = 1,
        int debitPage = 1,
        int pendingPage = 1,
        int pageSize = 3)
        {
            var customerId = ClaimsHelper.GetCustomerId(User);

            var balance = await mediator.Send(new GetWalletBalanceQuery(customerId));

            var transactionHistory = await mediator.Send(new GetTransactionsQuery(txPage, pageSize));

            var transactionStatus = await mediator.Send(new GetTransactionByTypeQuery(creditPage, pageSize, TransactionType.Credit));

            var transactionStatusDebit = await mediator.Send(new GetTransactionByTypeQuery(debitPage, pageSize, TransactionType.Debit));

            var transactionTypes = await mediator.Send(new GetPendingTransactionQuery(pendingPage, pageSize, WalletTransactionStatus.Pending));

            var walletFullDetails = new WalletFullData
            {
                Transactions = transactionHistory.Data!,
                TransactionStatusDebit = transactionStatusDebit.Data!,
                TransactionStatusCredit = transactionStatus.Data!,
                PendingTransactionStatus = transactionTypes.Data!,
                WalletBalance = balance.Data!
            };

            ViewBag.Balance = balance.Data?.Balance ?? 0;
            ViewBag.WalletId = balance.Data?.WalletId;

            return View(walletFullDetails);
        }

        [HttpPost]
        public async Task<IActionResult> EditLibrary(Guid id, string name, string phone, string email, string userName)
        {
            var result = await mediator.Send(new UpdateLibraryComand(id, name, email, phone, userName));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetLibraries));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]

        public async Task<IActionResult> EditCategory(Guid categoryId)
        {
            var result = await mediator.Send(new GetCategoryByIdQuery(categoryId));

            if (result == null || !result.Status || result.Data == null)
            {
                TempData["Error"] = "The requested category could not be found.";
                return RedirectToAction(nameof(GetCategories));
            }

            return View(result.Data);
        }


        [HttpPost]
        public async Task<IActionResult> EditCategory(Guid id, string name, string description)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var result = await mediator.Send(new UpdateCategoryCommand(userId, id, name, description));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(GetCategories));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(GetCategories));
        }

        


        [HttpPost]
        public async Task<IActionResult> UploadImg(IFormFile file)
        {
            var userId = ClaimsHelper.GetUserId(User);

            var result = await mediator.Send(new UploadProfilePicsCommand(userId, file.OpenReadStream(), file.FileName));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            TempData["Success"] = result.Message;
            return View(result.Data);
        }

        [HttpGet]

        public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 6)
        {
            var result = await mediator.Send(new GetUsersQuery(page, pageSize, true));

            if (!result.Status)
            {
                TempData["Error"] = result.Message; 
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = result.Message;
            return View(result.Data);
        }
        [HttpGet]

        public async Task<IActionResult> GetUserDetails(Guid userId, int page = 1, int pageSize = 6)
        {
            var response = await mediator.Send(new GetUserDetailsQuery(userId));
            var logRes = await mediator.Send(new GetAuditLogQuery(userId, page, pageSize));
            if (!response.Status)
            {
                TempData["Error"] = response.Message;
                return RedirectToAction(nameof(GetUsers));
            }

            var viewModel = new UserDetailsAuditLog
            {
                UserDetails = response.Data!,
                AuditLogs = logRes.Data!,
            };

            return View(viewModel);
        }

        [HttpGet]

        public async Task<IActionResult> Settings()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SystemSettings()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> ActivityLogs(int page = 1, int pageSize = 6)
        {
            var result = await mediator.Send(new GetAllAuditLogQuery(page, pageSize));
            if (!result.Status) 
            {
                TempData["Error"] = result.Message;
                return View(result.Message);
            }
            return View(result.Data);
        }

        [HttpGet]

        public async Task<IActionResult> GetActivity(Guid id)
        {
            var result = await mediator.Send(new GetActivityByIdQuery(id));
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return View(result.Message);
            }
            return View(result.Data);
        }
    }
}
