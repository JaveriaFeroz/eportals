using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProcureToPay.Models;

namespace ProcureToPay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Add debugging information
            _logger.LogInformation("=== HOME INDEX ACCESSED ===");
            _logger.LogInformation($"User authenticated: {User.Identity.IsAuthenticated}");
            _logger.LogInformation($"User name: {User.Identity.Name ?? "NULL"}");
            _logger.LogInformation($"User claims count: {User.Claims.Count()}");
            _logger.LogInformation($"Request path: {Request.Path}");
            _logger.LogInformation($"Request method: {Request.Method}");

            // Log all cookies
            foreach (var cookie in Request.Cookies)
            {
                _logger.LogInformation($"Cookie: {cookie.Key} = {cookie.Value}");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}