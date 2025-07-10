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
    public class ChargeController : Controller
    {
        private readonly IChargeService _chargeService;
        private readonly ILogger<ChargeController> _logger;

        public ChargeController(
            IChargeService chargeService,
            ILogger<ChargeController> logger)
        {
            _chargeService = chargeService;
            _logger = logger;
        }

        // GET: Master/Charge
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var charges = await _chargeService.GetChargesAsync(showInactive, searchTerm);

                var viewModel = new ChargeIndexViewModel
                {
                    Charges = charges.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading charges");
                TempData["ErrorMessage"] = "An error occurred while loading the charges.";
                return View(new ChargeIndexViewModel());
            }
        }

        // GET: Master/Charge/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var charge = await _chargeService.GetChargeByIdAsync(id);
                if (charge == null)
                {
                    return NotFound();
                }

                return View(charge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading charge details for ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the charge details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Charge/Create
        public IActionResult Create()
        {
            var model = new ChargeViewModel();
            return View(model);
        }

        // POST: Master/Charge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChargeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if charge name already exists
                    if (await _chargeService.ChargeExistsAsync(model.ChargeName))
                    {
                        ModelState.AddModelError("ChargeName", "A charge with this name already exists.");
                        return View(model);
                    }

                    var success = await _chargeService.SaveChargeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Charge created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create charge.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating charge");
                ModelState.AddModelError("", "An error occurred while creating the charge.");
                return View(model);
            }
        }

        // GET: Master/Charge/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var charge = await _chargeService.GetChargeByIdAsync(id);
                if (charge == null)
                {
                    return NotFound();
                }

                return View(charge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading charge for edit with ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the charge.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Charge/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ChargeViewModel model)
        {
            if (id != model.ChargeId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if charge name already exists (excluding current record)
                    if (await _chargeService.ChargeExistsAsync(model.ChargeName, id))
                    {
                        ModelState.AddModelError("ChargeName", "A charge with this name already exists.");
                        return PartialView(model);
                    }


                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _chargeService.SaveChargeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Charge updated successfully.";
                        return Json(new { success = true, message = "Charge updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update charge.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating charge with ID {ChargeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the charge.");
                return PartialView(model);
            }
        }

        // GET: Master/Charge/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var charge = await _chargeService.GetChargeByIdAsync(id);
                if (charge == null)
                {
                    return NotFound();
                }

                return View(charge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading charge for delete with ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the charge.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Charge/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _chargeService.DeleteChargeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Charge deleted successfully.";
                    return Json(new { success = true, message = "Charge deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete charge." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete charge with ID {ChargeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this charge." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting charge with ID {ChargeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the charge." });
            }
        }

        // GET: Master/Charge/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(short id)
        {
            try
            {
                var charge = await _chargeService.GetChargeByIdAsync(id);
                if (charge == null)
                {
                    return Json(new { success = false, message = "Charge not found." });
                }

                // Toggle the status
                charge.IsActive = !charge.IsActive;
                var success = await _chargeService.SaveChargeAsync(charge);

                if (success)
                {
                    var statusText = charge.IsActive ? "activated" : "deactivated";
                    return Json(new
                    {
                        success = true,
                        message = $"Charge '{charge.ChargeName}' {statusText} successfully.",
                        isActive = charge.IsActive
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update charge status." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for charge with ID {ChargeId}", id);
                return Json(new { success = false, message = "An error occurred while updating the charge status." });
            }
        }

        // GET: Master/Charge/CheckDuplicate
        [HttpGet]
        public async Task<IActionResult> CheckDuplicate(string chargeName, short? excludeId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(chargeName))
                {
                    return Json(new { exists = false });
                }

                var exists = await _chargeService.ChargeExistsAsync(chargeName.Trim(), excludeId);
                return Json(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate charge name");
                return Json(new { exists = false, error = true });
            }
        }
    }
}