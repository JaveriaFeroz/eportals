using Microsoft.AspNetCore.Mvc;

namespace ProcureToPay.Areas.Operation.Controllers
{
    public class RwbExpensesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
