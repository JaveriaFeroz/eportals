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
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(
            ICompanyService companyService,
            ILogger<CompanyController> logger)
        {
            _companyService = companyService;
            _logger = logger;
        }

        // GET: Master/Company
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var companies = await _companyService.GetCompaniesAsync(showInactive, searchTerm);

                var viewModel = new CompanyIndexViewModel
                {
                    Companies = companies.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading companies");
                TempData["ErrorMessage"] = "An error occurred while loading the companies.";
                return View(new CompanyIndexViewModel());
            }
        }

        // GET: Master/Company/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound();
                }

                return View(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading company details for ID {CompanyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the company details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Company/Create
        public IActionResult Create()
        {
            var model = new CompanyViewModel();
            return View(model);
        }

        // POST: Master/Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if company name already exists
                    if (await _companyService.CompanyExistsAsync(model.CompanyName))
                    {
                        ModelState.AddModelError("CompanyName", "A company with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _companyService.SaveCompanyAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Company created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create company.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                ModelState.AddModelError("", "An error occurred while creating the company.");
                return View(model);
            }
        }

        // GET: Master/Company/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound();
                }

                return View(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading company for edit with ID {CompanyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the company.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Company/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, CompanyViewModel model)
        {
            if (id != model.CompanyId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _companyService.CompanyExistsAsync(model.CompanyName, id))
                    {
                        ModelState.AddModelError("CompanyName", "A company with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _companyService.SaveCompanyAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Company updated successfully.";
                        return Json(new { success = true, message = "Company updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update company.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company with ID {CompanyId}", id);
                ModelState.AddModelError("", "An error occurred while updating the company.");
                return PartialView(model);
            }
        }

        // GET: Master/Company/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var company = await _companyService.GetCompanyByIdAsync(id);
                if (company == null)
                {
                    return NotFound();
                }

                return View(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading company for delete with ID {CompanyId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the company.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _companyService.DeleteCompanyAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Company deleted successfully.";
                    return Json(new { success = true, message = "Company deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete company." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete company with ID {CompanyId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this company." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting company with ID {CompanyId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the company." });
            }
        }

        // AJAX endpoint for checking if company name exists
        [HttpGet]
        public async Task<JsonResult> CheckCompanyNameExists(string companyName, short? excludeId = null)
        {
            try
            {
                var exists = await _companyService.CompanyExistsAsync(companyName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking company name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}