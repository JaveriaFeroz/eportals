using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Roles = "Admin")]
    public class ModulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ModulesController> _logger;

        public ModulesController(ApplicationDbContext context, ILogger<ModulesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: UserManagement/Modules
        public async Task<IActionResult> Index()
        {
            return View(await _context.Modules.ToListAsync());
        }

        // GET: UserManagement/Modules/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserManagement/Modules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Module module)
        {
            if (ModelState.IsValid)
            {
                _context.Add(module);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(module);
        }

        // GET: UserManagement/Modules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            return View(module);
        }

        // POST: UserManagement/Modules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Module module)
        {
            if (id != module.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(module);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModuleExists(module.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(module);
        }

        // GET: UserManagement/Modules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.Id == id);
            if (module == null)
            {
                return NotFound();
            }
            return View(module);
        }

        // POST: UserManagement/Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module != null)
            {
                _context.Modules.Remove(module);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: UserManagement/Modules/GeneratePermissions/5
        public async Task<IActionResult> GeneratePermissions(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var module = await _context.Modules.FindAsync(id);
            if (module == null)
            {
                return NotFound();
            }

            // Pass the module to the view
            return View(module);
        }

        // POST: UserManagement/Modules/GeneratePermissions/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePermissions(int id, bool createView, bool createAdd, bool createEdit, bool createDelete)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
            {
                return NotFound();
            }

            var permissionsToAdd = new List<Permission>();

            // Generate the selected CRUD permissions
            if (createView)
            {
                permissionsToAdd.Add(new Permission
                {
                    Name = $"View {module.DisplayName}",
                    Description = $"Permission to view {module.DisplayName} data",
                    ModuleId = module.Id,
                    IsActive = true
                });
            }

            if (createAdd)
            {
                permissionsToAdd.Add(new Permission
                {
                    Name = $"Add {module.DisplayName}",
                    Description = $"Permission to add new {module.DisplayName} data",
                    ModuleId = module.Id,
                    IsActive = true
                });
            }

            if (createEdit)
            {
                permissionsToAdd.Add(new Permission
                {
                    Name = $"Edit {module.DisplayName}",
                    Description = $"Permission to edit {module.DisplayName} data",
                    ModuleId = module.Id,
                    IsActive = true
                });
            }

            if (createDelete)
            {
                permissionsToAdd.Add(new Permission
                {
                    Name = $"Delete {module.DisplayName}",
                    Description = $"Permission to delete {module.DisplayName} data",
                    ModuleId = module.Id,
                    IsActive = true
                });
            }

            // Add the permissions to the database
            if (permissionsToAdd.Count > 0)
            {
                try
                {
                    _context.Permissions.AddRange(permissionsToAdd);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"{permissionsToAdd.Count} permissions created successfully for {module.DisplayName} module.";
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"Error generating permissions: {ex.Message}");
                    TempData["ErrorMessage"] = "Failed to create permissions. Please try again.";
                }
            }
            else
            {
                TempData["WarningMessage"] = "No permissions were selected to generate.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ModuleExists(int id)
        {
            return _context.Modules.Any(e => e.Id == id);
        }
    }
}