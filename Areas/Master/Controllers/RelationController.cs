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
    public class RelationController : Controller
    {
        private readonly IRelationService _relationService;
        private readonly ILogger<RelationController> _logger;

        public RelationController(
            IRelationService relationService,
            ILogger<RelationController> logger)
        {
            _relationService = relationService;
            _logger = logger;
        }

        // GET: Master/Relation
        public async Task<IActionResult> Index(bool showInactive = false, string searchTerm = null)
        {
            try
            {
                var relations = await _relationService.GetRelationsAsync(showInactive, searchTerm);

                var viewModel = new RelationIndexViewModel
                {
                    Relations = relations.ToList(),
                    ShowInactiveOnly = showInactive,
                    SearchTerm = searchTerm
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading relations");
                TempData["ErrorMessage"] = "An error occurred while loading the relations.";
                return View(new RelationIndexViewModel());
            }
        }

        // GET: Master/Relation/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var relation = await _relationService.GetRelationByIdAsync(id);
                if (relation == null)
                {
                    return NotFound();
                }

                return View(relation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading relation details for ID {RelationId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the relation details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Master/Relation/Create
        public IActionResult Create()
        {
            var model = new RelationViewModel();
            return View(model);
        }

        // POST: Master/Relation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RelationViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if relation name already exists
                    if (await _relationService.RelationExistsAsync(model.RelationName))
                    {
                        ModelState.AddModelError("RelationName", "A relation with this name already exists.");
                        return View(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _relationService.SaveRelationAsync(model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Relation created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create relation.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating relation");
                ModelState.AddModelError("", "An error occurred while creating the relation.");
                return View(model);
            }
        }

        // GET: Master/Relation/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var relation = await _relationService.GetRelationByIdAsync(id);
                if (relation == null)
                {
                    return NotFound();
                }

                return View(relation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading relation for edit with ID {RelationId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the relation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Relation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, RelationViewModel model)
        {
            if (id != model.RelationId) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    if (await _relationService.RelationExistsAsync(model.RelationName, id))
                    {
                        ModelState.AddModelError("RelationName", "A relation with this name already exists.");
                        return PartialView(model);
                    }

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
                    var success = await _relationService.SaveRelationAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Relation updated successfully.";
                        return Json(new { success = true, message = "Relation updated successfully." });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update relation.");
                    }
                }


                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating relation with ID {RelationId}", id);
                ModelState.AddModelError("", "An error occurred while updating the relation.");
                return PartialView(model);
            }
        }

        // GET: Master/Relation/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var relation = await _relationService.GetRelationByIdAsync(id);
                if (relation == null)
                {
                    return NotFound();
                }

                return View(relation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading relation for delete with ID {RelationId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the relation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Relation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _relationService.DeleteRelationAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Relation deleted successfully.";
                    return Json(new { success = true, message = "Relation deleted successfully." });
                }
                else
                {
                    // If service returned false (e.g., region not found, or other non-exception reason)
                    return BadRequest(new { success = false, message = "Failed to delete relation." });
                }
            }
            catch (InvalidOperationException ex) // Specific exception for business logic (e.g., associated cities)
            {
                _logger.LogWarning(ex, "Cannot delete relation with ID {RelationId}", id);
                return BadRequest(new { success = false, message = "Cannot delete this relation." });
            }
            catch (Exception ex) // General unexpected errors
            {
                _logger.LogError(ex, "Error deleting relation with ID {RelationId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "An unexpected error occurred while deleting the relation." });
            }
        }

        // AJAX endpoint for checking if relation name exists
        [HttpGet]
        public async Task<JsonResult> CheckRelationNameExists(string relationName, short? excludeId = null)
        {
            try
            {
                var exists = await _relationService.RelationExistsAsync(relationName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking relation name existence");
                return Json(new { exists = false, error = true });
            }
        }
    }
}