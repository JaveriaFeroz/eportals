// Modified RolePermissionsController.cs
using ProcureToPay.Areas.UserManagement.Attributes;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.ViewModels;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Roles = "Admin")]
    [RequirePermission("ManageRolePermissions")]
    public class RolePermissionsController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public RolePermissionsController(RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // GET: UserManagement/RolePermissions
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            // For each role, get the count of assigned permissions
            var roleSummaries = new List<RolePermissionSummaryViewModel>();
            foreach (var role in roles)
            {
                var permissionCount = await _context.RolePermissions
                    .CountAsync(rp => rp.RoleId == role.Id);

                roleSummaries.Add(new RolePermissionSummaryViewModel
                {
                    Role = role,
                    PermissionCount = permissionCount
                });
            }

            return View(roleSummaries);
        }

        // GET: UserManagement/RolePermissions/Manage/5
        public async Task<IActionResult> Manage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            // Get permissions with their modules
            var permissionsByModule = await _context.Permissions
                .Include(p => p.Module)
                .OrderBy(p => p.Module.DisplayName)
                .ThenBy(p => p.Name)
                .ToListAsync();

            // Get modules for grouping
            var modules = permissionsByModule
                .Select(p => p.Module.DisplayName)
                .Distinct()
                .ToList();

            // Get assigned permission IDs for this role
            var assignedPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var viewModel = new ManageRolePermissionsViewModel
            {
                Role = role,
                Modules = modules,
                Permissions = permissionsByModule,
                AssignedPermissionIds = assignedPermissionIds
            };

            return View(viewModel);
        }

        // POST: UserManagement/RolePermissions/Manage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(int id, List<int> selectedPermissions)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            // Get current role permissions
            var currentPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();

            // Remove all current permissions
            _context.RolePermissions.RemoveRange(currentPermissions);

            // Add selected permissions
            if (selectedPermissions != null)
            {
                foreach (var permissionId in selectedPermissions)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = id,
                        PermissionId = permissionId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Permissions for role '{role.Name}' updated successfully.";
            return RedirectToAction(nameof(Manage), new { id });
        }

        // GET: UserManagement/RolePermissions/Compare
        public async Task<IActionResult> Compare()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            return View(roles);
        }

        // POST: UserManagement/RolePermissions/Compare
        [HttpPost]
        public async Task<IActionResult> CompareRoles(List<int> selectedRoles)
        {
            if (selectedRoles == null || selectedRoles.Count < 2)
            {
                TempData["ErrorMessage"] = "Please select at least 2 roles to compare.";
                return RedirectToAction(nameof(Compare));
            }

            var selectedRolesData = await _roleManager.Roles
                .Where(r => selectedRoles.Contains(r.Id))
                .ToListAsync();

            // Get all permissions with their modules
            var allPermissions = await _context.Permissions
                .Include(p => p.Module)
                .OrderBy(p => p.Module.DisplayName)
                .ThenBy(p => p.Name)
                .ToListAsync();

            // Get permission mapping for each role
            var rolePermissionMap = new Dictionary<int, HashSet<int>>();
            foreach (var roleId in selectedRoles)
            {
                var permissionIds = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                rolePermissionMap[roleId] = new HashSet<int>(permissionIds);
            }

            var viewModel = new CompareRolesViewModel
            {
                Roles = selectedRolesData,
                AllPermissions = allPermissions,
                RolePermissionMap = rolePermissionMap
            };

            return View(viewModel);
        }

        // GET: UserManagement/RolePermissions/CopyPermissions
        public async Task<IActionResult> CopyPermissions()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            var viewModel = new CopyPermissionsViewModel
            {
                SourceRoles = roles,
                TargetRoles = roles
            };

            return View(viewModel);
        }

        // POST: UserManagement/RolePermissions/CopyPermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopyPermissions(int sourceRoleId, List<int> targetRoleIds)
        {
            if (targetRoleIds == null || targetRoleIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one target role.";
                return RedirectToAction(nameof(CopyPermissions));
            }

            // Get source role permissions
            var sourcePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == sourceRoleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            if (sourcePermissions.Count == 0)
            {
                TempData["ErrorMessage"] = "Source role has no permissions to copy.";
                return RedirectToAction(nameof(CopyPermissions));
            }

            // For each target role, copy permissions
            foreach (var targetRoleId in targetRoleIds)
            {
                // Skip if source and target are the same
                if (targetRoleId == sourceRoleId)
                    continue;

                // Get existing target role permissions
                var existingTargetPermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == targetRoleId)
                    .ToListAsync();

                // Remove existing permissions
                _context.RolePermissions.RemoveRange(existingTargetPermissions);

                // Add new permissions from source
                foreach (var permissionId in sourcePermissions)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = targetRoleId,
                        PermissionId = permissionId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Permissions copied successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}