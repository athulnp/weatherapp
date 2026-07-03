namespace WeatherApp.Data
{
    /// <summary>
    /// Curated list of high-search-volume world cities used to generate
    /// indexable /weather/{slug} landing pages in the sitemap.
    /// Slugs are lowercase-hyphenated to match the canonical URL form
    /// produced by <see cref="Controllers.CitiesController"/>.
    /// </summary>
    public static class PopularCities
    {
        public static readonly string[] Slugs = new[]
        {
            // ---- India ----
            "mumbai", "delhi", "bangalore", "chennai", "hyderabad", "kolkata",
            "kochi", "pune", "ahmedabad", "jaipur", "lucknow", "chandigarh",
            "surat", "kanpur", "nagpur", "indore", "bhopal", "patna",
            "vadodara", "coimbatore", "visakhapatnam", "thiruvananthapuram",
            "guwahati", "bhubaneswar", "dehradun", "shimla", "srinagar", "goa",

            // ---- Asia (excl. India) ----
            "tokyo", "osaka", "kyoto", "beijing", "shanghai", "guangzhou",
            "shenzhen", "hong-kong", "seoul", "busan", "singapore", "bangkok",
            "phuket", "kuala-lumpur", "jakarta", "bali", "manila", "hanoi",
            "ho-chi-minh-city", "colombo", "kathmandu", "dhaka", "karachi",
            "lahore", "islamabad", "dubai", "abu-dhabi", "doha", "riyadh",
            "jeddah", "kuwait-city", "tehran", "baghdad", "istanbul", "ankara",
            "tel-aviv", "jerusalem", "taipei", "almaty", "tashkent",

            // ---- Europe ----
            "london", "manchester", "birmingham", "edinburgh", "dublin",
            "paris", "marseille", "nice", "madrid", "barcelona", "lisbon",
            "porto", "rome", "milan", "venice", "florence", "naples",
            "berlin", "munich", "frankfurt", "hamburg", "amsterdam", "brussels",
            "vienna", "zurich", "geneva", "prague", "budapest", "warsaw",
            "krakow", "athens", "stockholm", "oslo", "copenhagen", "helsinki",
            "moscow", "saint-petersburg", "kyiv", "bucharest", "belgrade",
            "reykjavik",

            // ---- North America ----
            "new-york", "los-angeles", "chicago", "houston", "phoenix",
            "philadelphia", "san-antonio", "san-diego", "dallas", "san-jose",
            "austin", "san-francisco", "seattle", "denver", "boston",
            "washington", "las-vegas", "miami", "atlanta", "orlando",
            "toronto", "vancouver", "montreal", "calgary", "ottawa",
            "mexico-city", "guadalajara", "cancun", "monterrey",

            // ---- South America ----
            "sao-paulo", "rio-de-janeiro", "brasilia", "buenos-aires",
            "santiago", "lima", "bogota", "medellin", "quito", "caracas",
            "montevideo", "la-paz",

            // ---- Africa ----
            "cairo", "alexandria", "lagos", "abuja", "nairobi", "accra",
            "johannesburg", "cape-town", "durban", "casablanca", "marrakech",
            "tunis", "addis-ababa", "dar-es-salaam", "kampala", "algiers",

            // ---- Oceania ----
            "sydney", "melbourne", "brisbane", "perth", "adelaide",
            "auckland", "wellington", "gold-coast"
        };
    }
}
