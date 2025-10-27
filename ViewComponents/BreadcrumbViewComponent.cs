using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcureToPay.ViewComponents
{
    public class BreadcrumbItem
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }

    public class BreadcrumbViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var path = HttpContext.Request.Path.Value;
            var segments = path.Split('/', System.StringSplitOptions.RemoveEmptyEntries);

            var breadcrumbs = new List<BreadcrumbItem>();
            breadcrumbs.Add(new BreadcrumbItem { Text = "Home", Url = "/", IsActive = segments.Length == 0 });

            var currentPath = "";
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                currentPath += $"/{segment}";

                var text = segment.Replace('-', ' '); // Replace hyphens for cleaner display
                text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());

                var breadcrumb = new BreadcrumbItem
                {
                    Text = text,
                    Url = currentPath,
                    IsActive = i == segments.Length - 1
                };

                breadcrumbs.Add(breadcrumb);
            }

            return View(breadcrumbs);
        }
    }
}
