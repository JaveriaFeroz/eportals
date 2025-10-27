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
    public class SKUCategoryController : Controller
    {
        private readonly ISKUCategoryService _skuCategoryService;
        private readonly ILogger<SKUCategoryController> _logger;

        public SKUCategoryController(ISKUCategoryService skuCategoryService, ILogger<SKUCategoryController> logger)
        {
            _skuCategoryService = skuCategoryService;
            _logger = logger;
        }


        private async Task LoadDropdowns(SKUCategoryCreateEditViewModel viewModel)
        {
            var lookups = await _skuCategoryService.GetLookupsAsync();


            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();

           
        }

        // 1. INDEX
        // GET: Master/SKUCategory
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var skuCategorys = await _skuCategoryService.GetSKUCategoriesAsync(showInactiveOnly, searchTerm);
                var viewModel = new SKUCategoryIndexViewModel
                {
                    Categories = skuCategorys.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading skuCategorys index");
                TempData["ErrorMessage"] = "Error loading skuCategorys. Please try again.";
                return View(new SKUCategoryIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Master/SKUCategory/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new SKUCategoryCreateEditViewModel();

            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        // POST: Master/SKUCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SKUCategoryCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the skuCategory data via 'model.SKUCategory'
                if (ModelState.IsValid)
                {
                    if (await _skuCategoryService.CategoryNameExistsAsync(model.SKUCategory.CategoryName))
                    {
                        ModelState.AddModelError("SKUCategory.CategoryName", "SKUCategory name already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.SKUCategory.CreatedBy = currentUserId;
                        model.SKUCategory.UpdatedBy = currentUserId;
                        model.SKUCategory.CreatedOn = DateTimeHelper.GetPakistanStandardTime();
                        model.SKUCategory.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                        // Pass the correct object (model.SKUCategory) to the service
                        var success = await _skuCategoryService.SaveSKUCategoryAsync(model.SKUCategory);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "SKUCategory created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create skuCategory. Please try again.");
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
                _logger.LogError(ex, "Error creating skuCategory");
                TempData["ErrorMessage"] = "Error creating skuCategory. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Master/SKUCategory/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var skuCategory = await _skuCategoryService.GetSKUCategoryByIdAsync(id);
            if (skuCategory == null)
            {
                return NotFound();
            }

            var viewModel = new SKUCategoryCreateEditViewModel { SKUCategory = skuCategory };
            await LoadDropdowns(viewModel);

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Master/SKUCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SKUCategoryCreateEditViewModel viewModel)
        {
            if (id != viewModel.SKUCategory.CategoryId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _skuCategoryService.CategoryNameExistsAsync(viewModel.SKUCategory.CategoryName, id))
            {
                ModelState.AddModelError("SKUCategory.CategoryName", "This name is used by another skuCategory.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.SKUCategory.UpdatedBy = currentUserId;
                    viewModel.SKUCategory.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                    var success = await _skuCategoryService.SaveSKUCategoryAsync(viewModel.SKUCategory);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "SKUCategory updated successfully.";
                        return Json(new { success = true, message = "SKUCategory updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the skuCategory could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating skuCategory with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the skuCategory.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Master/SKUCategory/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var skuCategory = await _skuCategoryService.GetSKUCategoryByIdAsync(id);
                if (skuCategory == null) return NotFound();
                return View(skuCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading skuCategory details for ID {CategoryId}", id);
                TempData["ErrorMessage"] = "Error loading skuCategory details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Master/SKUCategory/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var skuCategory = await _skuCategoryService.GetSKUCategoryByIdAsync(id);
                if (skuCategory == null) return NotFound();
                return View(skuCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete skuCategory page for ID {CategoryId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SKUCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _skuCategoryService.DeleteSKUCategoryAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "SKUCategory deleted successfully.";
                    return Json(new { success = true, message = "SKUCategory deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete skuCategory.";
                    return Json(new { success = false, message = "Failed to delete skuCategory. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skuCategory with ID {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the skuCategory.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckCategoryNameExists(string skuCategoryName, short? excludeId = null)
        {
            try
            {
                var exists = await _skuCategoryService.CategoryNameExistsAsync(skuCategoryName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking skuCategory name existence");
                return Json(new { error = "Error checking skuCategory name" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _skuCategoryService.GetLookupsAsync();
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