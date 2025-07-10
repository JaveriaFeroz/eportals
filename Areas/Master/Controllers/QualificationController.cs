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
    public class QualificationController : Controller
    {
        private readonly IQualificationService _qualificationService;
        private readonly ILogger<QualificationController> _logger;

        public QualificationController(
            IQualificationService qualificationService,
            ILogger<QualificationController> logger)
        {
            _qualificationService = qualificationService;
            _logger = logger;
        }

        // GET: Master/Qualification
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var qualifications = await _qualificationService.GetQualificationsAsync(showInactive, searchTerm);

                var viewModel = new QualificationIndexViewModel
                {
                    Qualifications = qualifications.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading qualifications");
                TempData["ErrorMessage"] = "An error occurred while loading the qualifications.";
                return View(new QualificationIndexViewModel());
            }
        }

        // GET: Master/Qualification/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var qualification = await _qualificationService.GetQualificationByIdAsync(id);
                if (qualification == null)
                {
                    return NotFound();
                }

                return View(qualification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading qualification details for ID {QualificationId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the qualification details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Qualification/Create
        public IActionResult Create()
        {
            var model = new QualificationViewModel();
            return View(model);
        }

        // POST: Master/Qualification/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QualificationViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if qualification name already exists
                    if (await _qualificationService.QualificationExistsAsync(model.QualificationName))
                    {
                        ModelState.AddModelError("QualificationName", "A qualification with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _qualificationService.SaveQualificationAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Qualification created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create qualification.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating qualification");
                ModelState.AddModelError("", "An error occurred while creating the qualification.");
                return View(model);
            }
        }

        // GET: Master/Qualification/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var qualification = await _qualificationService.GetQualificationByIdAsync(id);
                if (qualification == null)
                {
                    return NotFound();
                }

                return View(qualification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading qualification for edit with ID {QualificationId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the qualification.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Qualification/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, QualificationViewModel model)
        {
            if (id != model.QualificationId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _qualificationService.QualificationExistsAsync(model.QualificationName, id))
                    {
                        ModelState.AddModelError("QualificationName", "A qualification with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _qualificationService.SaveQualificationAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Qualification updated successfully.";
                        return Json(new { success = true, message = "Qualification updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update qualification.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating qualification with ID {QualificationId}", id);
                ModelState.AddModelError("", "An error occurred while updating the qualification.");
                return PartialView(model);
            }
        }

        // GET: Master/Qualification/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var qualification = await _qualificationService.GetQualificationByIdAsync(id);
                if (qualification == null)
                {
                    return NotFound();
                }

                return View(qualification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading qualification for delete with ID {QualificationId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the qualification.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Qualification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _qualificationService.DeleteQualificationAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Qualification deleted successfully.";
                    return Json(new { success = true, message = "Qualification deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete qualification." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete qualification with ID {QualificationId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this qualification." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting qualification with ID {QualificationId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the qualification." });
            }
        }

        // AJAX endpoint for checking if qualification name exists
        [HttpGet]
        public async Task<JsonResult> CheckQualificationNameExists(string qualificationName, short? excludeId = null)
        {
            try
            {
                var exists = await _qualificationService.QualificationExistsAsync(qualificationName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking qualification name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}