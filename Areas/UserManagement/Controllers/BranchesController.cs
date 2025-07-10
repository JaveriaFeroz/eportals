using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    // [Authorize(Roles = "Admin")]
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public BranchesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserManagement/Branches
        public async Task<IActionResult> Index()
        {
            return View(await _context.Branches.ToListAsync());
        }

        // GET: UserManagement/Branches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(m => m.BranchId == id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // GET: UserManagement/Branches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserManagement/Branches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {

            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Property: {state.Key}, Errors: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");

                }
            }

            // Custom validation for duplicate BranchName
            if (await _context.Branches.AnyAsync(b => b.BranchName == branch.BranchName))
            {
                ModelState.AddModelError("BranchName", "A branch with this name already exists.");
            }

            // Custom validation for duplicate BranchCode
            if (await _context.Branches.AnyAsync(b => b.BranchCode == branch.BranchCode))
            {
                ModelState.AddModelError("BranchCode", "A branch with this code already exists.");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    branch.CreatedByUserId = currentUser.Id;
                }
                _context.Add(branch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        // GET: UserManagement/Branches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }
            return View(branch);
        }

        // POST: UserManagement/Branches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BranchId,BranchName,BranchCode,IsActive")] Branch branch)
        {
            if (id != branch.BranchId)
            {
                return Json(new { success = false, message = "Branch ID mismatch." });
            }

            // Custom validation for duplicate BranchName
            if (await _context.Branches.AnyAsync(b => b.BranchName == branch.BranchName && b.BranchId != branch.BranchId))
            {
                ModelState.AddModelError("BranchName", "A branch with this name already exists.");
            }

            // Custom validation for duplicate BranchCode
            if (await _context.Branches.AnyAsync(b => b.BranchCode == branch.BranchCode && b.BranchId != branch.BranchId))
            {
                ModelState.AddModelError("BranchCode", "A branch with this code already exists.");
            }

            // Add this to log ModelState errors to the console during debugging
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Property: {state.Key}, Errors: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBranch = await _context.Branches.FindAsync(branch.BranchId);
                    if (existingBranch == null)
                    {
                        return Json(new { success = false, message = "Branch not found for update." });
                    }

                    existingBranch.BranchName = branch.BranchName;
                    existingBranch.BranchCode = branch.BranchCode;
                    existingBranch.IsActive = branch.IsActive;

                    _context.Update(existingBranch);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Branch updated successfully!" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branch.BranchId))
                    {
                        return Json(new { success = false, message = "The branch was deleted by another user." });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The branch was modified by another user. Please refresh and try again.");
                        return PartialView(branch);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating branch: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during update.");
                    return PartialView(branch);
                }
            }

            return PartialView(branch);
        }

        // GET: UserManagement/Branches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(m => m.BranchId == id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // POST: UserManagement/Branches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.BranchId == id);
        }
    }
}
