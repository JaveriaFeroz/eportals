using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Net;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class MakeController : Controller
    {
        private readonly IMakeService _makeService;
        private readonly ILogger<MakeController> _logger;

        public MakeController(
            IMakeService makeService,
            ILogger<MakeController> logger)
        {
            _makeService = makeService;
            _logger = logger;
        }

        // GET: Master/Make
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var makes = await _makeService.GetMakesAsync(showInactive, searchTerm);

                var viewModel = new MakeIndexViewModel
                {
                    Makes = makes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading makes");
                TempData["ErrorMessage"] = "An error occurred while loading the makes.";
                return View(new MakeIndexViewModel());
            }
        }

        // GET: Master/Make/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var make = await _makeService.GetMakeByIdAsync(id);
                if (make == null)
                {
                    return NotFound();
                }

                return View(make);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading make details for ID {MakeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the make details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Make/Create
        public IActionResult Create()
        {
            var model = new MakeViewModel();
            return View(model);
        }

        // POST: Master/Make/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MakeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if make name already exists
                    if (await _makeService.MakeExistsAsync(model.MakeName))
                    {
                        ModelState.AddModelError("MakeName", "A make with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _makeService.SaveMakeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Make created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create make.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating make");
                ModelState.AddModelError("", "An error occurred while creating the make.");
                return View(model);
            }
        }

        // GET: Master/Make/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var make = await _makeService.GetMakeByIdAsync(id);
                if (make == null)
                {
                    return NotFound();
                }

                return View(make);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading make for edit with ID {MakeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the make.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Make/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, MakeViewModel model)
        {
            if (id != model.MakeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _makeService.MakeExistsAsync(model.MakeName, id))
                    {
                        ModelState.AddModelError("MakeName", "A make with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _makeService.SaveMakeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Make updated successfully.";
                        return Json(new { success = true, message = "Make updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update make.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating make with ID {MakeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the make.");
                return PartialView(model);
            }
        }

        // GET: Master/Make/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var make = await _makeService.GetMakeByIdAsync(id);
                if (make == null)
                {
                    return NotFound();
                }

                return View(make);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading make for delete with ID {MakeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the make.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Make/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _makeService.DeleteMakeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Make deleted successfully.";
                    return Json(new { success = true, message = "Make deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete make." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete make with ID {MakeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this make." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting make with ID {MakeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the make." });
            }
        }

        // AJAX endpoint for checking if make name exists
        [HttpGet]
        public async Task<JsonResult> CheckMakeNameExists(string makeName, short? excludeId = null)
        {
            try
            {
                var exists = await _makeService.MakeExistsAsync(makeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking make name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}