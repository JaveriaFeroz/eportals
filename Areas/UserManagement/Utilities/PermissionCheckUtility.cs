using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProcureToPay.Areas.UserManagement.Utilities
{
    public static class PermissionCheckUtility
    {
        // Extension method for ClaimsPrincipal - checks by permission name only
        public static async Task<bool> HasPermissionAsync(this ClaimsPrincipal user,
            string permissionName,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            if (!user.Identity.IsAuthenticated)
                return false;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return false;

            if (!int.TryParse(userId, out int parsedUserId))
                return false;

            // Check if user has the Admin role (admins have all permissions)
            if (await userManager.IsInRoleAsync(await userManager.FindByIdAsync(userId), "Admin"))
                return true;

            var userRoles = await userManager.GetRolesAsync(await userManager.FindByIdAsync(userId));

            var permission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Name == permissionName && p.IsActive);

            if (permission == null)
                return false;

            foreach (var roleName in userRoles)
            {
                var role = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == roleName && r.IsActive);

                if (role == null)
                    continue;

                var hasPermission = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                if (hasPermission)
                    return true;
            }

            return false;
        }

        // NEW: Check permission by module and action type (for your structure)
        public static async Task<bool> HasModulePermissionAsync(this ClaimsPrincipal user,
            string moduleName,
            string permissionName, // "Add", "Edit", "View", "Delete", etc.
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            if (!user.Identity.IsAuthenticated)
                return false;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return false;

            if (!int.TryParse(userId, out int parsedUserId))
                return false;

            // Check if user has the Admin role
            if (await userManager.IsInRoleAsync(await userManager.FindByIdAsync(userId), "Admin"))
                return true;

            // Get the module
            var module = await context.Modules
                .FirstOrDefaultAsync(m => m.Name == moduleName || m.DisplayName == moduleName);

            if (module == null)
                return false;

            var userRoles = await userManager.GetRolesAsync(await userManager.FindByIdAsync(userId));

            // Find the specific permission for this module and action
            var permission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Name == permissionName
                                       && p.ModuleId == module.Id
                                       && p.IsActive);

            if (permission == null)
                return false;

            // Check if any of the user's roles have this permission
            foreach (var roleName in userRoles)
            {
                var role = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == roleName && r.IsActive);

                if (role == null)
                    continue;

                var hasPermission = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

                if (hasPermission)
                    return true;
            }

            return false;
        }

        // NEW: Batch check - get all permissions for a module
        public static async Task<ModulePermissions> GetModulePermissionsAsync(this ClaimsPrincipal user,
            string moduleName,
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            var permissions = new ModulePermissions
            {
                ModuleName = moduleName
            };

            if (!user.Identity.IsAuthenticated)
                return permissions;

            // Admin has all permissions
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var appUser = await userManager.FindByIdAsync(userId);
                if (appUser != null && await userManager.IsInRoleAsync(appUser, "Admin"))
                {
                    permissions.CanView = true;
                    permissions.CanAdd = true;
                    permissions.CanEdit = true;
                    permissions.CanDelete = true;
                    permissions.CanApprove = true;
                    return permissions;
                }
            }

            // Check each permission type
            permissions.CanView = await user.HasModulePermissionAsync(moduleName, "View", context, userManager);
            permissions.CanAdd = await user.HasModulePermissionAsync(moduleName, "Add", context, userManager);
            permissions.CanEdit = await user.HasModulePermissionAsync(moduleName, "Edit", context, userManager);
            permissions.CanDelete = await user.HasModulePermissionAsync(moduleName, "Delete", context, userManager);
            permissions.CanApprove = await user.HasModulePermissionAsync(moduleName, "Approve", context, userManager);

            return permissions;
        }
    }

    // Helper class to store module permissions
    public class ModulePermissions
    {
        public string ModuleName { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanCancel { get; set; }
    }
}