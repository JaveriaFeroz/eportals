using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Data;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DepartmentsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserManagement/Departments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departments.ToListAsync());
        }

        // GET: UserManagement/Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: UserManagement/Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserManagement/Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Property: {state.Key}, Errors: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    // You can also use Debug.WriteLine if you prefer
                }
            }

            if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", "A department with this code already exists.");
            }

            // Custom validation for duplicate DepartmentCode
            if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", "A department with this code already exists.");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    department.CreatedByUserId = currentUser.Id;
                }
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: UserManagement/Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: UserManagement/Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,DepartmentName,DepartmentCode,IsActive")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            // Custom validation for duplicate DepartmentName
            if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName && d.DepartmentId != department.DepartmentId))
            {
                ModelState.AddModelError("DepartmentName", "A department with this name already exists.");
            }

            // Custom validation for duplicate DepartmentCode
            if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode && d.DepartmentId != department.DepartmentId))
            {
                ModelState.AddModelError("DepartmentCode", "A department with this code already exists.");
            }

            // Add this to log ModelState errors to the console during debugging, similar to your Create method
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Property: {state.Key}, Errors: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    // You can also use Debug.WriteLine if you prefer
                }
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing entity to avoid overwriting un-bound properties like CreatedDate, CreatedBy etc.
                    var existingDepartment = await _context.Departments.FindAsync(department.DepartmentId);
                    if (existingDepartment == null)
                    {
                        return NotFound(); // Department not found in DB during update
                    }

                    // Update only the properties that are being edited
                    existingDepartment.DepartmentName = department.DepartmentName;
                    existingDepartment.DepartmentCode = department.DepartmentCode;
                    existingDepartment.IsActive = department.IsActive;

                    _context.Update(existingDepartment); // Or _context.Entry(existingDepartment).State = EntityState.Modified;
                    await _context.SaveChangesAsync();


                    return Json(new { success = true, message = "Department updated successfully!" });
                    // --- END CHANGE ---
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If ModelState is not valid, return the view with the department object
            // This will display validation messages on the form.
            return PartialView(department);
        }
        // GET: UserManagement/Departments/Delete/5 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: UserManagement/Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.DepartmentId == id);
        }
    }
}
