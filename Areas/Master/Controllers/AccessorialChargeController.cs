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
    public class AccessorialChargeController : Controller
    {
        private readonly IAccessorialChargeService _accessorialChargeService;
        private readonly ILogger<AccessorialChargeController> _logger;

        public AccessorialChargeController(
            IAccessorialChargeService accessorialChargeService,
            ILogger<AccessorialChargeController> logger)
        {
            _accessorialChargeService = accessorialChargeService;
            _logger = logger;
        }

        // GET: Master/AccessorialCharge
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var charges = await _accessorialChargeService.GetAccessorialChargesAsync(showInactive, searchTerm);

                var viewModel = new AccessorialChargeIndexViewModel
                {
                    AccessorialCharges = charges.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accessorial charges");
                TempData["ErrorMessage"] = "An error occurred while loading the accessorial charges.";
                return View(new AccessorialChargeIndexViewModel());
            }
        }

        // GET: Master/AccessorialCharge/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var charge = await _accessorialChargeService.GetAccessorialChargeByIdAsync(id);
                if (charge == null)
                {
                    return NotFound();
                }

                return View(charge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accessorial charge details for ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the accessorial charge details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/AccessorialCharge/Create
        public IActionResult Create()
        {
            var model = new AccessorialChargeViewModel();
            return View(model);
        }

        // POST: Master/AccessorialCharge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccessorialChargeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if charge name already exists
                    if (await _accessorialChargeService.AccessorialChargeExistsAsync(model.ChargeName))
                    {
                        // Updated error message as per your request
                        ModelState.AddModelError("ChargeName", "A charge with this name already exists.");
                        return View(model);
                    }

                    // Check if charge code already exists
                    if (await _accessorialChargeService.AccessorialChargeExistsAsync(model.ChargeCode))
                    {
                        ModelState.AddModelError("ChargeCode", "A charge with this code already exists.");
                        return View(model);
                    }

                    // New check: Ensure ChargeName and ChargeCode are not the same in the input model
                    if (model.ChargeName.Equals(model.ChargeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("ChargeCode", "Charge Name and Charge Code cannot be the same.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _accessorialChargeService.SaveAccessorialChargeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Accessorial charge created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create accessorial charge.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accessorial charge");
                ModelState.AddModelError("", "An error occurred while creating the accessorial charge.");
                return View(model);
            }
        }

        // GET: Master/AccessorialCharge/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var charge = await _accessorialChargeService.GetAccessorialChargeByIdAsync(id);
                if (charge == null)
                {
                    return NotFound();
                }

                return View(charge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accessorial charge for edit with ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the accessorial charge.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/AccessorialCharge/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, AccessorialChargeViewModel model)
        {
            // The property name in your ViewModel is likely ChargeId
            if (id != model.ChargeId)
            {
                return NotFound();
            }

            // Check for duplicate Charge Code *before* checking the entire model state
            // This provides a better user experience
            if (await _accessorialChargeService.AccessorialChargeExistsAsync(model.ChargeCode, id))
            {
                ModelState.AddModelError("ChargeCode", "A charge with this code already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _accessorialChargeService.SaveAccessorialChargeAsync(model);

                    if (success)
                    {
                        // CHANGED: Return JSON for a successful AJAX call
                        return Json(new { success = true, message = "Accessorial charge updated successfully." });
                    }
                    else
                    {
                        // This case is for unexpected save failures (database error, etc.)
                        ModelState.AddModelError("", "Failed to update accessorial charge. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating accessorial charge with ID {ChargeId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the activity.");
                }
            }

            // If we reach here, ModelState is invalid.
            // CHANGED: Return a PartialView to show validation errors within the modal.
            // Make sure the filename "_EditAccessorialChargePartial" matches your actual file name.
            return PartialView(model);
        }

        // GET: Master/AccessorialCharge/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var charge = await _accessorialChargeService.GetAccessorialChargeByIdAsync(id);
                if (charge == null)
                {
                    return NotFound();
                }

                return View(charge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading accessorial charge for delete with ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the accessorial charge.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/AccessorialCharge/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _accessorialChargeService.DeleteAccessorialChargeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Accessorial charge deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete accessorial charge.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accessorial charge with ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the accessorial charge.";
                return RedirectToAction(nameof(Index));
            }
        }



        // AJAX endpoint for checking if charge code exists
        [HttpGet]
        public async Task<JsonResult> CheckChargeCodeExists(string chargeCode, short? excludeId = null)
        {
            try
            {
                var exists = await _accessorialChargeService.AccessorialChargeExistsAsync(chargeCode, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking charge code existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}