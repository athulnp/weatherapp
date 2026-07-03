using Microsoft.AspNetCore.Mvc;
using WeatherApp.IService;
using WeatherApp.Service;
using WeatherApp.Data;

namespace WeatherApp.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleService _service;
        private readonly AppDbContext _context;

        public ArticleController(IArticleService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        // GET: /Article or /blog
        [Route("Article")]
        [Route("blog")]
        [Route("{culture:regex(^(hi|ta|ml)$)}/Article")]
        [Route("{culture:regex(^(hi|ta|ml)$)}/blog")]
        public IActionResult Index()
        {
            var articles = _context.Articles.OrderByDescending(a => a.PublishDate).ToList();
            return View(articles);
        }

        // GET: /Article/{slug}
        [HttpGet]
        [Route("Article/{slug}")]
        [Route("{culture:regex(^(hi|ta|ml)$)}/Article/{slug}")]
        public IActionResult Details(string slug)
        {
            var article = _context.Articles.FirstOrDefault(a => a.Slug == slug);
            if (article == null)
                return NotFound();

            return View(article);
        }

        // Legacy route: /article/by-date/{date}
        [HttpGet]
        [Route("article/by-date/{date}")]
        public IActionResult DetailsByDate(string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return NotFound();

            var article = _service.GetArticleByDate(parsedDate);
            if (article == null)
                return NotFound();

            return View("Details", article);
        }
    }
}
