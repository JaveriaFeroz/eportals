// AssetTypeController.cs
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class AssetTypeController : Controller
    {
        private readonly IAssetTypeService _assetTypeService;
        private readonly ILogger<AssetTypeController> _logger;

        public AssetTypeController(IAssetTypeService assetTypeService, ILogger<AssetTypeController> logger)
        {
            _assetTypeService = assetTypeService;
            _logger = logger;
        }

        // GET: Master/AssetType
        public async Task<IActionResult> Index(AssetTypeIndexViewModel model)
        {
            try
            {
                var assetTypes = await _assetTypeService.GetAssetTypesAsync(model.ShowInactiveOnly, model.SearchTerm);
                model.AssetTypes = assetTypes.ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading asset types index");
                TempData["ErrorMessage"] = "An error occurred while loading asset types.";
                return View(new AssetTypeIndexViewModel());
            }
        }

        // GET: Master/AssetType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var assetType = await _assetTypeService.GetAssetTypeByIdAsync(id);
                if (assetType == null)
                {
                    return NotFound();
                }
                return View(assetType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading asset type details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading asset type details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/AssetType/Create
        public IActionResult Create()
        {
            return View(new AssetTypeViewModel());
        }

        // POST: Master/AssetType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _assetTypeService.AssetTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("TypeName", "Asset type name already exists.");
                    }
                    else
                    {
                        var result = await _assetTypeService.SaveAssetTypeAsync(model);
                        if (result)
                        {
                            TempData["SuccessMessage"] = "Asset type created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to create asset type.";
                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset type");
                TempData["ErrorMessage"] = "An error occurred while creating the asset type.";
                return View(model);
            }
        }

        // GET: Master/AssetType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var assetType = await _assetTypeService.GetAssetTypeByIdAsync(id);
                if (assetType == null)
                {
                    return NotFound();
                }
                return View(assetType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit asset type form for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the asset type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/AssetType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, AssetTypeViewModel model)
        {
            if (id != model.TypeId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _assetTypeService.AssetTypeExistsAsync(model.TypeName, model.TypeId))
                    {
                        ModelState.AddModelError("TypeName", "Asset type name already exists.");
                    }
                    else
                    {
                        var result = await _assetTypeService.SaveAssetTypeAsync(model);
                        if (result)
                        {
                            TempData["SuccessMessage"] = "Asset type updated successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to update asset type.";
                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating asset type with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the asset type.";
                return View(model);
            }
        }

        // GET: Master/AssetType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var assetType = await _assetTypeService.GetAssetTypeByIdAsync(id);
                if (assetType == null)
                {
                    return NotFound();
                }
                return View(assetType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for asset type ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the asset type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/AssetType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var result = await _assetTypeService.DeleteAssetTypeAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Asset type deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete asset type.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset type with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the asset type.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}