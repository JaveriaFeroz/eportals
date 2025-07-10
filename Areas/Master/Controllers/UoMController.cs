using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class UoMController : Controller
    {
        private readonly IUoMService _uomService;
        private readonly ILogger<UoMController> _logger;

        public UoMController(
            IUoMService uomService,
            ILogger<UoMController> logger)
        {
            _uomService = uomService;
            _logger = logger;
        }

        // GET: Master/UoM
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var uoms = await _uomService.GetUoMsAsync(showInactive, searchTerm);

                var viewModel = new UoMIndexViewModel
                {
                    UoMs = uoms.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading UoMs");
                TempData["ErrorMessage"] = "An error occurred while loading the units of measure.";
                return View(new UoMIndexViewModel());
            }
        }

        // GET: Master/UoM/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var uom = await _uomService.GetUoMByIdAsync(id);
                if (uom == null)
                {
                    return NotFound();
                }

                return View(uom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading UoM details for ID {UoMId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the unit of measure details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/UoM/Create
        public IActionResult Create()
        {
            var model = new UoMViewModel();
            return View(model);
        }

        // POST: Master/UoM/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UoMViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if UoM name already exists
                    if (await _uomService.UoMExistsAsync(model.UoMName))
                    {
                        ModelState.AddModelError("UoMName", "A unit of measure with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _uomService.SaveUoMAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Unit of measure created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create unit of measure.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating UoM");
                ModelState.AddModelError("", "An error occurred while creating the unit of measure.");
                return View(model);
            }
        }

        // GET: Master/UoM/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var uom = await _uomService.GetUoMByIdAsync(id);
                if (uom == null)
                {
                    return NotFound();
                }

                return View(uom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading UoM for edit with ID {UoMId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the unit of measure.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, UoMViewModel model)
        {
            if (id != model.UoMId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if product nature name already exists (excluding current record)
                    if (await _uomService.UoMExistsAsync(model.UoMName, id))
                    {
                        ModelState.AddModelError("UoMName", "A unit of measure with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _uomService.SaveUoMAsync(model);

                    if (success)
                    {
                        return Json(new { success = true, message = "Unit of measure updated successfully." });
                    }
                    else
                    {
                        // This case is for unexpected save failures (database error, etc.)
                        ModelState.AddModelError("", "Failed to update unit of measure. Please try again.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unit of measure with ID {UoMId}", id);
                ModelState.AddModelError("", "An error occurred while updating the unit of measure.");
                return View(model);
            }
        }

        // GET: Master/UoM/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var uom = await _uomService.GetUoMByIdAsync(id);
                if (uom == null)
                {
                    return NotFound();
                }

                return View(uom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading UoM for delete with ID {UoMId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the unit of measure.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/UoM/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _uomService.DeleteUoMAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Unit of measure deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete unit of measure.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting UoM with ID {UoMId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the unit of measure.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX endpoint for checking if UoM name exists
        [HttpGet]
        public async Task<JsonResult> CheckUoMNameExists(string uomName, short? excludeId = null)
        {
            try
            {
                var exists = await _uomService.UoMExistsAsync(uomName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking UoM name existence");
                return Json(new { exists = false, error = true });
            }
        }

        // API endpoint for getting active UoMs (for dropdowns)
        [HttpGet]
        public async Task<JsonResult> GetActiveUoMs()
        {
            try
            {
                var uoms = await _uomService.GetActiveUoMsAsync();
                return Json(uoms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active UoMs");
                return Json(new { error = true, message = "Failed to load units of measure" });
            }
        }
    }
}