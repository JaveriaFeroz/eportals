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
    public class RateTypeController : Controller
    {
        private readonly IRateTypeService _rateTypeService;
        private readonly ILogger<RateTypeController> _logger;

        public RateTypeController(
            IRateTypeService rateTypeService,
            ILogger<RateTypeController> logger)
        {
            _rateTypeService = rateTypeService;
            _logger = logger;
        }

        // GET: Master/RateType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var rateTypes = await _rateTypeService.GetRateTypesAsync(showInactive, searchTerm);

                var viewModel = new RateTypeIndexViewModel
                {
                    RateTypes = rateTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rateTypes");
                TempData["ErrorMessage"] = "An error occurred while loading the rateTypes.";
                return View(new RateTypeIndexViewModel());
            }
        }

        // GET: Master/RateType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var rateType = await _rateTypeService.GetRateTypeByIdAsync(id);
                if (rateType == null)
                {
                    return NotFound();
                }

                return View(rateType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rateType details for ID {RateTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the rateType details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/RateType/Create
        public IActionResult Create()
        {
            var model = new RateTypeViewModel();
            return View(model);
        }

        // POST: Master/RateType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RateTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if rateType name already exists
                    if (await _rateTypeService.RateTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("RateTypeName", "A rateType with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _rateTypeService.SaveRateTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "RateType created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create rateType.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rateType");
                ModelState.AddModelError("", "An error occurred while creating the rateType.");
                return View(model);
            }
        }

        // GET: Master/RateType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var rateType = await _rateTypeService.GetRateTypeByIdAsync(id);
                if (rateType == null)
                {
                    return NotFound();
                }

                return View(rateType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rateType for edit with ID {RateTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the rateType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/RateType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, RateTypeViewModel model)
        {
            if (id != model.TypeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _rateTypeService.RateTypeExistsAsync(model.TypeName, id))
                    {
                        ModelState.AddModelError("RateTypeName", "A rateType with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _rateTypeService.SaveRateTypeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "RateType updated successfully.";
                        return Json(new { success = true, message = "RateType updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update rateType.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rateType with ID {RateTypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the rateType.");
                return PartialView(model);
            }
        }

        // GET: Master/RateType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var rateType = await _rateTypeService.GetRateTypeByIdAsync(id);
                if (rateType == null)
                {
                    return NotFound();
                }

                return View(rateType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rateType for delete with ID {RateTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the rateType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/RateType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _rateTypeService.DeleteRateTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "RateType deleted successfully.";
                    return Json(new { success = true, message = "RateType deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete rateType." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete rateType with ID {RateTypeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this rateType." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting rateType with ID {RateTypeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the rateType." });
            }
        }

        // AJAX endpoint for checking if rateType name exists
        [HttpGet]
        public async Task<JsonResult> CheckRateTypeNameExists(string rateTypeName, short? excludeId = null)
        {
            try
            {
                var exists = await _rateTypeService.RateTypeExistsAsync(rateTypeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rateType name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}