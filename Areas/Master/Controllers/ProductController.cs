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
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: Master/Product
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var products = await _productService.GetProductsAsync(showInactive, searchTerm);

                var viewModel = new ProductIndexViewModel
                {
                    Products = products.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["ErrorMessage"] = "An error occurred while loading the products.";
                return View(new ProductIndexViewModel());
            }
        }

        // GET: Master/Product/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details for ID {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Product/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new ProductViewModel();
                var lookups = await _productService.GetLookupsAsync();

                // Load dropdowns
                model.ProductTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.ProductTypes, "TypeId", "TypeName");
                model.UoMs = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.UoMs, "UoMId", "UoMName");
                model.ProductNatures = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.ProductNatures, "NatureId", "NatureName");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create product form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if product name already exists
                    if (await _productService.ProductExistsAsync(model.ProductName))
                    {
                        ModelState.AddModelError("ProductName", "A product with this name already exists.");
                        await LoadDropdownsForModel(model);
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _productService.SaveProductAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Product created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create product.");
                    }
                }

                await LoadDropdownsForModel(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "An error occurred while creating the product.");
                await LoadDropdownsForModel(model);
                return View(model);
            }
        }

        // GET: Master/Product/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for edit with ID {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ProductViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if product name already exists (excluding current record)
                    if (await _productService.ProductExistsAsync(model.ProductName, id))
                    {
                        ModelState.AddModelError("ProductName", "A product with this name already exists.");
                        await LoadDropdownsForModel(model);
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _productService.SaveProductAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Product updated successfully.";
                        return Json(new { success = true, message = "Product updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update product.");
                    }
                }

                await LoadDropdownsForModel(model);
                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                ModelState.AddModelError("", "An error occurred while updating the product.");
                await LoadDropdownsForModel(model);
                return PartialView(model);
            }
        }


        // GET: Master/Product/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for delete with ID {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the product.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Product deleted successfully.";
                    return Json(new { success = true, message = "Product deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete product." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete product with ID {ProductId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this product." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the product." });
            }
        }

        // AJAX endpoint for checking if product name exists
        [HttpGet]
        public async Task<JsonResult> CheckProductNameExists(string productName, short? excludeId = null)
        {
            try
            {
                var exists = await _productService.ProductExistsAsync(productName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product name existence");
                return Json(new { exists = false, error = true });
            }
        }

        // API endpoint for getting lookups (similar to your original GetLookups)
        [HttpGet]
        public async Task<JsonResult> GetLookups()
        {
            try
            {
                var lookups = await _productService.GetLookupsAsync();
                return Json(new
                {
                    lstProductType = lookups.ProductTypes,
                    lstUoM = lookups.UoMs,
                    lstProductNature = lookups.ProductNatures
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookups");
                return Json(new { error = ex.Message });
            }
        }

        private async Task LoadDropdownsForModel(ProductViewModel model)
        {
            try
            {
                var lookups = await _productService.GetLookupsAsync();
                model.ProductTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.ProductTypes, "TypeId", "TypeName", model.ProductTypeId);
                model.UoMs = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.UoMs, "UoMId", "UoMName", model.UoMId);
                model.ProductNatures = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(lookups.ProductNatures, "NatureId", "NatureName", model.ProductNatureId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdowns for model");
            }
        }
    }
}