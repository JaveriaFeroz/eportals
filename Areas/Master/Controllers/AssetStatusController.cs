// AssetStatusController.cs
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ProcureToPay.Areas.Master.Models;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class AssetStatusController : Controller
    {
        private readonly IAssetStatusService _assetStatusService;
        private readonly ILogger<AssetStatusController> _logger;

        public AssetStatusController(IAssetStatusService assetStatusService, ILogger<AssetStatusController> logger)
        {
            _assetStatusService = assetStatusService;
            _logger = logger;
        }

        // GET: Master/AssetStatus
        public async Task<IActionResult> Index(AssetStatusIndexViewModel model)
        {
            try
            {
                var assetStatuses = await _assetStatusService.GetAssetStatusesAsync(model.ShowInactiveOnly, model.SearchTerm);
                model.AssetStatuses = assetStatuses.ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading asset statuses index");
                TempData["ErrorMessage"] = "An error occurred while loading asset statuses.";
                return View(new AssetStatusIndexViewModel());
            }
        }

        // GET: Master/AssetStatus/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var assetStatus = await _assetStatusService.GetAssetStatusByIdAsync(id);
                if (assetStatus == null)
                {
                    return NotFound();
                }
                return View(assetStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading asset type details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading asset type details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/AssetStatus/Create
        public IActionResult Create()
        {
            return View(new AssetStatusViewModel());
        }

        // POST: Master/AssetStatus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetStatusViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _assetStatusService.AssetStatusExistsAsync(model.StatusName))
                    {
                        ModelState.AddModelError("StatusName", "Asset status name already exists.");
                    }
                    else
                    {
                        var result = await _assetStatusService.SaveAssetStatusAsync(model);
                        if (result)
                        {
                            TempData["SuccessMessage"] = "Asset status created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to create asset status.";
                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset status");
                TempData["ErrorMessage"] = "An error occurred while creating the asset status.";
                return View(model);
            }
        }

        // GET: Master/AssetStatus/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var assetStatus = await _assetStatusService.GetAssetStatusByIdAsync(id);
                if (assetStatus == null)
                {
                    return NotFound();
                }

                // Check if the status is editable
                if (!assetStatus.Editable)
                {
                    TempData["ErrorMessage"] = "This asset status cannot be edited.";
                    return RedirectToAction(nameof(Index));
                }

                return View(assetStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit asset status form for ID {StatusId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the asset status.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/AssetStatus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, AssetStatusViewModel model)
        {
            // The property name in your ViewModel is likely ChargeId
            if (id != model.StatusId)
            {
                return NotFound();
            }

            // Check for duplicate Charge Code *before* checking the entire model state
            // This provides a better user experience
            if (await _assetStatusService.AssetStatusExistsAsync(model.StatusName, model.StatusId))
            {
                ModelState.AddModelError("AssetStatus", "A asset status with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _assetStatusService.SaveAssetStatusAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Asset status updated successfully.";
                        return Json(new { success = true, message = "Asset Status updated successfully." });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update asset status.";
                        ModelState.AddModelError("", "Failed to update asset status. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating asset status with ID {ChargeId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the activity.");
                }
            }

            
            return PartialView(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _assetStatusService.DeleteAssetStatusAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Asset status deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete asset status.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset status with ID {ChargeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the asset status.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}