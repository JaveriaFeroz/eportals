using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.ViewModels;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.ViewModels;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public RolesController(RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // GET: UserManagement/Roles
        public async Task<IActionResult> Index(string searchString = null, bool? isActive = null)
        {
            var rolesQuery = _roleManager.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                rolesQuery = rolesQuery.Where(r => r.Name.Contains(searchString) ||
                                                r.Description.Contains(searchString));
            }

            if (isActive.HasValue)
            {
                rolesQuery = rolesQuery.Where(r => r.IsActive == isActive.Value);
            }

            return View(await rolesQuery.OrderBy(r => r.HierarchyLevel).ToListAsync());
        }

        public IActionResult Create()
        {
            var model = new RoleCreateViewModel();

            // Get the maximum HierarchyLevel from the database
            // Use .DefaultIfEmpty(0) to handle the case where no roles exist yet
            var maxHierarchyLevel = _context.Roles.Any() ? _context.Roles.Max(r => r.HierarchyLevel) : 0;

            // Set the next available hierarchy level
            model.HierarchyLevel = maxHierarchyLevel + 1;

            return View(model);
        }

        // POST: UserManagement/Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateViewModel viewModel)
        {
            Console.WriteLine("Create action method called");

            Console.WriteLine("Form data:");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"  {key} = {Request.Form[key]}");
            }

            // Custom validation for duplicate role name
            if (await _roleManager.FindByNameAsync(viewModel.Name) != null)
            {
                ModelState.AddModelError("Name", "A role with this name already exists.");
            }

            // Log ModelState errors BEFORE checking IsValid
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState errors:");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"  Field: {state.Key}");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"    - {error.ErrorMessage}");
                        }
                    }
                }
                return PartialView(viewModel);
            }

            // If ModelState is valid, proceed to create the role
            var role = new Role
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                HierarchyLevel = viewModel.HierarchyLevel,
                IsActive = viewModel.IsActive
            };

            // Use RoleManager to create the role
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                // Optional: Add a success message (e.g., using TempData)
                TempData["SuccessMessage"] = $"Role '{role.Name}' created successfully.";

                // Redirect to the Index page after successful creation
                return RedirectToAction("Index", "Roles", new { area = "UserManagement" });
            }
            else
            {
                // Add errors from RoleManager to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    Console.WriteLine($"RoleManager Error: {error.Description}");
                }
                // If there are errors from RoleManager, return the view with errors
                return PartialView(viewModel);
            }
        }

        // GET: UserManagement/Roles/Edit/5 (For showing the Edit Form in a Modal)
        [HttpGet] // <-- Add this attribute
        public async Task<IActionResult> Edit(int? id)
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

            var viewModel = new RoleEditViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                HierarchyLevel = role.HierarchyLevel,
                IsActive = role.IsActive
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            // Custom validation for duplicate role name (excluding the current role)
            var roleWithSameName = await _roleManager.FindByNameAsync(viewModel.Name);
            if (roleWithSameName != null && roleWithSameName.Id != viewModel.Id)
            {
                ModelState.AddModelError("Name", "A role with this name already exists.");
            }

            // Check ModelState.IsValid *after* adding custom errors
            if (!ModelState.IsValid)
            {
                return PartialView(viewModel); // Return PartialView on validation error
            }

            try
            {
                var existingRole = await _roleManager.FindByIdAsync(id.ToString()); // Use string id
                if (existingRole == null)
                {
                    return Json(new { success = false, message = "Role not found for update." });
                }

                // Update properties
                existingRole.Name = viewModel.Name;
                existingRole.NormalizedName = viewModel.Name.ToUpper();
                existingRole.Description = viewModel.Description;
                existingRole.HierarchyLevel = viewModel.HierarchyLevel;
                existingRole.IsActive = viewModel.IsActive;

                var result = await _roleManager.UpdateAsync(existingRole);
                if (result.Succeeded)
                {
                    // Success for AJAX modal submission
                    return Json(new { success = true, message = "Role updated successfully!" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // Concurrency handling for AJAX modals
                if (!RoleExists(viewModel.Id)) // Ensure RoleExists accepts string ID
                {
                    return Json(new { success = false, message = "The role was deleted by another user." });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The role was modified by another user. Please refresh and try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION during role update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"Error updating role: {ex.Message}");
            }

            // If ModelState is not valid or an error occurred, return the PartialView with errors.
            return PartialView(viewModel);
        }


        // GET: UserManagement/Roles/Delete/5 (For showing the Delete confirmation in a Modal)
        public async Task<IActionResult> Delete(string id) // Changed id type to string
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            // You might want to return a PartialView for the delete confirmation modal as well
            return PartialView("_DeleteRolePartial", role); // Assuming you have a partial for this
        }

        // POST: UserManagement/Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // Changed id type to int based on Role : IdentityRole<int>
        {
            // Find the role by ID (using int type)
            var role = await _roleManager.FindByIdAsync(id.ToString()); // FindByIdAsync expects string, so convert
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction("Index"); // Redirect to index with error message
            }

            // Attempt to delete the role
            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                // Set a success message for display on the Index page
                TempData["SuccessMessage"] = $"Role '{role.Name}' deleted successfully!";
                return RedirectToAction("Index"); // Redirect to the main Index page
            }
            else
            {
                // If deletion failed, collect errors and pass them to the Index page
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"Error deleting role '{role.Name}': {errors}";
                return RedirectToAction("Index"); // Redirect to Index with error message
            }
        }

        // GET UserManagement/Roles/ManageHierarchy
        public async Task<IActionResult> ManageHierarchy()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.HierarchyLevel).ToListAsync();
            return View(roles);
        }

        //  UserManagement/Roles/UpdateHierarchy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHierarchy(List<int> roleIds, List<int> hierarchyLevels)
        {
            if (roleIds.Count != hierarchyLevels.Count)
            {
                return BadRequest("Invalid data");
            }

            for (int i = 0; i < roleIds.Count; i++)
            {
                var role = await _roleManager.FindByIdAsync(roleIds[i].ToString());
                if (role != null)
                {
                    role.HierarchyLevel = hierarchyLevels[i];
                    await _roleManager.UpdateAsync(role);
                }
            }

            return RedirectToAction(nameof(ManageHierarchy));
        }

        [HttpGet]
        public async Task<IActionResult> ManageModuleHierarchy(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                //  all modules for selection
                var modules = await _context.Modules.Where(m => m.IsActive).ToListAsync();
                return View("SelectModule", modules);
            }

            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Name == moduleName);
            if (module == null)
            {
                return NotFound("Module not found");
            }

            //  all roles with their hierarchy for this module
            var roleHierarchies = await _context.ModuleRoleHierarchies
                .Include(m => m.Role)
                .Where(m => m.ModuleName == moduleName)
                .OrderBy(m => m.HierarchyLevel)
                .ToListAsync();

            //  roles that don't have a hierarchy for this module yet
            var rolesWithoutHierarchy = await _roleManager.Roles
                .Where(r => !roleHierarchies.Select(m => m.RoleId).Contains(r.Id))
                .ToListAsync();

            // Create view model
            var viewModel = new ModuleHierarchyViewModel
            {
                ModuleName = moduleName,
                ModuleDisplayName = module.DisplayName,
                RoleHierarchies = roleHierarchies,
                AvailableRoles = rolesWithoutHierarchy
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateModuleHierarchy(string moduleName, List<int> roleIds, List<int> hierarchyLevels)
        {
            if (string.IsNullOrEmpty(moduleName) || roleIds.Count != hierarchyLevels.Count)
            {
                return BadRequest("Invalid data");
            }

            // Get the module
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Name == moduleName);
            if (module == null)
            {
                return NotFound("Module not found");
            }

            // Remove existing hierarchies for this module
            var existingHierarchies = await _context.ModuleRoleHierarchies
                .Where(m => m.ModuleName == moduleName)
                .ToListAsync();

            _context.ModuleRoleHierarchies.RemoveRange(existingHierarchies);

            //  new hierarchies
            for (int i = 0; i < roleIds.Count; i++)
            {
                var roleHierarchy = new ModuleRoleHierarchy
                {
                    RoleId = roleIds[i],
                    ModuleName = moduleName,
                    HierarchyLevel = hierarchyLevels[i]
                };

                _context.ModuleRoleHierarchies.Add(roleHierarchy);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageModuleHierarchy), new { moduleName = moduleName });
        }

        private bool RoleExists(int id)
        {
            return _roleManager.Roles.Any(e => e.Id == id);
        }

        //  duplicate role method
        public async Task<IActionResult> Duplicate(int? id)
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

            var viewModel = new RoleCreateViewModel
            {
                Name = $"{role.Name} (Copy)",
                Description = role.Description,
                HierarchyLevel = role.HierarchyLevel,
                IsActive = role.IsActive
            };

            return View("Create", viewModel);
        }

        //  bulk activation/deactivation method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkActivate(List<int> selectedRoles, bool activate)
        {
            if (selectedRoles == null || selectedRoles.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var roleId in selectedRoles)
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role != null)
                {
                    role.IsActive = activate;
                    await _roleManager.UpdateAsync(role);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        //  role export method
        public async Task<IActionResult> Export()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var moduleHierarchies = await _context.ModuleRoleHierarchies
                .Include(m => m.Role)
                .ToListAsync();

            var exportData = new
            {
                Roles = roles,
                ModuleHierarchies = moduleHierarchies
            };

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });

            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "roles_export.json");
        }


        //  user role assignment method
        public async Task<IActionResult> AssignUsers(int? id, [FromServices] UserManager<User> userManager)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id.ToString()); // FindByIdAsync expects string
            if (role == null)
            {
                return NotFound();
            }

            // Get all users from the database
            var allUsers = await userManager.Users.ToListAsync();

            // Filter users who are currently in this role
            var usersInRole = new List<IdentityUser<int>>();
            var availableUsers = new List<IdentityUser<int>>();

            foreach (var user in allUsers)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    usersInRole.Add(user);
                }
                else
                {
                    availableUsers.Add(user);
                }
            }

            var viewModel = new AssignUsersViewModel
            {
                Role = role,
                UsersInRole = usersInRole,
                AvailableUsers = availableUsers
            };

            return View(viewModel);
        }



        // POST: UserManagement/Roles/AssignUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUsers(int id, List<string> selectedUsers,
            [FromServices] UserManager<User> userManager)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            // Get current users in this role
            var usersInRole = new List<User>();
            var allUsers = await userManager.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    // Remove from role if not in selected users
                    if (!selectedUsers.Contains(user.Id.ToString()))
                    {
                        await userManager.RemoveFromRoleAsync(user, role.Name);
                    }
                }
                else
                {
                    // Add to role if in selected users
                    if (selectedUsers.Contains(user.Id.ToString()))
                    {
                        await userManager.AddToRoleAsync(user, role.Name);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: UserManagement/Roles/ManagePermissions/5
        public async Task<IActionResult> ManagePermissions(int? id)
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

            // Get all permissions
            var allPermissions = await _context.Permissions
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .ToListAsync();

            // Get assigned permissions for this role
            var assignedPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var assignedPermissions = allPermissions
                .Where(p => assignedPermissionIds.Contains(p.Id))
                .ToList();

            var availablePermissions = allPermissions
                .Where(p => !assignedPermissionIds.Contains(p.Id))
                .ToList();

            var viewModel = new RolePermissionsViewModel
            {
                Role = role,
                AssignedPermissions = assignedPermissions,
                AvailablePermissions = availablePermissions
            };

            return View(viewModel);
        }

        // POST: UserManagement/Roles/UpdatePermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermissions(int id, List<int> selectedPermissions)
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

            // Remove all existing permissions
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
            return RedirectToAction(nameof(Index), new { id = id });
        }
    }
}