using System.Security.Claims;

namespace ProcureToPay.Extensions
{
    public static class PermissionExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string resource, string action)
        {
            var requiredClaimValue = $"{resource}.{action}";
            return user.HasClaim(c => c.Type == "Permission" && c.Value == requiredClaimValue);
        }
    }
}