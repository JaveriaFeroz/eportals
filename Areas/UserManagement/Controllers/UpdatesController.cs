using ProcureToPay.Areas.UserManagement.Models;
using ProcureToPay.Areas.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProcureToPay.Areas.UserManagement.Controllers
{
    [Area("UserManagement")]
    [Authorize(Roles = "Admin,UpdateManager")] // Adjust roles as needed
    public class UpdatesController : Controller
    {
        private readonly IUpdateService _updateService;

        public UpdatesController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        public async Task<IActionResult> Index()
        {
            var updates = await _updateService.GetAllUpdatesAsync();
            return View(updates);
        }

        public IActionResult Create()
        {
            return View(new Update());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Priority,IsActive,VisibleToRoles")] Update update)
        {
            if (ModelState.IsValid)
            {
                update.CreatedBy = User.Identity.Name;
                await _updateService.CreateUpdateAsync(update);
                return RedirectToAction(nameof(Index));
            }
            return View(update);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var update = await _updateService.GetAllUpdatesAsync();
            var updateToEdit = update.FirstOrDefault(u => u.Id == id);
            if (updateToEdit == null)
            {
                return NotFound();
            }
            return View(updateToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Update update)
        {
            if (ModelState.IsValid)
            {
                await _updateService.UpdateUpdateAsync(update);
                return RedirectToAction(nameof(Index));
            }
            return View(update);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _updateService.DeleteUpdateAsync(id);
            return Json(new { success = result });
        }
    }
}
