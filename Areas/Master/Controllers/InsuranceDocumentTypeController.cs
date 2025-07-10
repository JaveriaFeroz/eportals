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
    public class InsuranceDocumentTypeController : Controller
    {
        private readonly IInsuranceDocumentTypeService _insuranceDocumentTypeService;
        private readonly ILogger<InsuranceDocumentTypeController> _logger;

        public InsuranceDocumentTypeController(
            IInsuranceDocumentTypeService insuranceDocumentTypeService,
            ILogger<InsuranceDocumentTypeController> logger)
        {
            _insuranceDocumentTypeService = insuranceDocumentTypeService;
            _logger = logger;
        }

        // GET: Master/InsuranceDocumentType
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var insuranceDocumentTypes = await _insuranceDocumentTypeService.GetInsuranceDocumentTypesAsync(showInactive, searchTerm);

                var viewModel = new InsuranceDocumentTypeIndexViewModel
                {
                    InsuranceDocumentTypes = insuranceDocumentTypes.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insuranceDocumentTypes");
                TempData["ErrorMessage"] = "An error occurred while loading the insuranceDocumentTypes.";
                return View(new InsuranceDocumentTypeIndexViewModel());
            }
        }

        // GET: Master/InsuranceDocumentType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var insuranceDocumentType = await _insuranceDocumentTypeService.GetInsuranceDocumentTypeByIdAsync(id);
                if (insuranceDocumentType == null)
                {
                    return NotFound();
                }

                return View(insuranceDocumentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insurance Document Type details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the insurance Document Type details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/InsuranceDocumentType/Create
        public IActionResult Create()
        {
            var model = new InsuranceDocumentTypeViewModel();
            return View(model);
        }

        // POST: Master/InsuranceDocumentType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceDocumentTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if insurance Document Type name already exists
                    if (await _insuranceDocumentTypeService.InsuranceDocumentTypeExistsAsync(model.TypeName))
                    {
                        ModelState.AddModelError("TypeName", "A insurance Document Type with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _insuranceDocumentTypeService.SaveInsuranceDocumentTypeAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "InsuranceDocumentType created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create insuranceDocumentType.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating insuranceDocumentType");
                ModelState.AddModelError("", "An error occurred while creating the insuranceDocumentType.");
                return View(model);
            }
        }

        // GET: Master/InsuranceDocumentType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var insuranceDocumentType = await _insuranceDocumentTypeService.GetInsuranceDocumentTypeByIdAsync(id);
                if (insuranceDocumentType == null)
                {
                    return NotFound();
                }

                return View(insuranceDocumentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insurance Document Type for edit with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the insuranceDocumentType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/InsuranceDocumentType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, InsuranceDocumentTypeViewModel model)
        {
            if (id != model.TypeId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _insuranceDocumentTypeService.InsuranceDocumentTypeExistsAsync(model.TypeName, id))
                    {
                        ModelState.AddModelError("TypeName", "A insurance Document Type with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _insuranceDocumentTypeService.SaveInsuranceDocumentTypeAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "InsuranceDocumentType updated successfully.";
                        return Json(new { success = true, message = "InsuranceDocumentType updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update insuranceDocumentType.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating insurance Document Type with ID {TypeId}", id);
                ModelState.AddModelError("", "An error occurred while updating the insuranceDocumentType.");
                return PartialView(model);
            }
        }

        // GET: Master/InsuranceDocumentType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var insuranceDocumentType = await _insuranceDocumentTypeService.GetInsuranceDocumentTypeByIdAsync(id);
                if (insuranceDocumentType == null)
                {
                    return NotFound();
                }

                return View(insuranceDocumentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insurance Document Type for delete with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the insuranceDocumentType.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/InsuranceDocumentType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _insuranceDocumentTypeService.DeleteInsuranceDocumentTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Insurance Document Type deleted successfully.";
                    return Json(new { success = true, message = "Insurance Document Type deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete insuranceDocumentType." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete insurance Document Type with ID {TypeId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this insurance Document Type." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting insurance Document Type with ID {TypeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the insuranceDocumentType." });
            }
        }

        // AJAX endpoint for checking if insurance Document Type name exists
        [HttpGet]
        public async Task<JsonResult> CheckTypeNameExists(string insuranceDocumentTypeName, short? excludeId = null)
        {
            try
            {
                var exists = await _insuranceDocumentTypeService.InsuranceDocumentTypeExistsAsync(insuranceDocumentTypeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking insurance Document Type name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}