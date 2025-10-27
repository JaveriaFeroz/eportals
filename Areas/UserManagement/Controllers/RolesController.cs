using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProcureToPay.Areas.Common.Models;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.ViewModels;
using ProcureToPay.Areas.UserManagement.ViewModels;
using ProcureToPay.Data;
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
                // ⭐ FIX: Add condition to exclude the module with Id == 15.
                var modules = await _context.Modules
                    .Where(m => m.IsActive && m.Id != 15)
                    .ToListAsync();

                return View("SelectModule", modules);
            }

            // Check if the module is "Payment Request" and redirect to the new selection page
            if (moduleName == "Payment Request")
            {
                return RedirectToAction(nameof(SelectPaymentHierarchy), new { moduleName = moduleName });
            }

            // Existing logic for other modules
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
        public async Task<IActionResult> UpdateModuleHierarchy(
    string moduleName,
    List<int> roleIds,
    List<int> hierarchyLevels,
    int? paymentNatureId = null, // Add these parameters
    int? paymentSubNatureId = null) // Add these parameters
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

            // Yahan hum paymentNatureId aur paymentSubNatureId ko short? mein convert kar rahe hain
            short? natureId = paymentNatureId.HasValue ? (short)paymentNatureId.Value : (short?)null;
            short? subNatureId = paymentSubNatureId.HasValue ? (short)paymentSubNatureId.Value : (short?)null;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Step 1: Remove existing hierarchies for the specific module and, if provided, nature/sub-nature combination
                    var existingHierarchies = await _context.ModuleRoleHierarchies
                        .Where(m => m.ModuleName == moduleName)
                        .ToListAsync();
                    _context.ModuleRoleHierarchies.RemoveRange(existingHierarchies);

                    // Step 2: Remove existing WorkflowApprovalSequences for this workflow type and combination
                    var existingWorkflowSequences = await _context.WorkFlowApprovalSequences
                        .Where(s => s.WorkFlowTypeId == module.Id &&
                                    s.PaymentNatureID == natureId && // Use the new variables here
                                    s.PaymentSubNatureID == subNatureId) // Use the new variables here
                        .ToListAsync();

                    _context.WorkFlowApprovalSequences.RemoveRange(existingWorkflowSequences);

                    // Step 3: Add new ModuleRoleHierarchies and WorkflowApprovalSequences
                    for (int i = 0; i < roleIds.Count; i++)
                    {
                        var roleHierarchy = new ModuleRoleHierarchy
                        {
                            RoleId = roleIds[i],
                            ModuleName = moduleName,
                            HierarchyLevel = hierarchyLevels[i]
                        };

                        _context.ModuleRoleHierarchies.Add(roleHierarchy);

                        // Combine user role and user data to get BranchId and DepartmentId
                        var userDetails = await (from ur in _context.UserRoles
                                                 join u in _context.Users on ur.UserId equals u.Id
                                                 where ur.RoleId == roleIds[i]
                                                 select new
                                                 {
                                                     u.BranchId,
                                                     u.DepartmentId
                                                 }).FirstOrDefaultAsync();

                        if (userDetails != null)
                        {
                            var workflowSequence = new WorkFlowApprovalSequence
                            {
                                WorkFlowTypeId = module.Id,
                                RoleID = roleIds[i],
                                ApprovalSeq = hierarchyLevels[i],
                                DepartmentCode = userDetails.DepartmentId.ToString(),
                                BranchCode = userDetails.BranchId.ToString(),
                                IsActive = true,
                                // Yahan par new IDs store kar rahe hain
                                PaymentNatureID = natureId,
                                PaymentSubNatureID = subNatureId,
                                // Other fields remain as placeholders or as defined by your logic
                                RequestNatureId = 0,
                                RequestTypeId = 0,
                                CompanyCode = "XYZ",
                                MinAmount = 0,
                                MaxAmount = 999999999
                            };
                            _context.WorkFlowApprovalSequences.Add(workflowSequence);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(ManageModuleHierarchy), new { moduleName = moduleName });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Log the exception and handle it appropriately
                    return StatusCode(500, "An error occurred while updating the module hierarchy.");
                }
            }
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


        //  user role assignment method
        public async Task<IActionResult> AssignUsers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Use RoleManager to find the role by ID
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }

            // Get all users and their role assignments in a single query
            // This is the efficient part that solves the N+1 problem
            var usersWithRoles = await _context.Users
                .Include(u => u.UserRoles) // Make sure to include the navigation property
                .Select(u => new
                {
                    User = u,
                    IsInRole = u.UserRoles.Any(ur => ur.RoleId == role.Id)
                })
                .ToListAsync();

            // Now, split the results into two lists in memory (which is very fast)
            var usersInRole = usersWithRoles
                .Where(x => x.IsInRole)
                .Select(x => x.User)
                .ToList();

            var availableUsers = usersWithRoles
                .Where(x => !x.IsInRole)
                .Select(x => x.User)
                .ToList();

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

        // GET: UserManagement/Roles/SelectPaymentHierarchy
        [HttpGet]
        public async Task<IActionResult> SelectPaymentHierarchy(string moduleName)
        {
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Name == moduleName);
            if (module == null)
            {
                return NotFound("Module not found.");
            }

            var viewModel = new SelectPaymentHierarchyViewModel
            {
                ModuleName = module.Name,
                ModuleDisplayName = module.DisplayName,
                // Only get active PaymentNatures
                PaymentNatures = await _context.PaymentNatures
                                               .Where(pn => pn.IsActive)
                                               .ToListAsync(),
                // Only get active PaymentSubNatures
                PaymentSubNatures = await _context.SubNatures
                                                  .Where(sn => sn.IsActive)
                                                  .ToListAsync()
            };

            // Check for existing hierarchies to display on the cards
            var existingSequences = await _context.WorkFlowApprovalSequences
                .Where(s => s.WorkFlowTypeId == module.Id)
                .Select(s => new { s.PaymentNatureID, s.PaymentSubNatureID })
                .Distinct()
                .ToListAsync();

            viewModel.ExistingHierarchies = existingSequences
                .ToDictionary(s => (s.PaymentNatureID, s.PaymentSubNatureID), s => true);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ManagePaymentRequestHierarchy(string moduleName, int? paymentNatureId, int? paymentSubNatureId)
        {
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Name == moduleName);

            if (module == null)
            {
                return NotFound("Module not found.");
            }

            // Default values set karein agar parameters null hon
            var natureId = paymentNatureId ?? 0;
            var subNatureId = paymentSubNatureId ?? 0;

            // Existing hierarchy load karein
            var roleHierarchies = new List<ModuleRoleHierarchy>();
            if (natureId > 0 && subNatureId > 0)
            {
                // WorkFlowApprovalSequences ko join karke ModuleRoleHierarchy mein map karein
                roleHierarchies = await _context.WorkFlowApprovalSequences
                    .Include(s => s.Role) // Ensure Role navigation property is loaded
                    .Where(s => s.WorkFlowTypeId == module.Id &&
                                s.PaymentNatureID == natureId &&
                                s.PaymentSubNatureID == subNatureId)
                    .OrderBy(s => s.ApprovalSeq)
                    .Select(s => new ModuleRoleHierarchy
                    {
                        RoleId = s.RoleID,
                        Role = s.Role,
                        HierarchyLevel = s.ApprovalSeq,
                    })
                    .ToListAsync();
            }

            // Available roles load karein jo is hierarchy mein nahi hain
            var rolesInHierarchy = roleHierarchies.Select(r => r.RoleId).ToList();
            var availableRoles = await _roleManager.Roles
                .Where(r => !rolesInHierarchy.Contains(r.Id))
                .ToListAsync();

            // Dropdowns ke liye SelectListItem banayein
            var paymentNaturesForDropdown = await _context.PaymentNatures.Select(n => new SelectListItem
            {
                Value = n.PaymentNatureId.ToString(),
                Text = n.PaymentNatureName
            }).ToListAsync();

            var paymentSubNaturesForDropdown = await _context.SubNatures.Select(s => new SelectListItem
            {
                Value = s.SubNatureId.ToString(),
                Text = s.SubNatureName
            }).ToListAsync();

            // View Model banayein
            var viewModel = new PaymentRequestHierarchyViewModel
            {
                ModuleName = module.Name,
                ModuleDisplayName = module.DisplayName,
                PaymentNatureId = natureId,
                PaymentSubNatureId = subNatureId,
                RoleHierarchies = roleHierarchies,
                AvailableRoles = availableRoles,
                PaymentNatures = paymentNaturesForDropdown,
                PaymentSubNatures = paymentSubNaturesForDropdown,
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentRequestHierarchy(
    string moduleName,
    int? paymentNatureId,
    int? paymentSubNatureId,
    List<int> roleIds,
    List<int> hierarchyLevels)
        {
            // Pehle validation check karein
            if (string.IsNullOrEmpty(moduleName) || roleIds.Count != hierarchyLevels.Count)
            {
                return BadRequest("Invalid data. Role and hierarchy lists do not match.");
            }

            // Module find karein
            var module = await _context.Modules.FirstOrDefaultAsync(m => m.Name == moduleName);
            if (module == null)
            {
                return NotFound("Module not found.");
            }

            // PaymentNatureId aur PaymentSubNatureId ko check karein
            short? natureId = paymentNatureId.HasValue ? (short?)paymentNatureId.Value : null;
            short? subNatureId = paymentSubNatureId.HasValue ? (short?)paymentSubNatureId.Value : null;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Step 1: Existing records remove karein
                    // Hum hamesha existing records ko remove karte hain taaki naye records unki jagah le sakein.
                    var existingSequences = await _context.WorkFlowApprovalSequences
                        .Where(s => s.WorkFlowTypeId == module.Id &&
                                    s.PaymentNatureID == natureId &&
                                    s.PaymentSubNatureID == subNatureId)
                        .ToListAsync();

                    if (existingSequences.Any())
                    {
                        _context.WorkFlowApprovalSequences.RemoveRange(existingSequences);
                    }

                    // Step 2: Naye records add karein
                    for (int i = 0; i < roleIds.Count; i++)
                    {
                        var roleId = roleIds[i];
                        var hierarchyLevel = hierarchyLevels[i];

                        var userDetails = await (from ur in _context.UserRoles
                                                 join u in _context.Users on ur.UserId equals u.Id
                                                 where ur.RoleId == roleId
                                                 select new
                                                 {
                                                     u.BranchId,
                                                     u.DepartmentId
                                                 }).FirstOrDefaultAsync();

                        var workflowSequence = new WorkFlowApprovalSequence
                        {
                            WorkFlowTypeId = module.Id,
                            RoleID = roleId,
                            ApprovalSeq = hierarchyLevel,
                            PaymentNatureID = natureId,
                            PaymentSubNatureID = subNatureId,
                            IsActive = true,
                            BranchCode = userDetails?.BranchId.ToString() ?? "N/A",
                            DepartmentCode = userDetails?.DepartmentId.ToString() ?? "N/A",
                            RequestNatureId = 0,
                            RequestTypeId = 0,
                            CompanyCode = "XYZ",
                            MinAmount = 0,
                            MaxAmount = 999999999
                        };
                        _context.WorkFlowApprovalSequences.Add(workflowSequence);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Payment Request hierarchy updated successfully!";
                    return RedirectToAction(nameof(ManageModuleHierarchy));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "An error occurred while saving the hierarchy. Please try again.";
                    return RedirectToAction(nameof(ManageModuleHierarchy));
                }
            }
        }
    }
}