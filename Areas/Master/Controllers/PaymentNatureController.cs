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
    public class PaymentNatureController : Controller
    {
        private readonly IPaymentNatureService _paymentNatureService;
        private readonly ILogger<PaymentNatureController> _logger;

        public PaymentNatureController(
            IPaymentNatureService paymentNatureService,
            ILogger<PaymentNatureController> logger)
        {
            _paymentNatureService = paymentNatureService;
            _logger = logger;
        }

        // GET: Master/PaymentNature
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var paymentNatures = await _paymentNatureService.GetPaymentNaturesAsync(showInactive, searchTerm);

                var viewModel = new PaymentNatureIndexViewModel
                {
                    PaymentNatures = paymentNatures.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentNatures");
                TempData["ErrorMessage"] = "An error occurred while loading the paymentNatures.";
                return View(new PaymentNatureIndexViewModel());
            }
        }

        // GET: Master/PaymentNature/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var paymentNature = await _paymentNatureService.GetPaymentNatureByIdAsync(id);
                if (paymentNature == null)
                {
                    return NotFound();
                }

                return View(paymentNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentNature details for ID {PaymentNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentNature details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/PaymentNature/Create
        public IActionResult Create()
        {
            var model = new PaymentNatureViewModel();
            return View(model);
        }

        // POST: Master/PaymentNature/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentNatureViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if paymentNature name already exists
                    if (await _paymentNatureService.PaymentNatureExistsAsync(model.PaymentNatureName))
                    {
                        ModelState.AddModelError("PaymentNatureName", "A paymentNature with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _paymentNatureService.SavePaymentNatureAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "PaymentNature created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create paymentNature.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating paymentNature");
                ModelState.AddModelError("", "An error occurred while creating the paymentNature.");
                return View(model);
            }
        }

        // GET: Master/PaymentNature/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var paymentNature = await _paymentNatureService.GetPaymentNatureByIdAsync(id);
                if (paymentNature == null)
                {
                    return NotFound();
                }

                return View(paymentNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentNature for edit with ID {PaymentNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentNature.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PaymentNature/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, PaymentNatureViewModel model)
        {
            if (id != model.PaymentNatureId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _paymentNatureService.PaymentNatureExistsAsync(model.PaymentNatureName, id))
                    {
                        ModelState.AddModelError("PaymentNatureName", "A paymentNature with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _paymentNatureService.SavePaymentNatureAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "PaymentNature updated successfully.";
                        return Json(new { success = true, message = "PaymentNature updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update paymentNature.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating paymentNature with ID {PaymentNatureId}", id);
                ModelState.AddModelError("", "An error occurred while updating the paymentNature.");
                return PartialView(model);
            }
        }

        // GET: Master/PaymentNature/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var paymentNature = await _paymentNatureService.GetPaymentNatureByIdAsync(id);
                if (paymentNature == null)
                {
                    return NotFound();
                }

                return View(paymentNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentNature for delete with ID {PaymentNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentNature.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PaymentNature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _paymentNatureService.DeletePaymentNatureAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "PaymentNature deleted successfully.";
                    return Json(new { success = true, message = "PaymentNature deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete paymentNature." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete paymentNature with ID {PaymentNatureId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this paymentNature." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting paymentNature with ID {PaymentNatureId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the paymentNature." });
            }
        }

        // AJAX endpoint for checking if paymentNature name exists
        [HttpGet]
        public async Task<JsonResult> CheckPaymentNatureNameExists(string paymentNatureName, short? excludeId = null)
        {
            try
            {
                var exists = await _paymentNatureService.PaymentNatureExistsAsync(paymentNatureName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking paymentNature name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}