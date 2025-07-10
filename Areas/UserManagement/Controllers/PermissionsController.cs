using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Models.ViewModels;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Roles = "Admin")]
    public class PermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(ApplicationDbContext context, ILogger<PermissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: UserManagement/Permissions
        public async Task<IActionResult> Index(string searchString, int? moduleId)
        {
            var query = _context.Permissions.Include(p => p.Module).AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) ||
                                        p.Description.Contains(searchString));
            }

            if (moduleId.HasValue)
            {
                query = query.Where(p => p.ModuleId == moduleId.Value);
            }

            // Get modules for filter dropdown - keep as regular list for Index view
            ViewBag.Modules = await _context.Modules
                .Where(m => m.IsActive)
                .ToListAsync();

            return View(await query.OrderBy(p => p.Module.Name).ThenBy(p => p.Name).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET method was called");

            var viewModel = new CreatePermissionViewModel();

            // Get modules for dropdown
            var modules = await _context.Modules.Where(m => m.IsActive).ToListAsync();
            viewModel.ModuleOptions = modules.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.DisplayName
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePermissionViewModel viewModel)
        {
            _logger.LogInformation($"Create POST called with Name={viewModel?.Name}, ModuleId={viewModel?.ModuleId}, Description length={viewModel?.Description?.Length ?? 0}, IsActive={viewModel?.IsActive}");

            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Model state is valid");

                    // Create Permission from ViewModel
                    var permission = new Permission
                    {
                        Name = viewModel.Name,
                        ModuleId = viewModel.ModuleId,
                        Description = viewModel.Description,
                        IsActive = viewModel.IsActive
                    };

                    try
                    {
                        // Add the permission to the context
                        _context.Add(permission);
                        _logger.LogInformation($"Adding permission to context - Name: {permission.Name}, ModuleId: {permission.ModuleId}");

                        // Save changes asynchronously
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Permission successfully saved with ID: {permission.Id}");

                        // Redirect to the Index action
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException ex)
                    {
                        _logger.LogError($"Database error: {ex.InnerException?.Message ?? ex.Message}");
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
                else
                {
                    _logger.LogWarning("Model state is invalid. Errors:");
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning($"ModelState Error: {modelError.ErrorMessage}");
                    }

                    // Log specific field values for debugging
                    _logger.LogWarning($"Posted values - Name: '{viewModel?.Name}', ModuleId: {viewModel?.ModuleId}, Description: '{viewModel?.Description}', IsActive: {viewModel?.IsActive}");
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError($"Exception occurred: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error creating permission: {ex.Message}");
            }

            // Reload modules for dropdown (needed for the view again)
            var modules = await _context.Modules.Where(m => m.IsActive).ToListAsync();
            viewModel.ModuleOptions = modules.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.DisplayName
            }).ToList();

            return View(viewModel);
        }
        // GET: UserManagement/Permissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            // Get modules for dropdown - use regular list to match Index view pattern
            ViewBag.Modules = await _context.Modules.Where(m => m.IsActive).ToListAsync();

            return PartialView(permission);
        }

        // POST: UserManagement/Permissions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Permission permission)
        {
            if (id != permission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(permission);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PermissionExists(permission.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Get modules for dropdown - use regular list
            ViewBag.Modules = await _context.Modules.Where(m => m.IsActive).ToListAsync();

            return View(permission);
        }

        // GET: UserManagement/Permissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permission = await _context.Permissions
                .Include(p => p.Module)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (permission == null)
            {
                return NotFound();
            }

            return View(permission);
        }

        // POST: UserManagement/Permissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private bool PermissionExists(int id)
        {
            return _context.Permissions.Any(e => e.Id == id);
        }
    }
}