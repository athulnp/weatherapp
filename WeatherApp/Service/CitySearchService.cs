namespace WeatherApp.Service
{
    public class CitySearchService
    {
        private static readonly Dictionary<string, int> _searchCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();

        // Initialize with some default popular cities
        static CitySearchService()
        {
            _searchCounts["Mumbai"] = 100;
            _searchCounts["Delhi"] = 95;
            _searchCounts["Bangalore"] = 90;
            _searchCounts["Chennai"] = 85;
            _searchCounts["Kochi"] = 80;
            _searchCounts["Hyderabad"] = 75;
        }

        public void RecordSearch(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return;

            lock (_lock)
            {
                cityName = cityName.Trim();
                if (_searchCounts.ContainsKey(cityName))
                {
                    _searchCounts[cityName]++;
                }
                else
                {
                    _searchCounts[cityName] = 1;
                }
            }
        }

        public List<CitySearchResult> GetPopularCities(int topN = 6)
        {
            lock (_lock)
            {
                return _searchCounts
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(topN)
                    .Select(kvp => new CitySearchResult
                    {
                        CityName = kvp.Key,
                        SearchCount = kvp.Value
                    })
                    .ToList();
            }
        }

        public void ResetSearches()
        {
            lock (_lock)
            {
                _searchCounts.Clear();
            }
        }
    }

    public class CitySearchResult
    {
        public string CityName { get; set; } = string.Empty;
        public int SearchCount { get; set; }
    }
}
