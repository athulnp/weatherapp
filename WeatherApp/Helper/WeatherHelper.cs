using System.Globalization;
using System.Text.RegularExpressions;

namespace WeatherApp.Helper
{
    public static class WeatherHelper
    {

        public static bool IsPostalCode(string value)
        {
            return Regex.IsMatch(value, @"^\d{4,10}$");
        }
        public static bool TryGetLongitude(string value, out double longitude)
        {
            return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out longitude)
                && longitude >= -180
                && longitude <= 180;
        }

        public static bool TryGetLatitude(string value, out double latitude)
        {
            return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out latitude)
                && latitude >= -90
                && latitude <= 90;
        }

        public static bool IsLatLongPair(string value, out double lat, out double lon)
        {
            lat = lon = 0;
            var parts = value.Split(',');

            if (parts.Length != 2) return false;

            var isLatlongPair = double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon)
                && lat >= -90 && lat <= 90
                && lon >= -180 && lon <= 180;

            return isLatlongPair;
        }
    }
}
