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
    public class SubCategoryController : Controller
    {
        private readonly ISubCategoryService _subCategoryService;
        private readonly ILogger<SubCategoryController> _logger;

        public SubCategoryController(
            ISubCategoryService subCategoryService,
            ILogger<SubCategoryController> logger)
        {
            _subCategoryService = subCategoryService;
            _logger = logger;
        }

        // GET: Master/SubCategory
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var subCategories = await _subCategoryService.GetSubCategoriesAsync(showInactive, searchTerm);

                var viewModel = new SubCategoryIndexViewModel
                {
                    SubCategories = subCategories.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sub categories");
                TempData["ErrorMessage"] = "An error occurred while loading the sub categories.";
                return View(new SubCategoryIndexViewModel());
            }
        }

        // GET: Master/SubCategory/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(id);
                if (subCategory == null)
                {
                    return NotFound();
                }

                return View(subCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sub category details for ID {SubCategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the sub category details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/SubCategory/Create
        public IActionResult Create()
        {
            var model = new SubCategoryViewModel();
            return View(model);
        }

        // POST: Master/SubCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if sub category name already exists
                    if (await _subCategoryService.SubCategoryExistsAsync(model.SubCategoryName))
                    {
                        ModelState.AddModelError("SubCategoryName", "A sub category with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _subCategoryService.SaveSubCategoryAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Sub category created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create sub category.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sub category");
                ModelState.AddModelError("", "An error occurred while creating the sub category.");
                return View(model);
            }
        }

        // GET: Master/SubCategory/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(id);
                if (subCategory == null)
                {
                    return NotFound();
                }

                return View(subCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sub category for edit with ID {SubCategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the sub category.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SubCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SubCategoryViewModel model)
        {
            if (id != model.SubCategoryId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if product name already exists (excluding current record)
                    if (await _subCategoryService.SubCategoryExistsAsync(model.SubCategoryName, id))
                    {
                        ModelState.AddModelError("SubCategoryName", "A sub category with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _subCategoryService.SaveSubCategoryAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "SubCategory updated successfully.";
                        return Json(new { success = true, message = "SubCategory updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update subCategory.");
                    }
                }

                
                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subCategory with ID {SubCategoryId}", id);
                ModelState.AddModelError("", "An error occurred while updating the subCategory.");
                return PartialView(model);
            }
        }

        // GET: Master/SubCategory/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(id);
                if (subCategory == null)
                {
                    return NotFound();
                }

                return View(subCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sub category for delete with ID {SubCategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the sub category.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SubCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _subCategoryService.DeleteSubCategoryAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "SubCategory deleted successfully.";
                    return Json(new { success = true, message = "SubCategory deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete subCategory." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete subCategory with ID {SubCategoryId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this subCategory." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting subCategory with ID {SubCategoryId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the subCategory." });
            }
        }

        // AJAX endpoint for checking if subCategory name exists
        [HttpGet]
        public async Task<JsonResult> CheckSubCategoryNameExists(string subCategoryName, short? excludeId = null)
        {
            try
            {
                var exists = await _subCategoryService.SubCategoryExistsAsync(subCategoryName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking sub category name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}