using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class ProductTypeController : Controller
    {
        private readonly IProductTypeService _productTypeService;
        private readonly ILogger<ProductTypeController> _logger;

        public ProductTypeController(
            IProductTypeService productTypeService,
            ILogger<ProductTypeController> logger)
        {
            _productTypeService = productTypeService;
            _logger = logger;
        }

        // GET: Master/ProductType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var productTypes = await _productTypeService.GetProductTypesAsync(showInactive, searchTerm);

                var viewModel = new ProductTypeIndexViewModel
                {
                    ProductTypes = productTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product types");
                TempData["ErrorMessage"] = "An error occurred while loading the product types.";
                return View(new ProductTypeIndexViewModel());
            }
        }

        // GET: Master/ProductType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var productType = await _productTypeService.GetProductTypeByIdAsync(id);
                if (productType == null)
                {
                    return NotFound();
                }

                return View(productType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product type details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the product type details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/ProductType/Create
        public IActionResult Create()
        {
            var model = new ProductTypeViewModel();
            return View(model);
        }

        // POST: Master/ProductType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if product type already exists
                    if (await _productTypeService.ProductTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("TypeName", "A product type with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _productTypeService.SaveProductTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Product type created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create product type.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product type");
                ModelState.AddModelError("", "An error occurred while creating the product type.");
                return View(model);
            }
        }

        // GET: Master/ProductType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var productType = await _productTypeService.GetProductTypeByIdAsync(id);
                if (productType == null)
                {
                    return NotFound();
                }

                return View(productType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product type for edit with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the product type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/ProductType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ProductTypeViewModel model)
        {
            if (id != model.TypeId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if product type name already exists (excluding current record)
                    if (await _productTypeService.ProductTypeExistsAsync(model.TypeName, id))
                    {
                        ModelState.AddModelError("TypeName", "A product type with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _productTypeService.SaveProductTypeAsync(model);

                    if (success)
                    {
                        return Json(new { success = true, message = "Product type updated successfully." });
                    }
                    else
                    {
                        // This case is for unexpected save failures (database error, etc.)
                        ModelState.AddModelError("", "Failed to update product type. Please try again.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product type with ID {TypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the product type.");
                return View(model);
            }
        }

        // GET: Master/ProductType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var productType = await _productTypeService.GetProductTypeByIdAsync(id);
                if (productType == null)
                {
                    return NotFound();
                }

                return View(productType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product type for delete with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the product type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/ProductType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _productTypeService.DeleteProductTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Product type deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete product type.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product type with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the product type.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX endpoint for checking if product type name exists
        [HttpGet]
        public async Task<JsonResult> CheckTypeNameExists(string typeName, short? excludeId = null)
        {
            try
            {
                var exists = await _productTypeService.ProductTypeExistsAsync(typeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product type name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}