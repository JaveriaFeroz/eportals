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
    public class SeparationTypeController : Controller
    {
        private readonly ISeparationTypeService _separationTypeService;
        private readonly ILogger<SeparationTypeController> _logger;

        public SeparationTypeController(
            ISeparationTypeService separationTypeService,
            ILogger<SeparationTypeController> logger)
        {
            _separationTypeService = separationTypeService;
            _logger = logger;
        }

        // GET: Master/SeparationType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var separationTypes = await _separationTypeService.GetSeparationTypesAsync(showInactive, searchTerm);

                var viewModel = new SeparationTypeIndexViewModel
                {
                    SeparationTypes = separationTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading separationTypes");
                TempData["ErrorMessage"] = "An error occurred while loading the separationTypes.";
                return View(new SeparationTypeIndexViewModel());
            }
        }

        // GET: Master/SeparationType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var separationType = await _separationTypeService.GetSeparationTypeByIdAsync(id);
                if (separationType == null)
                {
                    return NotFound();
                }

                return View(separationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading separationType details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the separationType details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/SeparationType/Create
        public IActionResult Create()
        {
            var model = new SeparationTypeViewModel();
            return View(model);
        }

        // POST: Master/SeparationType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SeparationTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if separationType name already exists
                    if (await _separationTypeService.SeparationTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("TypeName", "A separationType with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _separationTypeService.SaveSeparationTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "SeparationType created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create separationType.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating separationType");
                ModelState.AddModelError("", "An error occurred while creating the separationType.");
                return View(model);
            }
        }

        // GET: Master/SeparationType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var separationType = await _separationTypeService.GetSeparationTypeByIdAsync(id);
                if (separationType == null)
                {
                    return NotFound();
                }

                return View(separationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading separationType for edit with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the separationType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SeparationType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SeparationTypeViewModel model)
        {
            if (id != model.TypeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _separationTypeService.SeparationTypeExistsAsync(model.TypeName, id))
                    {
                        ModelState.AddModelError("TypeName", "A separationType with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _separationTypeService.SaveSeparationTypeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "SeparationType updated successfully.";
                        return Json(new { success = true, message = "SeparationType updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update separationType.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating separationType with ID {TypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the separationType.");
                return PartialView(model);
            }
        }

        // GET: Master/SeparationType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var separationType = await _separationTypeService.GetSeparationTypeByIdAsync(id);
                if (separationType == null)
                {
                    return NotFound();
                }

                return View(separationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading separationType for delete with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the separationType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SeparationType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _separationTypeService.DeleteSeparationTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "SeparationType deleted successfully.";
                    return Json(new { success = true, message = "SeparationType deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete separationType." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete separationType with ID {TypeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this separationType." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting separationType with ID {TypeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the separationType." });
            }
        }

        // AJAX endpoint for checking if separationType name exists
        [HttpGet]
        public async Task<JsonResult> CheckTypeNameExists(string TypeName, short? excludeId = null)
        {
            try
            {
                var exists = await _separationTypeService.SeparationTypeExistsAsync(TypeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking separationType name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}