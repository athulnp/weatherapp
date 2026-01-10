using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace WeatherApp.Models
{
    public class NominatimLocation
    {
        [JsonProperty("place_id")]
        public int PlaceId { get; set; }

        [JsonProperty("licence")]
        public string Licence { get; set; }


        [JsonProperty("lat")]
        public string Latitude { get; set; }

        [JsonProperty("lon")]
        public string Longitude { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("addresstype")]
        public string AddressType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
    }
}
