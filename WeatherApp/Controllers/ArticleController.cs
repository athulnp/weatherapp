using Microsoft.AspNetCore.Mvc;
using WeatherApp.IService;
using WeatherApp.Service;

namespace WeatherApp.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleService _service;

        public ArticleController(IArticleService service)
        {
            _service = service;
        }

        [Route("article/{date}")]
        public IActionResult Details(string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return NotFound();

            var article = _service.GetArticleByDate(parsedDate);
            if (article == null)
                return NotFound();

            return View(article);
        }
    }
}
