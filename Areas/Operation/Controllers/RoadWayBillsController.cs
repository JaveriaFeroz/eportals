using Microsoft.AspNetCore.Mvc;

namespace ProcureToPay.Areas.Operation.Controllers
{
    public class RoadWayBillsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
