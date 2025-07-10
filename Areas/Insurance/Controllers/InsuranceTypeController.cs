using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Insurance.Models;
using ProcureToPay.Areas.Insurance.Services;
using ProcureToPay.Areas.Insurance.ViewModels; // Ensure you have a using for ViewModels
using ProcureToPay.Areas.Master.ViewModels;
using ProcureToPay.Areas.Master.Services;
using System.Security.Claims;

namespace ProcureToPay.Areas.Insurance.Controllers
{
    [Area("Insurance")]
    [Authorize]
    public class InsuranceTypeController : Controller
    {
        private readonly IInsuranceTypeService _insuranceTypeService;
        private readonly ILogger<InsuranceTypeController> _logger;

        public InsuranceTypeController(IInsuranceTypeService insuranceTypeService, ILogger<InsuranceTypeController> logger)
        {
            _insuranceTypeService = insuranceTypeService;
            _logger = logger;
        }


        private async Task LoadDropdowns(InsuranceTypeCreateEditViewModel viewModel)
        {
            var lookups = await _insuranceTypeService.GetLookupsAsync();


            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();


        }

        [HttpGet]
        public async Task<IActionResult> LoadRequiredDocuments()
        {
            var model = new InsuranceDocumentTypeIndexViewModel();
            model.InsuranceDocumentTypes = await _insuranceTypeService.GetRequiredDocumentsAsync();
            return PartialView("RequiredDocument", model);
        }


        // 1. INDEX
        // GET: Insurance/InsuranceType
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var insuranceTypes = await _insuranceTypeService.GetInsuranceTypesAsync(showInactiveOnly, searchTerm);
                var viewModel = new InsuranceTypeIndexViewModel
                {
                    InsuranceTypes = insuranceTypes.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insuranceTypes index");
                TempData["ErrorMessage"] = "Error loading insuranceTypes. Please try again.";
                return View(new InsuranceTypeIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Insurance/InsuranceType/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new InsuranceTypeCreateEditViewModel();

            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        // POST: Insurance/InsuranceType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceTypeCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the insuranceType data via 'model.InsuranceType'
                if (ModelState.IsValid)
                {
                    if (await _insuranceTypeService.TypeNameExistsAsync(model.InsuranceType.TypeName))
                    {
                        ModelState.AddModelError("InsuranceType.TypeName", "InsuranceType name already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.InsuranceType.CreatedBy = currentUserId;
                        model.InsuranceType.UpdatedBy = currentUserId;
                        model.InsuranceType.CreatedOn = DateTime.UtcNow;
                        model.InsuranceType.UpdatedOn = DateTime.UtcNow;

                        // Pass the correct object (model.InsuranceType) to the service
                        var success = await _insuranceTypeService.SaveInsuranceTypeAsync(model.InsuranceType);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "InsuranceType created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create insuranceType. Please try again.");
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
                _logger.LogError(ex, "Error creating insuranceType");
                TempData["ErrorMessage"] = "Error creating insuranceType. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Insurance/InsuranceType/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var insuranceType = await _insuranceTypeService.GetInsuranceTypeByIdAsync(id);
            if (insuranceType == null)
            {
                return NotFound();
            }

            var viewModel = new InsuranceTypeCreateEditViewModel { InsuranceType = insuranceType };
            await LoadDropdowns(viewModel);

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Insurance/InsuranceType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, InsuranceTypeCreateEditViewModel viewModel)
        {
            if (id != viewModel.InsuranceType.TypeId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _insuranceTypeService.TypeNameExistsAsync(viewModel.InsuranceType.TypeName, id))
            {
                ModelState.AddModelError("InsuranceType.TypeName", "This name is used by another insuranceType.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.InsuranceType.UpdatedBy = currentUserId;
                    viewModel.InsuranceType.UpdatedOn = DateTime.UtcNow;

                    var success = await _insuranceTypeService.SaveInsuranceTypeAsync(viewModel.InsuranceType);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "InsuranceType updated successfully.";
                        return Json(new { success = true, message = "InsuranceType updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the insuranceType could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating insuranceType with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the insuranceType.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Insurance/InsuranceType/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var insuranceType = await _insuranceTypeService.GetInsuranceTypeByIdAsync(id);
                if (insuranceType == null) return NotFound();
                return View(insuranceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insuranceType details for ID {TypeId}", id);
                TempData["ErrorMessage"] = "Error loading insuranceType details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Insurance/InsuranceType/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var insuranceType = await _insuranceTypeService.GetInsuranceTypeByIdAsync(id);
                if (insuranceType == null) return NotFound();
                return View(insuranceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete insuranceType page for ID {TypeId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Insurance/InsuranceType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _insuranceTypeService.DeleteInsuranceTypeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "InsuranceType deleted successfully.";
                    return Json(new { success = true, message = "InsuranceType deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete insuranceType.";
                    return Json(new { success = false, message = "Failed to delete insuranceType. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting insuranceType with ID {TypeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the insuranceType.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckTypeNameExists(string insuranceTypeName, short? excludeId = null)
        {
            try
            {
                var exists = await _insuranceTypeService.TypeNameExistsAsync(insuranceTypeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking insuranceType name existence");
                return Json(new { error = "Error checking insuranceType name" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _insuranceTypeService.GetLookupsAsync();
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