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
    public class WarningTypeController : Controller
    {
        private readonly IWarningTypeService _warningTypeService;
        private readonly ILogger<WarningTypeController> _logger;

        public WarningTypeController(
            IWarningTypeService warningTypeService,
            ILogger<WarningTypeController> logger)
        {
            _warningTypeService = warningTypeService;
            _logger = logger;
        }

        // GET: Master/WarningType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var warningTypes = await _warningTypeService.GetWarningTypesAsync(showInactive, searchTerm);

                var viewModel = new WarningTypeIndexViewModel
                {
                    WarningTypes = warningTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warningTypes");
                TempData["ErrorMessage"] = "An error occurred while loading the warningTypes.";
                return View(new WarningTypeIndexViewModel());
            }
        }

        // GET: Master/WarningType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var warningType = await _warningTypeService.GetWarningTypeByIdAsync(id);
                if (warningType == null)
                {
                    return NotFound();
                }

                return View(warningType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warningType details for ID {WarningTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the warningType details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/WarningType/Create
        public IActionResult Create()
        {
            var model = new WarningTypeViewModel();
            return View(model);
        }

        // POST: Master/WarningType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarningTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if warningType name already exists
                    if (await _warningTypeService.WarningTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("WarningTypeName", "A warningType with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _warningTypeService.SaveWarningTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "WarningType created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create warningType.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warningType");
                ModelState.AddModelError("", "An error occurred while creating the warningType.");
                return View(model);
            }
        }

        // GET: Master/WarningType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var warningType = await _warningTypeService.GetWarningTypeByIdAsync(id);
                if (warningType == null)
                {
                    return NotFound();
                }

                return View(warningType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warningType for edit with ID {WarningTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the warningType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/WarningType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, WarningTypeViewModel model)
        {
            if (id != model.TypeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _warningTypeService.WarningTypeExistsAsync(model.TypeName, id))
                    {
                        ModelState.AddModelError("WarningTypeName", "A warningType with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _warningTypeService.SaveWarningTypeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "WarningType updated successfully.";
                        return Json(new { success = true, message = "WarningType updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update warningType.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warningType with ID {WarningTypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the warningType.");
                return PartialView(model);
            }
        }

        // GET: Master/WarningType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var warningType = await _warningTypeService.GetWarningTypeByIdAsync(id);
                if (warningType == null)
                {
                    return NotFound();
                }

                return View(warningType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warningType for delete with ID {WarningTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the warningType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/WarningType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _warningTypeService.DeleteWarningTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "WarningType deleted successfully.";
                    return Json(new { success = true, message = "WarningType deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete warningType." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete warningType with ID {WarningTypeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this warningType." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting warningType with ID {WarningTypeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the warningType." });
            }
        }

        // AJAX endpoint for checking if warningType name exists
        [HttpGet]
        public async Task<JsonResult> CheckWarningTypeNameExists(string warningTypeName, short? excludeId = null)
        {
            try
            {
                var exists = await _warningTypeService.WarningTypeExistsAsync(warningTypeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking warningType name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}