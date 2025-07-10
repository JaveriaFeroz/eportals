using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Areas.Insurance.Services;
using ProcureToPay.Areas.Insurance.ViewModels;
using ProcureToPay.Areas.Master.ViewModels;
using System.Net;
using System.Security.Claims;

namespace ProcureToPay.Areas.Insurance.Controllers
{
    [Area("Insurance")]
    [Authorize]
    public class InsuranceCompanyController : Controller
    {
        private readonly IInsuranceCompanyService _insuranceCompanyService;
        private readonly ILogger<InsuranceCompanyController> _logger;

        public InsuranceCompanyController(
            IInsuranceCompanyService insuranceCompanyService,
            ILogger<InsuranceCompanyController> logger)
        {
            _insuranceCompanyService = insuranceCompanyService;
            _logger = logger;
        }

        // GET: Insurance/InsuranceCompany
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var insuranceCompanys = await _insuranceCompanyService.GetInsuranceCompaniesAsync(showInactive, searchTerm);

                var viewModel = new InsuranceCompanyIndexViewModel
                {
                    InsuranceCompanies = insuranceCompanys.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insuranceCompanys");
                TempData["ErrorMessage"] = "An error occurred while loading the insuranceCompanys.";
                return View(new InsuranceCompanyIndexViewModel());
            }
        }

        // GET: Insurance/InsuranceCompany/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var insuranceCompany = await _insuranceCompanyService.GetInsuranceCompanyByIdAsync(id);
                if (insuranceCompany == null)
                {
                    return NotFound();
                }

                return View(insuranceCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insuranceCompany details for ID {CompanyIdF}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the insuranceCompany details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Insurance/InsuranceCompany/Create
        public IActionResult Create()
        {
            var model = new InsuranceCompanyViewModel();
            return View(model);
        }

        // POST: Insurance/InsuranceCompany/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceCompanyViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if insuranceCompany name already exists
                    if (await _insuranceCompanyService.InsuranceCompanyExistsAsync(model.CompanyName))
                    {
                        ModelState.AddModelError("CompanyName", "A insuranceCompany with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _insuranceCompanyService.SaveInsuranceCompanyAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Insurance Company created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create insurance Company.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating insurance Company");
                ModelState.AddModelError("", "An error occurred while creating the insurance Company.");
                return View(model);
            }
        }

        // GET: Insurance/InsuranceCompany/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var insuranceCompany = await _insuranceCompanyService.GetInsuranceCompanyByIdAsync(id);
                if (insuranceCompany == null)
                {
                    return NotFound();
                }

                return View(insuranceCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insurance Company for edit with ID {CompanyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the insurance Company.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Insurance/InsuranceCompany/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, InsuranceCompanyViewModel model)
        {
            if (id != model.CompanyId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _insuranceCompanyService.InsuranceCompanyExistsAsync(model.CompanyName, id))
                    {
                        ModelState.AddModelError("Insurance CompanyName", "A contractor with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _insuranceCompanyService.SaveInsuranceCompanyAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Insurance Company updated successfully.";
                        return Json(new { success = true, message = "Insurance Company updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update contractor.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contractor with ID {InsuranceCompanyId}", id);
                ModelState.AddModelError("", "An error occurred while updating the contractor.");
                return PartialView(model);
            }
        }

        // GET: Insurance/InsuranceCompany/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var insuranceCompany = await _insuranceCompanyService.GetInsuranceCompanyByIdAsync(id);
                if (insuranceCompany == null)
                {
                    return NotFound();
                }

                return View(insuranceCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading insurance Company for delete with ID {CompanyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the insuranceCompany.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Insurance/InsuranceCompany/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _insuranceCompanyService.DeleteInsuranceCompanyAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Insurance Company deleted successfully.";
                    return Json(new { success = true, message = "Insurance Company deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete insurance Company." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete insurance Company with ID {CompanyId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this insurance Company." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting insurance Company with ID {CompanyId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the insuranceCompany." });
            }
        }

        // AJAX endpoint for checking if insuranceCompany name exists
        [HttpGet]
        public async Task<JsonResult> CheckCompanyNameExists(string insuranceCompanyName, short? excludeId = null)
        {
            try
            {
                var exists = await _insuranceCompanyService.InsuranceCompanyExistsAsync(insuranceCompanyName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking insuranceCompany name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}