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
    public class PaymentModeController : Controller
    {
        private readonly IPaymentModeService _paymentModeService;
        private readonly ILogger<PaymentModeController> _logger;

        public PaymentModeController(
            IPaymentModeService paymentModeService,
            ILogger<PaymentModeController> logger)
        {
            _paymentModeService = paymentModeService;
            _logger = logger;
        }

        // GET: Master/PaymentMode
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var paymentModes = await _paymentModeService.GetPaymentModesAsync(showInactive, searchTerm);

                var viewModel = new PaymentModeIndexViewModel
                {
                    PaymentModes = paymentModes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentModes");
                TempData["ErrorMessage"] = "An error occurred while loading the paymentModes.";
                return View(new PaymentModeIndexViewModel());
            }
        }

        // GET: Master/PaymentMode/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var paymentMode = await _paymentModeService.GetPaymentModeByIdAsync(id);
                if (paymentMode == null)
                {
                    return NotFound();
                }

                return View(paymentMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentMode details for ID {PaymentModeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentMode details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/PaymentMode/Create
        public IActionResult Create()
        {
            var model = new PaymentModeViewModel();
            return View(model);
        }

        // POST: Master/PaymentMode/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentModeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if paymentMode name already exists
                    if (await _paymentModeService.PaymentModeExistsAsync(model.PaymentModeName))
                    {
                        ModelState.AddModelError("PaymentModeName", "A paymentMode with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _paymentModeService.SavePaymentModeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "PaymentMode created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create paymentMode.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating paymentMode");
                ModelState.AddModelError("", "An error occurred while creating the paymentMode.");
                return View(model);
            }
        }

        // GET: Master/PaymentMode/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var paymentMode = await _paymentModeService.GetPaymentModeByIdAsync(id);
                if (paymentMode == null)
                {
                    return NotFound();
                }

                return View(paymentMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentMode for edit with ID {PaymentModeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentMode.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PaymentMode/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, PaymentModeViewModel model)
        {
            if (id != model.PaymentModeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _paymentModeService.PaymentModeExistsAsync(model.PaymentModeName, id))
                    {
                        ModelState.AddModelError("PaymentModeName", "A paymentMode with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _paymentModeService.SavePaymentModeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "PaymentMode updated successfully.";
                        return Json(new { success = true, message = "PaymentMode updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update paymentMode.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating paymentMode with ID {PaymentModeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the paymentMode.");
                return PartialView(model);
            }
        }

        // GET: Master/PaymentMode/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var paymentMode = await _paymentModeService.GetPaymentModeByIdAsync(id);
                if (paymentMode == null)
                {
                    return NotFound();
                }

                return View(paymentMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentMode for delete with ID {PaymentModeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentMode.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PaymentMode/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _paymentModeService.DeletePaymentModeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "PaymentMode deleted successfully.";
                    return Json(new { success = true, message = "PaymentMode deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete paymentMode." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete paymentMode with ID {PaymentModeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this paymentMode." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting paymentMode with ID {PaymentModeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the paymentMode." });
            }
        }

        // AJAX endpoint for checking if paymentMode name exists
        [HttpGet]
        public async Task<JsonResult> CheckPaymentModeNameExists(string paymentModeName, short? excludeId = null)
        {
            try
            {
                var exists = await _paymentModeService.PaymentModeExistsAsync(paymentModeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking paymentMode name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}