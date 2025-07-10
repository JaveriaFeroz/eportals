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
    public class CapacityController : Controller
    {
        private readonly ICapacityService _capacityService;
        private readonly ILogger<CapacityController> _logger;

        public CapacityController(
            ICapacityService capacityService,
            ILogger<CapacityController> logger)
        {
            _capacityService = capacityService;
            _logger = logger;
        }

        // GET: Master/Capacity
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var capacities = await _capacityService.GetCapacitiesAsync(!showInactive, searchTerm);

                var viewModel = new CapacityIndexViewModel
                {
                    Capacities = capacities.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading capacities");
                TempData["ErrorMessage"] = "An error occurred while loading the capacities.";
                return View(new CapacityIndexViewModel());
            }
        }

        // GET: Master/Capacity/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var capacity = await _capacityService.GetCapacityByIdAsync(id);
                if (capacity == null)
                {
                    return NotFound();
                }

                return View(capacity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading capacity details for ID {CapacityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the capacity details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Capacity/Create
        public IActionResult Create()
        {
            var model = new CapacityViewModel();
            return View(model);
        }

        // POST: Master/Capacity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CapacityViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if capacity name already exists
                    if (await _capacityService.CapacityExistsAsync(model.CapacityName))
                    {
                        ModelState.AddModelError("CapacityName", "A capacity with this name already exists.");
                        return View(model);
                    }

                    var success = await _capacityService.SaveCapacityAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Capacity created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create capacity.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating capacity");
                ModelState.AddModelError("", "An error occurred while creating the capacity.");
                return View(model);
            }
        }

        // GET: Master/Capacity/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var capacity = await _capacityService.GetCapacityByIdAsync(id);
                if (capacity == null)
                {
                    return NotFound();
                }

                return View(capacity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading capacity for edit with ID {CapacityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the capacity.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Capacity/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, CapacityViewModel model)
        {
            if (id != model.CapacityId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if capacity name already exists (excluding current record)
                    if (await _capacityService.CapacityExistsAsync(model.CapacityName, id))
                    {
                        ModelState.AddModelError("CapacityName", "A capacity with this name already exists.");
                        return View(model);
                    }

                    var success = await _capacityService.SaveCapacityAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Capacity updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update capacity.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating capacity with ID {CapacityId}", id);
                ModelState.AddModelError("", "An error occurred while updating the capacity.");
                return View(model);
            }
        }

        // GET: Master/Capacity/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var capacity = await _capacityService.GetCapacityByIdAsync(id);
                if (capacity == null)
                {
                    return NotFound();
                }

                return View(capacity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading capacity for delete with ID {CapacityId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the capacity.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Capacity/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _capacityService.DeleteCapacityAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Capacity deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete capacity.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting capacity with ID {CapacityId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the capacity.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX endpoint for checking if capacity name exists
        [HttpGet]
        public async Task<JsonResult> CheckCapacityNameExists(string capacityName, short? excludeId = null)
        {
            try
            {
                var exists = await _capacityService.CapacityExistsAsync(capacityName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking capacity name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}