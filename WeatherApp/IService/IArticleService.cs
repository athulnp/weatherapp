using WeatherApp.Models;

namespace WeatherApp.IService
{
    public interface IArticleService
    {
        Article GetArticleByDate(DateTime date);
    }
}
