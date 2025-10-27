using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels; // Ensure you have a using for ViewModels
using ProcureToPay.Helpers;
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class ConsigneeController : Controller
    {
        private readonly IConsigneeService _consigneeService;
        private readonly ILogger<ConsigneeController> _logger;

        public ConsigneeController(IConsigneeService consigneeService, ILogger<ConsigneeController> logger)
        {
            _consigneeService = consigneeService;
            _logger = logger;
        }


        private async Task LoadDropdowns(ConsigneeCreateEditViewModel viewModel)
        {
            var lookups = await _consigneeService.GetLookupsAsync();


            viewModel.Cities = lookups.Cities.Select(c => new SelectListItem
            {
                Text = c.CityName,
                Value = c.CityId.ToString()
            }).ToList();

            viewModel.Clients = lookups.Clients.Select(c => new SelectListItem
            {
                Text = c.ClientName,
                Value = c.ClientId.ToString()
            }).ToList();

            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();

           
           
        }

        // 1. INDEX
        // GET: Master/Consignee
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var consignees = await _consigneeService.GetConsigneesAsync(showInactiveOnly, searchTerm);
                var viewModel = new ConsigneeIndexViewModel
                {
                    Consignees = consignees.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consignees index");
                TempData["ErrorMessage"] = "Error loading consignees. Please try again.";
                return View(new ConsigneeIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Master/Consignee/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ConsigneeCreateEditViewModel();

            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        // POST: Master/Consignee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsigneeCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the consignee data via 'model.Consignee'
                if (ModelState.IsValid)
                {
                    if (await _consigneeService.ConsigneeNameExistsAsync(model.Consignee.ConsigneeName))
                    {
                        ModelState.AddModelError("Consignee.ConsigneeName", "Consignee name already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.Consignee.CreatedBy = currentUserId;
                        model.Consignee.UpdatedBy = currentUserId;
                        model.Consignee.CreatedOn = DateTimeHelper.GetPakistanStandardTime();
                        model.Consignee.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                        // Pass the correct object (model.Consignee) to the service
                        var success = await _consigneeService.SaveConsigneeAsync(model.Consignee);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "Consignee created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create consignee. Please try again.");
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
                _logger.LogError(ex, "Error creating consignee");
                TempData["ErrorMessage"] = "Error creating consignee. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Master/Consignee/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var consignee = await _consigneeService.GetConsigneeByIdAsync(id);
            if (consignee == null)
            {
                return NotFound();
            }

            var viewModel = new ConsigneeCreateEditViewModel { Consignee = consignee };
            await LoadDropdowns(viewModel);

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Master/Consignee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ConsigneeCreateEditViewModel viewModel)
        {
            if (id != viewModel.Consignee.ConsigneeId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _consigneeService.ConsigneeNameExistsAsync(viewModel.Consignee.ConsigneeName, id))
            {
                ModelState.AddModelError("Consignee.ConsigneeName", "This name is used by another consignee.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.Consignee.UpdatedBy = currentUserId;
                    viewModel.Consignee.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                    var success = await _consigneeService.SaveConsigneeAsync(viewModel.Consignee);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Consignee updated successfully.";
                        return Json(new { success = true, message = "Consignee updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the consignee could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating consignee with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the consignee.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Master/Consignee/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var consignee = await _consigneeService.GetConsigneeByIdAsync(id);
                if (consignee == null) return NotFound();
                return View(consignee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consignee details for ID {ConsigneeId}", id);
                TempData["ErrorMessage"] = "Error loading consignee details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Master/Consignee/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var consignee = await _consigneeService.GetConsigneeByIdAsync(id);
                if (consignee == null) return NotFound();
                return View(consignee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete consignee page for ID {ConsigneeId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Consignee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _consigneeService.DeleteConsigneeAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Consignee deleted successfully.";
                    return Json(new { success = true, message = "Consignee deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete consignee.";
                    return Json(new { success = false, message = "Failed to delete consignee. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting consignee with ID {ConsigneeId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the consignee.";
                return RedirectToAction(nameof(Index));
            }
        }


        
        [HttpPost]
        public async Task<IActionResult> CheckConsigneeNameExists(string consigneeName, short? excludeId = null)
        {
            try
            {
                var exists = await _consigneeService.ConsigneeNameExistsAsync(consigneeName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking consignee name existence");
                return Json(new { error = "Error checking consignee name" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _consigneeService.GetLookupsAsync();
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