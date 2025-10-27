using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels; // Ensure you have a using for ViewModels
using ProcureToPay.Helpers;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly ILogger<AssetController> _logger;

        public AssetController(IAssetService assetService, ILogger<AssetController> logger)
        {
            _assetService = assetService;
            _logger = logger;
        }


        private async Task LoadDropdowns(AssetCreateEditViewModel viewModel)
        {
            var lookups = await _assetService.GetLookupsAsync();

            viewModel.AssetTypes = lookups.AssetTypes.Select(c => new SelectListItem
            {
                Text = c.TypeName,
                Value = c.TypeId.ToString()
            }).ToList();

            viewModel.AssetStatuses = lookups.AssetStatuses.Select(c => new SelectListItem
            {
                Text = c.StatusName,
                Value = c.StatusId.ToString()
            }).ToList();
            viewModel.Capacities = lookups.Capacities.Select(c => new SelectListItem
            {
                Text = c.CapacityName,
                Value = c.CapacityId.ToString()
            }).ToList();
            viewModel.Makes = lookups.Makes.Select(c => new SelectListItem
            {
                Text = c.MakeName,
                Value = c.MakeId.ToString()
            }).ToList();
            viewModel.LeaseTypes = lookups.LeaseTypes.Select(c => new SelectListItem
            {
                Text = c.TypeName,
                Value = c.TypeId.ToString()
            }).ToList();
            viewModel.Trailers = lookups.Trailers.Select(c => new SelectListItem
            {
                Text = c.TrailerName,
                Value = c.TrailerId.ToString()
            }).ToList();
            viewModel.Suppliers = lookups.Suppliers.Select(c => new SelectListItem
            {
                Text = c.SupplierName,
                Value = c.SupplierId.ToString()
            }).ToList();
            viewModel.Drivers = lookups.Drivers.Select(c => new SelectListItem
            {
                Text = c.DriverName,
                Value = c.DriverId.ToString()
            }).ToList();
           

            viewModel.Cities = lookups.Cities.Select(c => new SelectListItem
            {
                Text = c.CityName,
                Value = c.CityId.ToString()
            }).ToList();

            viewModel.Clients = lookups.Clients.Select(c => new SelectListItem
            {
                Text = c.ClientName,
                Value = c.ClientId.ToString()
            }).ToList();
            viewModel.Bases = lookups.Bases.Select(c => new SelectListItem
            {
                Text = c.BaseName,
                Value = c.BaseId.ToString()
            }).ToList();
           
            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();

        }

        // 1. INDEX
        // GET: Master/Asset
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var assets = await _assetService.GetAssetsAsync(showInactiveOnly, searchTerm);
                var viewModel = new AssetIndexViewModel
                {
                    Assets = assets.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assets index");
                TempData["ErrorMessage"] = "Error loading assets. Please try again.";
                return View(new AssetIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Master/Asset/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new AssetCreateEditViewModel();

            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        // POST: Master/Asset/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the asset data via 'model.Asset'
                if (ModelState.IsValid)
                {
                    if (await _assetService.AssetExistsAsync(model.Asset.AssetNo))
                    {
                        ModelState.AddModelError("Asset.AssetNumber", "Asset number already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.Asset.CreatedBy = currentUserId;
                        model.Asset.UpdatedBy = currentUserId;
                        model.Asset.CreatedOn = DateTimeHelper.GetPakistanStandardTime();
                        model.Asset.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                        // Pass the correct object (model.Asset) to the service
                        var success = await _assetService.SaveAssetAsync(model.Asset);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "Asset created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create asset. Please try again.");
                        }
                    }
                }

                // If we reach here, validation failed. We must reload the dropdowns
                // before returning the view, otherwise it will crash.
                // await LoadDropdowns(model); // You need a LoadDropdowns method here
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset");
                TempData["ErrorMessage"] = "Error creating asset. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Master/Asset/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            var viewModel = new AssetCreateEditViewModel { Asset = asset };
            await LoadDropdowns(viewModel);

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Master/Asset/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, AssetCreateEditViewModel viewModel)
        {
            if (id != viewModel.Asset.AssetId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _assetService.AssetExistsAsync(viewModel.Asset.AssetNo, id))
            {
                ModelState.AddModelError("Asset.AssetNo", "This Number is used by another asset.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.Asset.UpdatedBy = currentUserId;
                    viewModel.Asset.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                    var success = await _assetService.SaveAssetAsync(viewModel.Asset);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Asset updated successfully.";
                        return Json(new { success = true, message = "Asset updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the asset could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating asset with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the asset.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Master/Asset/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null) return NotFound();
                return View(asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading asset details for ID {AssetId}", id);
                TempData["ErrorMessage"] = "Error loading asset details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Master/Asset/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset == null) return NotFound();
                return View(asset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete asset page for ID {AssetId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Asset/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _assetService.DeleteAssetAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Asset deleted successfully.";
                    return Json(new { success = true, message = "Asset deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete asset.";
                    return Json(new { success = false, message = "Failed to delete asset. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting asset with ID {AssetId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the asset.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        public async Task<IActionResult> CheckAssetNameExists(string assetName, short? excludeId = null)
        {
            try
            {
                var exists = await _assetService.AssetExistsAsync(assetName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking asset name existence");
                return Json(new { error = "Error checking asset name" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _assetService.GetLookupsAsync();
                return Json(lookups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                return Json(new { error = "Error retrieving lookups" });
            }
        }
    }
}