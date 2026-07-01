using System.Security.Claims;

namespace Web.Helpers
{
    public static class ClaimsHelper
    {
        public static Guid GetUserId(ClaimsPrincipal user)
            => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Guid.Empty.ToString());

        public static Guid GetCustomerId(ClaimsPrincipal user)
            => Guid.Parse(user.FindFirstValue("CustomerId")
                ?? Guid.Empty.ToString());

        public static string GetFullName(ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        public static string GetEmail(ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public static string GetRole(ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}