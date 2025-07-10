using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.ViewModels;
using ProcureToPay.Data;
using ProcureToPay.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.ViewModels;
using ProcureToPay.Data;
using ProcureToPay.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly IEmailService _emailService;
        private readonly IPasswordGeneratorService _passwordGenerator;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ApplicationDbContext context,
            ILogger<UsersController> logger,
            IEmailService emailService,
            IPasswordGeneratorService passwordGenerator)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _passwordGenerator = passwordGenerator ?? throw new ArgumentNullException(nameof(passwordGenerator));
        }

        // GET: UserManagement/Users
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Branch)
                    .Include(u => u.Department)
                    .ToListAsync();

                var viewModel = new UsersIndexViewModel
                {
                    Users = users,
                    UserRoles = new Dictionary<int, IList<string>>()
                };

                // Collect roles for each user
                foreach (var user in users)
                {
                    viewModel.UserRoles[user.Id] = await _userManager.GetRolesAsync(user);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users list");
                TempData["ErrorMessage"] = "Failed to retrieve users. Please try again.";
                return View(new UsersIndexViewModel { Users = new List<User>() });
            }
        }

        // GET: UserManagement/Users/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Branch)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                var viewModel = new UserDetailsViewModel
                {
                    User = user,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for ID: {UserId}", id);
                TempData["ErrorMessage"] = "Failed to retrieve user details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: UserManagement/Users/Create
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Entering Create GET method");

            try
            {
                var model = new AdminUserFormViewModel();
                await PopulateFormViewModel(model); // This populates RolesList, BranchesList, DepartmentsList
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing user creation form");
                TempData["ErrorMessage"] = "Failed to load form data. Please try again.";

                // On error, create a new model and populate its lists
                // The PopulateFormViewModel's catch block ensures minimal lists if the DB call fails
                var errorModel = new AdminUserFormViewModel();
                try
                {
                    await PopulateFormViewModel(errorModel); // Attempt to populate even on error, so form is not entirely blank
                }
                catch // Catch internal PopulateFormViewModel error to avoid re-throw
                {
                    // Do nothing, errorModel will have empty lists or minimal error lists from PopulateFormViewModel's catch
                }
                return View(errorModel);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserFormViewModel model)
        {
            _logger.LogInformation("Attempting to create user: {Username}", model?.User?.UserName);

            // --- Password and ConfirmPassword Handling ---
            // These are auto-generated, so we remove their ModelState entries
            // before checking overall ModelState.IsValid.
            var temporaryPassword = _passwordGenerator.GenerateTemporaryPassword();
            model.Password = temporaryPassword;
            model.ConfirmPassword = temporaryPassword;

            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            // Call your custom validation method. It should add errors to ModelState if needed.
            ValidateUserModelForAutoPasswordCreation(model);

            // Now, check if ModelState is valid for all fields that were actually submitted
            // (User properties, Id for Role, User.BranchId, User.DepartmentId)
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    model.User.ForcePasswordChange = true;

                    // This method should add specific errors to ModelState if it fails.
                    var createResult = await CreateUserWithRole(model.User, model.Password, model.RoleId);

                    if (createResult) // Check the result of CreateUserWithRole
                    {
                        try
                        {
                            await _emailService.SendCredentialsEmailAsync(
                                model.User.Email,
                                model.User.UserName,
                                temporaryPassword);

                            await transaction.CommitAsync();
                            _logger.LogInformation("User {Username} created successfully and credentials email sent", model.User.UserName);
                            TempData["SuccessMessage"] = $"User {model.User.UserName} created successfully! Login credentials have been sent to {model.User.Email}.";
                            return RedirectToAction(nameof(Index));
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, "User created but failed to send credentials email to {Email}", model.User.Email);
                            await transaction.CommitAsync(); // Still commit user creation even if email fails
                            TempData["WarningMessage"] = $"User {model.User.UserName} created successfully, but failed to send credentials email. Please manually provide the temporary password: {temporaryPassword}";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                    // If createResult is false, it implies CreateUserWithRole added errors to ModelState,
                    // and we'll fall through to re-display the form.
                }
                catch (Exception ex) // Catch transaction-level errors or other unhandled exceptions
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync(); // Rollback only if transaction was started
                    }
                    _logger.LogError(ex, "Error creating user. Transaction rolled back for {Username}.", model.User?.UserName);
                    ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                    // Fall through to re-display the form with error message.
                }
            }
            else
            {
                // This block is hit if initial model binding + data annotations + custom validation fails.
                _logger.LogWarning("ModelState is invalid for user {Username}. Errors: {Errors}",
                    model.User?.UserName, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // Always repopulate dropdowns and return the view if ModelState is invalid or if creation failed
            // (e.g., CreateUserWithRole returned false or an exception occurred).
            await PopulateFormViewModel(model); // This will re-populate RolesList, BranchesList, DepartmentsList
            return View(model);
        }
        private async Task PopulateFormViewModel<T>(T model) where T : IUserFormViewModel
        {
            if (model == null)
            {
                _logger.LogError("PopulateFormViewModel called with null model");
                throw new ArgumentNullException(nameof(model));
            }

            // Initialize collections first to prevent null reference exceptions,
            // even though the constructor does this, it's a good defensive practice.
            model.RolesList = model.RolesList ?? new List<SelectListItem>();
            model.BranchesList = model.BranchesList ?? new List<SelectListItem>();
            model.DepartmentsList = model.DepartmentsList ?? new List<SelectListItem>();

            try
            {
                // Populate Roles
                var roles = await _roleManager.Roles.ToListAsync();
                _logger.LogDebug("Fetched {RoleCount} roles", roles?.Count ?? -1);
                if (roles != null)
                {
                    model.RolesList = roles.Select(r =>
                    {
                        _logger.LogDebug("Creating SelectListItem for Role - Id: {RoleId}, Name: {RoleName}", r.Id, r.Name);
                        return new SelectListItem
                        {
                            Value = r.Id.ToString(), // Convert int Id to string for SelectListItem Value
                            Text = r.Name ?? "Unknown Role",
                            // Correct comparison for int? model.Id and int r.Id
                            Selected = model.RoleId.HasValue && model.RoleId.Value == r.Id
                        };
                    }).ToList();
                }
                else
                {
                    model.RolesList = new List<SelectListItem>();
                }
                _logger.LogDebug("RolesList count: {RolesListCount}", model.RolesList.Count());

                // Populate Branches
                var branches = await _context.Branches.ToListAsync(); // Assuming _context is your ApplicationDbContext
                _logger.LogDebug("Fetched {BranchCount} branches", branches?.Count ?? -1);
                if (branches != null)
                {
                    model.BranchesList = branches.Select(b =>
                    {
                        _logger.LogDebug("Creating SelectListItem for Branch - Id: {BranchId}, Name: {BranchName}", b.BranchId, b.BranchName);
                        return new SelectListItem
                        {
                            Value = b.BranchId.ToString(),
                            Text = b.BranchName ?? "Unknown Branch",
                            // model.User.BranchId is int?, b.BranchId is int
                            Selected = model.User?.BranchId.HasValue == true && model.User.BranchId.Value == b.BranchId
                        };
                    }).ToList();
                }
                else
                {
                    model.BranchesList = new List<SelectListItem>();
                }
                _logger.LogDebug("BranchesList count: {BranchesListCount}", model.BranchesList.Count());

                // Populate Departments (This is the one that was showing 0)
                var departments = await _context.Departments.ToListAsync(); // Assuming _context is your ApplicationDbContext
                _logger.LogDebug("Fetched {DepartmentCount} departments", departments?.Count ?? -1);
                if (departments != null)
                {
                    model.DepartmentsList = departments.Select(d =>
                    {
                        _logger.LogDebug("Creating SelectListItem for Department - Id: {DepartmentId}, Name: {DepartmentName}", d.DepartmentId, d.DepartmentName);
                        return new SelectListItem
                        {
                            Value = d.DepartmentId.ToString(), // Assuming DepartmentId is int
                            Text = d.DepartmentName ?? "Unknown Department",
                            // model.User.DepartmentId is int?, d.DepartmentId is int
                            Selected = model.User?.DepartmentId.HasValue == true && model.User.DepartmentId.Value == d.DepartmentId
                        };
                    }).ToList();
                }
                else
                {
                    model.DepartmentsList = new List<SelectListItem>();
                }
                _logger.LogDebug("DepartmentsList count: {DepartmentsListCount}", model.DepartmentsList.Count());

                _logger.LogInformation("Successfully populated view model with {RolesCount} roles, {BranchesCount} branches, and {DepartmentsCount} departments",
                    model.RolesList.Count(), model.BranchesList.Count(), model.DepartmentsList.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating form view model");
                // Ensure lists are empty on error to prevent partial data or null references in the view
                model.RolesList = new List<SelectListItem>();
                model.BranchesList = new List<SelectListItem>();
                model.DepartmentsList = new List<SelectListItem>();
                throw; // Re-throw to handle at the calling level (e.g., in the Controller action)
            }
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                // Get user with navigation properties
                var user = await _context.Users
                    .Include(u => u.Branch)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                // Create and populate view model
                var model = new AdminEditUserViewModel
                {
                    User = user
                };

                // Get current role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    var role = await _roleManager.FindByNameAsync(roles.First());
                    if (role != null)
                    {
                        model.RoleId = role.Id;
                    }
                }

                // Load dropdown data
                await PopulateFormViewModel(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for edit, ID: {UserId}", id);
                TempData["ErrorMessage"] = "Failed to retrieve user for editing. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: UserManagement/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminEditUserViewModel model)
        {
            // Initial check: Ensure the ID from the URL matches the ID in the model's User object.
            // This is important if your routing sends the User ID in the URL.
            if (id != model.User.Id)
            {
                // Return JSON for an ID mismatch, indicating an error that should close the modal
                return Json(new { success = false, message = "User ID mismatch." });
            }

            // Always check ModelState.IsValid first to catch any basic validation errors (e.g., [Required])
            // If ModelState is not valid at this point, return the PartialView to show errors in modal.
            if (!ModelState.IsValid)
            {
                await PopulateFormViewModel(model); // Repopulate dropdowns before returning the view
                return PartialView(model); // Assuming this is your partial view name
            }

            try
            {
                var existingUser = await _userManager.FindByIdAsync(id.ToString()); // Use 'id' from route parameter
                if (existingUser == null)
                {
                    // Return JSON for not found, indicating an error that should close the modal
                    return Json(new { success = false, message = "User not found." });
                }

                // --- Custom Validation for Email and Username Uniqueness ---
                // Check if email already exists for another user
                var userWithSameEmail = await _userManager.FindByEmailAsync(model.User.Email);
                if (userWithSameEmail != null && userWithSameEmail.Id != existingUser.Id)
                {
                    ModelState.AddModelError("User.Email", "This email address is already in use by another user.");
                }

                // Check if username already exists for another user
                var userWithSameUserName = await _userManager.FindByNameAsync(model.User.UserName);
                if (userWithSameUserName != null && userWithSameUserName.Id != existingUser.Id)
                {
                    ModelState.AddModelError("User.UserName", "This username is already taken by another user.");
                }

                // If custom validation failed, return the PartialView with errors
                if (!ModelState.IsValid)
                {
                    await PopulateFormViewModel(model); // Repopulate dropdowns
                    return PartialView(model); // Assuming this is your partial view name
                }
                // --- End Custom Validation ---

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Update user properties
                    existingUser.FirstName = model.User.FirstName;
                    existingUser.LastName = model.User.LastName;
                    existingUser.Email = model.User.Email;
                    existingUser.UserName = model.User.UserName;
                    existingUser.BranchId = model.User.BranchId;
                    existingUser.DepartmentId = model.User.DepartmentId;
                    existingUser.IsActive = model.User.IsActive;

                    // Update the user
                    var updateResult = await _userManager.UpdateAsync(existingUser);
                    if (!updateResult.Succeeded)
                    {
                        foreach (var error in updateResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await transaction.RollbackAsync();
                        await PopulateFormViewModel(model);
                        return PartialView("_EditUserPartial", model); // Return PartialView on UserManager error
                    }

                    // Handle role assignment if changed
                    if (model.RoleId.HasValue)
                    {
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        var newRole = await _roleManager.FindByIdAsync(model.RoleId.Value.ToString());

                        if (newRole != null)
                        {
                            bool roleChanged = !currentRoles.Contains(newRole.Name);

                            if (roleChanged)
                            {
                                var addResult = await _userManager.AddToRoleAsync(existingUser, newRole.Name);
                                if (addResult.Succeeded)
                                {
                                    var rolesToRemove = currentRoles.Where(r => r != newRole.Name).ToList();
                                    if (rolesToRemove.Any())
                                    {
                                        await _userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);
                                    }
                                }
                                else
                                {
                                    foreach (var error in addResult.Errors)
                                    {
                                        ModelState.AddModelError(string.Empty, error.Description);
                                    }
                                    await transaction.RollbackAsync();
                                    await PopulateFormViewModel(model);
                                    return PartialView(model); // Return PartialView on role add error
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    // On successful update, return JSON to indicate success and close the modal
                    return Json(new { success = true, message = "User updated successfully!" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Log the error more specifically for transaction issues
                    _logger.LogError(ex, "Transaction rolled back during user update for ID: {UserId}", id);
                    ModelState.AddModelError(string.Empty, "An unexpected database error occurred during update. Please try again.");
                    await PopulateFormViewModel(model); // Repopulate dropdowns in case of transaction error
                    return PartialView(model); // Return PartialView on transaction error
                }
            }
            catch (Exception ex)
            {
                // General error handling
                _logger.LogError(ex, "Error updating user {UserId}", id);
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                await PopulateFormViewModel(model); // Repopulate dropdowns
                return PartialView(model); // Return PartialView on general error
            }
        }

        // GET: UserManagement/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Branch)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                var viewModel = new UserDetailsViewModel
                {
                    User = user,
                    Roles = await _userManager.GetRolesAsync(user)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user for deletion, ID: {UserId}", id);
                TempData["ErrorMessage"] = "Failed to retrieve user for deletion. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: UserManagement/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Get user roles first
                    var roles = await _userManager.GetRolesAsync(user);

                    // Remove roles
                    if (roles.Any())
                    {
                        var roleResult = await _userManager.RemoveFromRolesAsync(user, roles);
                        if (!roleResult.Succeeded)
                        {
                            _logger.LogWarning("Failed to remove roles from user {UserId}. Errors: {Errors}",
                                id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                            TempData["WarningMessage"] = "Failed to remove roles from user. Deletion aborted.";
                            await transaction.RollbackAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // Delete the user
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("User {UserId} deleted successfully", id);
                        TempData["SuccessMessage"] = "User deleted successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete user {UserId}. Errors: {Errors}",
                            id, string.Join(", ", result.Errors.Select(e => e.Description)));

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Failed to delete user. Please try again.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Transaction rolled back due to error", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = $"An error occurred while deleting the user: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: UserManagement/Users/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };

            return View(model);
        }

        // POST: UserManagement/Users/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.UserId <= 0)
                {
                    return NotFound();
                }

                var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Generate password reset token
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // Reset password
                    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        _logger.LogInformation("Password changed for user {UserId}", model.UserId);
                        TempData["SuccessMessage"] = "Password changed successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        await transaction.RollbackAsync();
                        _logger.LogWarning("Password change failed for user {UserId}. Errors: {Errors}",
                            model.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Transaction rolled back due to error", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", model.UserId);
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(model);
        }

        // New action to resend credentials
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendCredentials(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                // Generate new temporary password
                var newTemporaryPassword = _passwordGenerator.GenerateTemporaryPassword();

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Reset password
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, newTemporaryPassword);

                    if (result.Succeeded)
                    {
                        // Set force password change
                        user.ForcePasswordChange = true;
                        await _userManager.UpdateAsync(user);

                        // Send new credentials email
                        await _emailService.SendCredentialsEmailAsync(
                            user.Email,
                            user.UserName,
                            newTemporaryPassword);

                        await transaction.CommitAsync();
                        _logger.LogInformation("New credentials sent for user {UserId}", id);
                        TempData["SuccessMessage"] = $"New login credentials have been sent to {user.Email}.";
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Failed to reset password. Please try again.";
                    }
                }
                catch (Exception emailEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(emailEx, "Failed to send credentials email for user {UserId}", id);
                    TempData["ErrorMessage"] = "Failed to send credentials email. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending credentials for user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

#if DEBUG
        // GET: UserManagement/Users/Debug - Only available in debug mode
        public async Task<IActionResult> Debug()
        {
            var model = new DebugViewModel();

            try
            {
                // Database connection status
                model.DbConnectionStatus = _context.Database.CanConnect() ? "Connected" : "Disconnected";
                model.IdentityEnabled = _userManager != null && _roleManager != null;

                // Check User table
                try
                {
                    model.UserCount = await _context.Users.CountAsync();
                    model.UserTableAccessible = true;
                }
                catch (Exception ex)
                {
                    model.UserTableAccessible = false;
                    model.UserTableError = ex.Message;
                }

                // Check Roles table
                try
                {
                    model.RoleCount = await _roleManager.Roles.CountAsync();
                    model.RoleTableAccessible = true;
                }
                catch (Exception ex)
                {
                    model.RoleTableAccessible = false;
                    model.RoleTableError = ex.Message;
                }

                // Check Branches table
                try
                {
                    model.BranchCount = await _context.Branches.CountAsync();
                    model.BranchTableAccessible = true;
                }
                catch (Exception ex)
                {
                    model.BranchTableAccessible = false;
                    model.BranchTableError = ex.Message;
                }

                // Check Departments table
                try
                {
                    model.DeptCount = await _context.Departments.CountAsync();
                    model.DeptTableAccessible = true;
                }
                catch (Exception ex)
                {
                    model.DeptTableAccessible = false;
                    model.DeptTableError = ex.Message;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Debug view");
                return View(model);
            }
        }
#endif

        #region Helper Methods

        // Combined helper method to populate view models

        private void ValidateUserModelForAutoPasswordCreation(AdminUserFormViewModel model)
        {
            if (!model.RoleId.HasValue || model.RoleId.Value <= 0)
            {
                ModelState.AddModelError("Id", "Role is required");
            }

            if (string.IsNullOrEmpty(model.User.UserName))
            {
                ModelState.AddModelError("User.UserName", "Username is required");
            }

            if (string.IsNullOrEmpty(model.User.Email))
            {
                ModelState.AddModelError("User.Email", "Email is required");
            }

            if (string.IsNullOrEmpty(model.User.FirstName))
            {
                ModelState.AddModelError("User.FirstName", "First name is required");
            }

            if (string.IsNullOrEmpty(model.User.LastName))
            {
                ModelState.AddModelError("User.LastName", "Last name is required");
            }
        }

        private async Task<bool> CreateUserWithRole(User user, string password, int? selectedRoleId)
        {
            // Create the user with Identity
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created successfully: {Username}, UserId: {UserId}",
                    user.UserName, user.Id);

                // Assign role to user if available
                if (selectedRoleId.HasValue)
                {
                    var role = await _roleManager.FindByIdAsync(selectedRoleId.Value.ToString());
                    if (role != null)
                    {
                        _logger.LogInformation("Found role: {RoleName}, attempting to assign to user", role.Name);
                        var roleResult = await _userManager.AddToRoleAsync(user, role.Name);

                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("Role {RoleName} assigned successfully to user {Username}",
                                role.Name, user.UserName);
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("Error assigning role. Errors: {Errors}",
                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));

                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, $"Role assignment error: {error.Description}");
                            }
                            return false;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Role with ID {RoleId} not found", selectedRoleId.Value);
                        ModelState.AddModelError(string.Empty, "Selected role was not found.");
                        return false;
                    }
                }

                return true;
            }
            else
            {
                _logger.LogWarning("User creation failed. Identity errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return false;
            }
        }

        #endregion
    }
}