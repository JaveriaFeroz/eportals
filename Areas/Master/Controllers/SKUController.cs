using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class SKUController : Controller
    {
        private readonly ISKUService _skuService;
        private readonly ILogger<SKUController> _logger;

        public SKUController(ISKUService skuService, ILogger<SKUController> logger)
        {
            _skuService = skuService;
            _logger = logger;
        }

        private async Task LoadDropdowns(SKUCreateEditViewModel viewModel)
        {
            var lookups = await _skuService.GetLookupsAsync();

            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();

            // FIX: Populate the Types dropdown
            viewModel.Types = lookups.Types.Select(t => new SelectListItem
            {
                Text = t.SKUTypeName,
                Value = t.SKUTypeId.ToString()
            }).ToList();
        }

        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                // FIX: Called the correct service method.
                var skus = await _skuService.GetSKUsAsync(showInactiveOnly, searchTerm);
                var viewModel = new SKUIndexViewModel
                {
                    // FIX: Used the correct ViewModel property.
                    SKUs = skus.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SKUs index");
                TempData["ErrorMessage"] = "Error loading SKUs. Please try again.";
                return View(new SKUIndexViewModel());
            }
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new SKUCreateEditViewModel();
            await LoadDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SKUCreateEditViewModel model)
        {
            try
            {
                // FIX: Corrected validation logic to check SKUName.
                if (await _skuService.SKUNameExistsAsync(model.SKU.SKUName))
                {
                    ModelState.AddModelError("SKU.SKUName", "SKU name already exists.");
                }

                if (ModelState.IsValid)
                {
                    var success = await _skuService.SaveSKUAsync(model.SKU);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "SKU created successfully.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "Failed to create SKU. Please try again.");
                }

                await LoadDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SKU");
                TempData["ErrorMessage"] = "Error creating SKU. Please try again.";
                await LoadDropdowns(model);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(short id)
        {
            var sku = await _skuService.GetSKUByIdAsync(id);
            if (sku == null)
            {
                return NotFound();
            }

            var viewModel = new SKUCreateEditViewModel { SKU = sku };
            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SKUCreateEditViewModel viewModel)
        {
            // FIX: Corrected ID check to use SKUId.
            if (id != viewModel.SKU.SKUId)
            {
                return NotFound();
            }

            // FIX: Corrected validation logic.
            if (await _skuService.SKUNameExistsAsync(viewModel.SKU.SKUName, id))
            {
                ModelState.AddModelError("SKU.SKUName", "This name is used by another SKU.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.SKU.UpdatedBy = currentUserId;
                    viewModel.SKU.UpdatedOn = DateTime.UtcNow;
                    var success = await _skuService.SaveSKUAsync(viewModel.SKU);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "SKU updated successfully.";
                        return Json(new { success = true, message = "SKU updated successfully." });
                    }
                    ModelState.AddModelError("", "A database error occurred and the SKU could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating SKU with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the SKU.");
                }
            }

            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var sku = await _skuService.GetSKUByIdAsync(id);
                if (sku == null) return NotFound();
                return View(sku);
            }
            catch (Exception ex)
            {
                // FIX: Corrected log message.
                _logger.LogError(ex, "Error loading SKU details for ID {SKUId}", id);
                TempData["ErrorMessage"] = "Error loading SKU details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var sku = await _skuService.GetSKUByIdAsync(id);
                if (sku == null) return NotFound();
                return View(sku);
            }
            catch (Exception ex)
            {
                // FIX: Corrected log message.
                _logger.LogError(ex, "Error loading delete SKU page for ID {SKUId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _skuService.DeleteSKUAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "SKU deleted successfully.";
                    return Json(new { success = true, message = "SKU deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete SKU.";
                    return Json(new { success = false, message = "Failed to delete SKU. It may have already been removed." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SKU with ID {SKUId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the SKU.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckSKUNameExists(string skuName, short? excludeId = null)
        {
            try
            {
                // FIX: Called the correct service method.
                var exists = await _skuService.SKUNameExistsAsync(skuName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SKU name existence");
                return Json(new { error = "Error checking SKU name" });
            }
        }
    }
}