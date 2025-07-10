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
    public class PurchaseNatureController : Controller
    {
        private readonly IPurchaseNatureService _purchaseNatureService;
        private readonly ILogger<PurchaseNatureController> _logger;

        public PurchaseNatureController(
            IPurchaseNatureService purchaseNatureService,
            ILogger<PurchaseNatureController> logger)
        {
            _purchaseNatureService = purchaseNatureService;
            _logger = logger;
        }

        // GET: Master/PurchaseNature
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var purchaseNatures = await _purchaseNatureService.GetPurchaseNaturesAsync(showInactive, searchTerm);

                var viewModel = new PurchaseNatureIndexViewModel
                {
                    PurchaseNatures = purchaseNatures.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchaseNatures");
                TempData["ErrorMessage"] = "An error occurred while loading the purchaseNatures.";
                return View(new PurchaseNatureIndexViewModel());
            }
        }

        // GET: Master/PurchaseNature/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var purchaseNature = await _purchaseNatureService.GetPurchaseNatureByIdAsync(id);
                if (purchaseNature == null)
                {
                    return NotFound();
                }

                return View(purchaseNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchaseNature details for ID {PurchaseNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the purchaseNature details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/PurchaseNature/Create
        public IActionResult Create()
        {
            var model = new PurchaseNatureViewModel();
            return View(model);
        }

        // POST: Master/PurchaseNature/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseNatureViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if purchaseNature name already exists
                    if (await _purchaseNatureService.PurchaseNatureExistsAsync(model.PurchaseNatureName))
                    {
                        ModelState.AddModelError("PurchaseNatureName", "A purchaseNature with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _purchaseNatureService.SavePurchaseNatureAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "PurchaseNature created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create purchaseNature.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchaseNature");
                ModelState.AddModelError("", "An error occurred while creating the purchaseNature.");
                return View(model);
            }
        }

        // GET: Master/PurchaseNature/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var purchaseNature = await _purchaseNatureService.GetPurchaseNatureByIdAsync(id);
                if (purchaseNature == null)
                {
                    return NotFound();
                }

                return View(purchaseNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchaseNature for edit with ID {PurchaseNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the purchaseNature.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PurchaseNature/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, PurchaseNatureViewModel model)
        {
            if (id != model.PurchaseNatureId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _purchaseNatureService.PurchaseNatureExistsAsync(model.PurchaseNatureName, id))
                    {
                        ModelState.AddModelError("PurchaseNatureName", "A purchaseNature with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _purchaseNatureService.SavePurchaseNatureAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "PurchaseNature updated successfully.";
                        return Json(new { success = true, message = "PurchaseNature updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update purchaseNature.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchaseNature with ID {PurchaseNatureId}", id);
                ModelState.AddModelError("", "An error occurred while updating the purchaseNature.");
                return PartialView(model);
            }
        }

        // GET: Master/PurchaseNature/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var purchaseNature = await _purchaseNatureService.GetPurchaseNatureByIdAsync(id);
                if (purchaseNature == null)
                {
                    return NotFound();
                }

                return View(purchaseNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchaseNature for delete with ID {PurchaseNatureId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the purchaseNature.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/PurchaseNature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _purchaseNatureService.DeletePurchaseNatureAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "PurchaseNature deleted successfully.";
                    return Json(new { success = true, message = "PurchaseNature deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete purchaseNature." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete purchaseNature with ID {PurchaseNatureId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this purchaseNature." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting purchaseNature with ID {PurchaseNatureId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the purchaseNature." });
            }
        }

        // AJAX endpoint for checking if purchaseNature name exists
        [HttpGet]
        public async Task<JsonResult> CheckPurchaseNatureNameExists(string purchaseNatureName, short? excludeId = null)
        {
            try
            {
                var exists = await _purchaseNatureService.PurchaseNatureExistsAsync(purchaseNatureName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking purchaseNature name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}