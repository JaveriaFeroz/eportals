using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels; // Ensure you have a using for ViewModels
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class ProductNatureController : Controller
    {
        private readonly IProductNatureService _productNatureService;
        private readonly ILogger<ProductNatureController> _logger;

        public ProductNatureController(IProductNatureService productNatureService, ILogger<ProductNatureController> logger)
        {
            _productNatureService = productNatureService;
            _logger = logger;
        }


        private async Task LoadDropdowns(ProductNatureCreateEditViewModel viewModel)
        {
            var lookups = await _productNatureService.GetLookupsAsync();


            viewModel.PurchaseNatures = lookups.PurchaseNatures.Select(c => new SelectListItem
            {
                Text = c.PurchaseNatureName,
                Value = c.PurchaseNatureId.ToString()
            }).ToList();


        }

        // 1. INDEX
        // GET: Master/ProductNature
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var productNatures = await _productNatureService.GetProductNaturesAsync(showInactiveOnly, searchTerm);
                var viewModel = new ProductNatureIndexViewModel
                {
                    ProductNatures = productNatures.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading productNatures index");
                TempData["ErrorMessage"] = "Error loading productNatures. Please try again.";
                return View(new ProductNatureIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Master/ProductNature/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductNatureCreateEditViewModel();

            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        // POST: Master/ProductNature/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductNatureCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the productNature data via 'model.ProductNature'
                if (ModelState.IsValid)
                {
                    if (await _productNatureService.ProductNatureExistsAsync(model.ProductNature.NatureName))
                    {
                        ModelState.AddModelError("ProductNature.NatureName", "ProductNature name already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.ProductNature.CreatedBy = currentUserId;
                        model.ProductNature.UpdatedBy = currentUserId;
                        model.ProductNature.CreatedOn = DateTime.UtcNow;
                        model.ProductNature.UpdatedOn = DateTime.UtcNow;

                        // Pass the correct object (model.ProductNature) to the service
                        var success = await _productNatureService.SaveProductNatureAsync(model.ProductNature);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "ProductNature created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create productNature. Please try again.");
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
                _logger.LogError(ex, "Error creating productNature");
                TempData["ErrorMessage"] = "Error creating productNature. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Master/ProductNature/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var productNature = await _productNatureService.GetProductNatureByIdAsync(id);
            if (productNature == null)
            {
                return NotFound();
            }

            var viewModel = new ProductNatureCreateEditViewModel { ProductNature = productNature };
            await LoadDropdowns(viewModel);

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Master/ProductNature/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ProductNatureCreateEditViewModel viewModel)
        {
            if (id != viewModel.ProductNature.NatureId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _productNatureService.ProductNatureExistsAsync(viewModel.ProductNature.NatureName, id))
            {
                ModelState.AddModelError("ProductNature.NatureName", "This name is used by another productNature.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.ProductNature.UpdatedBy = currentUserId;
                    viewModel.ProductNature.UpdatedOn = DateTime.UtcNow;

                    var success = await _productNatureService.SaveProductNatureAsync(viewModel.ProductNature);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "ProductNature updated successfully.";
                        return Json(new { success = true, message = "ProductNature updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the productNature could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating productNature with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the productNature.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Master/ProductNature/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var productNature = await _productNatureService.GetProductNatureByIdAsync(id);
                if (productNature == null) return NotFound();
                return View(productNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading productNature details for ID {CategoryId}", id);
                TempData["ErrorMessage"] = "Error loading productNature details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Master/ProductNature/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var productNature = await _productNatureService.GetProductNatureByIdAsync(id);
                if (productNature == null) return NotFound();
                return View(productNature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete productNature page for ID {CategoryId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/ProductNature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _productNatureService.DeleteProductNatureAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "ProductNature deleted successfully.";
                    return Json(new { success = true, message = "ProductNature deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete productNature.";
                    return Json(new { success = false, message = "Failed to delete productNature. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting productNature with ID {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the productNature.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckNatureNameExists(string productNatureName, short? excludeId = null)
        {
            try
            {
                var exists = await _productNatureService.ProductNatureExistsAsync(productNatureName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking productNature name existence");
                return Json(new { error = "Error checking productNature name" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _productNatureService.GetLookupsAsync();
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