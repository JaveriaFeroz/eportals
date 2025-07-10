using Microsoft.AspNetCore.Mvc;

namespace ProcureToPay.Areas.Operation.Controllers
{
    public class JobsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
