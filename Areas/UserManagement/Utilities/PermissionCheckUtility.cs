using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProcureToPay.Areas.UserManagement.Utilities
{
    public static class PermissionCheckUtility
    {
        // Extension method for ClaimsPrincipal
        public static async Task<bool> HasPermissionAsync(this ClaimsPrincipal user,
            string permissionName,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            if (!user.Identity.IsAuthenticated)
                return false;

            // Get user
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return false;

            // Parse user ID
            if (!int.TryParse(userId, out int parsedUserId))
                return false;

            // Check if user has the Admin role (admins have all permissions)
            if (await userManager.IsInRoleAsync(await userManager.FindByIdAsync(userId), "Admin"))
                return true;

            // Get all roles for the user
            var userRoles = await userManager.GetRolesAsync(await userManager.FindByIdAsync(userId));

            // Get permission ID 
            var permission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Name == permissionName && p.IsActive);

            if (permission == null)
                return false;

            // Check if any of the user's roles have this permission
            foreach (var roleName in userRoles)
            {
                var role = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == roleName && r.IsActive);

                if (role == null)
                    continue;

                // Check if this role has the permission
                var hasPermission = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                if (hasPermission)
                    return true;
            }

            return false;
        }

        // For module-specific permissions in the future
        public static async Task<bool> HasModulePermissionAsync(this ClaimsPrincipal user,
            string moduleName,
            string permissionName,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            // Base permission check first
            if (!await HasPermissionAsync(user, permissionName, context, userManager))
                return false;

            // Additional module-specific logic could be added here
            // For example, checking if the user has access to the specific module

            return true;
        }
    }
}