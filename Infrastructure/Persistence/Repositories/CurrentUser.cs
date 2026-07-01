using Application.Common.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Persistence.Repositories
{
    public class CurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
    {
        public Guid GetCurrentUser()
        {
            if (contextAccessor.HttpContext == null)
                throw new InvalidOperationException("GetCurrentUser() called outside of an HTTP request context.");

            //var sub = contextAccessor.HttpContext.User?.FindFirstValue("sub");
            var sub = contextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(sub))
                throw new InvalidOperationException("No 'sub' claim found. User may not be authenticated.");

            return Guid.Parse(sub);
        }
    }
}
