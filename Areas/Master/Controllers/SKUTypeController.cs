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
    public class SKUTypeController : Controller
    {
        private readonly ISKUTypeService _skuTypeService;
        private readonly ILogger<SKUTypeController> _logger;

        public SKUTypeController(
            ISKUTypeService skuTypeService,
            ILogger<SKUTypeController> logger)
        {
            _skuTypeService = skuTypeService;
            _logger = logger;
        }

        // GET: Master/SKUType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var skuTypes = await _skuTypeService.GetSKUTypesAsync(showInactive, searchTerm);

                var viewModel = new SKUTypeIndexViewModel
                {
                    SKUTypes = skuTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading skuTypes");
                TempData["ErrorMessage"] = "An error occurred while loading the skuTypes.";
                return View(new SKUTypeIndexViewModel());
            }
        }

        // GET: Master/SKUType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var skuType = await _skuTypeService.GetSKUTypeByIdAsync(id);
                if (skuType == null)
                {
                    return NotFound();
                }

                return View(skuType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading skuType details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the skuType details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/SKUType/Create
        public IActionResult Create()
        {
            var model = new SKUTypeViewModel();
            return View(model);
        }

        // POST: Master/SKUType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SKUTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if skuType name already exists
                    if (await _skuTypeService.SKUTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("TypeName", "A skuType with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _skuTypeService.SaveSKUTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "SKUType created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create skuType.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skuType");
                ModelState.AddModelError("", "An error occurred while creating the skuType.");
                return View(model);
            }
        }

        // GET: Master/SKUType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var skuType = await _skuTypeService.GetSKUTypeByIdAsync(id);
                if (skuType == null)
                {
                    return NotFound();
                }

                return View(skuType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading skuType for edit with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the skuType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SKUType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SKUTypeViewModel model)
        {
            if (id != model.TypeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _skuTypeService.SKUTypeExistsAsync(model.TypeName, id))
                    {
                        ModelState.AddModelError("TypeName", "A skuType with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _skuTypeService.SaveSKUTypeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "SKUType updated successfully.";
                        return Json(new { success = true, message = "SKUType updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update skuType.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skuType with ID {TypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the skuType.");
                return PartialView(model);
            }
        }

        // GET: Master/SKUType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var skuType = await _skuTypeService.GetSKUTypeByIdAsync(id);
                if (skuType == null)
                {
                    return NotFound();
                }

                return View(skuType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading skuType for delete with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the skuType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/SKUType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _skuTypeService.DeleteSKUTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "SKUType deleted successfully.";
                    return Json(new { success = true, message = "SKUType deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete skuType." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete skuType with ID {TypeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this skuType." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting skuType with ID {TypeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the skuType." });
            }
        }

        // AJAX endpoint for checking if skuType name exists
        [HttpGet]
        public async Task<JsonResult> CheckTypeNameExists(string skuTypeName, short? excludeId = null)
        {
            try
            {
                var exists = await _skuTypeService.SKUTypeExistsAsync(skuTypeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking skuType name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}