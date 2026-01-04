using WeatherApp.Data;
using WeatherApp.IService;
using WeatherApp.Models;

namespace WeatherApp.Service
{
    public class ArticleService : IArticleService
    {
        private readonly AppDbContext _context;

        public ArticleService(AppDbContext context)
        {
            _context = context;
        }

        public Article GetArticleByDate(DateTime date)
        {
            return _context.Articles.FirstOrDefault(a => a.PublishDate.Date == date.Date);
        }
    }
}
