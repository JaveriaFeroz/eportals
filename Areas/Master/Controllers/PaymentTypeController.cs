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
    public class PaymentTypeController : Controller
    {
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly ILogger<PaymentTypeController> _logger;

        public PaymentTypeController(
            IPaymentTypeService paymentTypeService,
            ILogger<PaymentTypeController> logger)
        {
            _paymentTypeService = paymentTypeService;
            _logger = logger;
        }

        // GET: Master/PaymentType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var paymentTypes = await _paymentTypeService.GetPaymentTypesAsync(showInactive, searchTerm);

                var viewModel = new PaymentTypeIndexViewModel
                {
                    PaymentTypes = paymentTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentTypes");
                TempData["ErrorMessage"] = "An error occurred while loading the paymentTypes.";
                return View(new PaymentTypeIndexViewModel());
            }
        }

        // GET: Master/PaymentType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id);
                if (paymentType == null)
                {
                    return NotFound();
                }

                return View(paymentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentType details for ID {PaymentTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentType details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/PaymentType/Create
        public IActionResult Create()
        {
            var model = new PaymentTypeViewModel();
            return View(model);
        }

        // POST: Master/PaymentType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if paymentType name already exists
                    if (await _paymentTypeService.PaymentTypeExistsAsync(model.PaymentTypeName))
                    {
                        ModelState.AddModelError("PaymentTypeName", "A paymentType with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _paymentTypeService.SavePaymentTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "PaymentType created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create paymentType.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating paymentType");
                ModelState.AddModelError("", "An error occurred while creating the paymentType.");
                return View(model);
            }
        }

        // GET: Master/PaymentType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id);
                if (paymentType == null)
                {
                    return NotFound();
                }

                return View(paymentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentType for edit with ID {PaymentTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PaymentType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, PaymentTypeViewModel model)
        {
            if (id != model.PaymentTypeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _paymentTypeService.PaymentTypeExistsAsync(model.PaymentTypeName, id))
                    {
                        ModelState.AddModelError("PaymentTypeName", "A paymentType with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _paymentTypeService.SavePaymentTypeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "PaymentType updated successfully.";
                        return Json(new { success = true, message = "PaymentType updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update paymentType.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating paymentType with ID {PaymentTypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the paymentType.");
                return PartialView(model);
            }
        }

        // GET: Master/PaymentType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id);
                if (paymentType == null)
                {
                    return NotFound();
                }

                return View(paymentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paymentType for delete with ID {PaymentTypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the paymentType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PaymentType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _paymentTypeService.DeletePaymentTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "PaymentType deleted successfully.";
                    return Json(new { success = true, message = "PaymentType deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete paymentType." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete paymentType with ID {PaymentTypeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this paymentType." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting paymentType with ID {PaymentTypeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the paymentType." });
            }
        }

        // AJAX endpoint for checking if paymentType name exists
        [HttpGet]
        public async Task<JsonResult> CheckPaymentTypeNameExists(string paymentTypeName, short? excludeId = null)
        {
            try
            {
                var exists = await _paymentTypeService.PaymentTypeExistsAsync(paymentTypeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking paymentType name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}