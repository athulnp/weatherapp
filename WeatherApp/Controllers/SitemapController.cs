using Microsoft.AspNetCore.Mvc;
using WeatherApp.Data;

namespace WeatherApp.Controllers
{
    public class SitemapController : Controller
    {
        private readonly AppDbContext _context;

        public SitemapController(AppDbContext context)
        {
            _context = context;
        }

        [Route("sitemap.xml")]
        public IActionResult Index()
        {
            var articles = _context.Articles
                .OrderByDescending(a => a.PublishDate)
                .ToList();

            Response.ContentType = "application/xml";
            return View("Sitemap", articles);
        }
    }
}
