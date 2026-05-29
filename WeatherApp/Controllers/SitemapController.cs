using Microsoft.AspNetCore.Mvc;

namespace WeatherApp.Controllers
{
    public class SitemapController : Controller
    {
        [Route("sitemap.xml")]
        public IActionResult Index()
        {
            Response.ContentType = "application/xml";
            return View("Sitemap");
        }
    }
}
