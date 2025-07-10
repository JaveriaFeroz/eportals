using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Master.ViewModels; // Ensure you have a using for ViewModels
using System.Security.Claims;

namespace ProcureToPay.Areas.Master.Controllers
{
    [Area("Master")]
    [Authorize]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }


        private async Task LoadDropdowns(ClientCreateEditViewModel viewModel)
        {
            var lookups = await _clientService.GetLookupsAsync();


            viewModel.Cities = lookups.Cities.Select(c => new SelectListItem
            {
                Text = c.CityName,
                Value = c.CityId.ToString()
            }).ToList();

            viewModel.Companies = lookups.Companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.CompanyId.ToString()
            }).ToList();

            viewModel.PaymentModes = lookups.PaymentModes.Select(c => new SelectListItem
            {
                Text = c.PaymentModeName,
                Value = c.PaymentModeId.ToString()
            }).ToList();

            viewModel.IndustryVerticals = lookups.IndustryVerticals.Select(c => new SelectListItem
            {
                Text = c.IndustryName,
                Value = c.IndustryVerticalId.ToString()
            }).ToList();

            viewModel.RateTypes = lookups.RateTypes.Select(c => new SelectListItem
            {
                Text = c.RateTypeName,
                Value = c.RateTypeId.ToString()
            }).ToList();
            // Ab yeh 'viewModel.Client' ke andar 'InvoiceFormats' ko set kar raha hai
            viewModel.Client.InvoiceFormats = lookups.InvoiceFormats
                .Select(f => new ClientInvoiceFormatViewModel
                {
                    FormatId = f.FormatId,
                    FormatName = f.FormatName,
                    IsSelected = false
                })
                .ToList();
        }

        // 1. INDEX
        // GET: Master/Client
        public async Task<IActionResult> Index(bool showInactiveOnly = false, string? searchTerm = null)
        {
            try
            {
                var clients = await _clientService.GetClientsAsync(showInactiveOnly, searchTerm);
                var viewModel = new ClientIndexViewModel
                {
                    Clients = clients.ToList(),
                    ShowInactiveOnly = showInactiveOnly,
                    SearchTerm = searchTerm
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clients index");
                TempData["ErrorMessage"] = "Error loading clients. Please try again.";
                return View(new ClientIndexViewModel());
            }
        }

        // 2. CREATE
        // GET: Master/Client/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ClientCreateEditViewModel();

            await LoadDropdowns(viewModel);

            return View(viewModel);
        }

        // POST: Master/Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCreateEditViewModel model) // FIX: Parameter is now the container ViewModel
        {
            try
            {
                // Since the parameter is 'model', we access the client data via 'model.Client'
                if (ModelState.IsValid)
                {
                    if (await _clientService.ClientNameExistsAsync(model.Client.ClientName))
                    {
                        ModelState.AddModelError("Client.ClientName", "Client name already exists.");
                    }
                    else
                    {
                        // Set audit properties on the correct object
                        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        model.Client.CreatedBy = currentUserId;
                        model.Client.UpdatedBy = currentUserId;
                        model.Client.CreatedOn = DateTime.UtcNow;
                        model.Client.UpdatedOn = DateTime.UtcNow;

                        // Pass the correct object (model.Client) to the service
                        var success = await _clientService.SaveClientAsync(model.Client);

                        if (success)
                        {
                            TempData["SuccessMessage"] = "Client created successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create client. Please try again.");
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
                _logger.LogError(ex, "Error creating client");
                TempData["ErrorMessage"] = "Error creating client. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 3. EDIT
        // GET: Master/Client/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new ClientCreateEditViewModel { Client = client };
            await LoadDropdowns(viewModel);

            return View(viewModel); // Assuming Edit uses a full view like Create.
        }


        // POST: Master/Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, ClientCreateEditViewModel viewModel)
        {
            if (id != viewModel.Client.ClientId)
            {
                return NotFound();
            }

            // Perform any custom validation before checking ModelState
            if (await _clientService.ClientNameExistsAsync(viewModel.Client.ClientName, id))
            {
                ModelState.AddModelError("Client.ClientName", "This name is used by another client.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the audit properties for the update
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    viewModel.Client.UpdatedBy = currentUserId;
                    viewModel.Client.UpdatedOn = DateTime.UtcNow;

                    var success = await _clientService.SaveClientAsync(viewModel.Client);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Client updated successfully.";
                        return Json(new { success = true, message = "Client updated successfully." });
                    }

                    ModelState.AddModelError("", "A database error occurred and the client could not be saved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating client with ID {id}", id);
                    ModelState.AddModelError("", "An unexpected error occurred while updating the client.");
                }
            }

            // If we reach here, validation failed. Reload dropdowns and return the Partial View with errors.
            await LoadDropdowns(viewModel);
            return PartialView(viewModel);
        }

        // 4. DETAILS
        // GET: Master/Client/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null) return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client details for ID {ClientId}", id);
                TempData["ErrorMessage"] = "Error loading client details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 5. DELETE
        // GET: Master/Client/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var client = await _clientService.GetClientByIdAsync(id);
                if (client == null) return NotFound();
                return View(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete client page for ID {ClientId}", id);
                TempData["ErrorMessage"] = "Error loading delete page. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Master/Client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var success = await _clientService.DeleteClientAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Client deleted successfully.";
                    return Json(new { success = true, message = "Client deleted successfully." });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete client.";
                    return Json(new { success = false, message = "Failed to delete client. It may have already been removed." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client with ID {ClientId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the client.";
                return RedirectToAction(nameof(Index));
            }
        }

        // --- API & AJAX Endpoints ---

        [HttpGet]
        public async Task<IActionResult> GetClientsForRWB()
        {
            try
            {
                var clients = await _clientService.GetClientsForRWBAsync();
                return Json(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clients for RWB");
                return Json(new { error = "Error retrieving clients" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckClientNameExists(string clientName, short? excludeId = null)
        {
            try
            {
                var exists = await _clientService.ClientNameExistsAsync(clientName, excludeId);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking client name existence");
                return Json(new { error = "Error checking client name" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _clientService.GetLookupsAsync();
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