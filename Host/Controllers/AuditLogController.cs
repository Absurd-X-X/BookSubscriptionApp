using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.AddAuditLog;
using static Application.Queries.GetAllAuditLog;
using static Application.Queries.GetAuditLog;
using static Application.Queries.GetAuditLogByActionType;
using static Application.Queries.GetAuditLogByRole;

namespace Host.Controllers
{
    public class AuditLogController(IMediator mediator) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id, int page = 1, int pageSize = 6)
        {
            var res = await mediator.Send(new GetAuditLogQuery(id, page, pageSize));

            if (!res.Status)
            {
                TempData["Error"] = res.Message;
                return View(res.Data);
            }

            return View(res.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetByRole(string role, int page = 1, int pageSize = 6)
        {
            var res = await mediator.Send(new GetAuditLogByRoleQuery(role, page, pageSize));

            if (!res.Status)
            {
                TempData["Error"] = res.Message;
                return View(res.Data);
            }

            return View(res.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 6)
        {
            var res = await mediator.Send(new GetAllAuditLogQuery(page, pageSize));

            if (!res.Status)
            {
                TempData["Error"] = res.Message;
                return View(res.Data);
            }

            return View(res.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetByActionType(string actionType, int page = 1, int pageSize = 6)
        {
            var res = await mediator.Send(new GetAuditLogByActionTypeQuery(actionType, page, pageSize));

            if (!res.Status)
            {
                TempData["Error"] = res.Message;
                return View(res.Data);
            }

            return View(res.Data);
        }
    }
}
