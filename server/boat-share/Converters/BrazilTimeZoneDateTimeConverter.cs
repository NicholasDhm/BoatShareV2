using System.Text.Json;
using System.Text.Json.Serialization;

namespace boat_share.Converters
{
    /// <summary>
    /// JSON converter that serializes and deserializes DateTime values in Brazil timezone (America/Sao_Paulo)
    /// This ensures all users see reservations in the boat's local time, regardless of their own timezone
    /// </summary>
    public class BrazilTimeZoneDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly TimeZoneInfo BrazilTimeZone = GetBrazilTimeZone();

        private static TimeZoneInfo GetBrazilTimeZone()
        {
            try
            {
                // Try IANA timezone ID (Linux/Mac)
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fall back to Windows timezone ID
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeString = reader.GetString();
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return DateTime.MinValue;
            }

            // Parse the incoming datetime
            if (DateTime.TryParse(dateTimeString, out var dateTime))
            {
                // If the datetime doesn't have timezone info, treat it as Brazil time and convert to UTC for storage
                if (dateTime.Kind == DateTimeKind.Unspecified)
                {
                    // Interpret as Brazil time, convert to UTC
                    return TimeZoneInfo.ConvertTimeToUtc(dateTime, BrazilTimeZone);
                }

                // If it's already UTC or has timezone info, just ensure it's UTC
                return dateTime.ToUniversalTime();
            }

            throw new JsonException($"Unable to parse datetime: {dateTimeString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Convert from UTC to Brazil time for display
            var brazilTime = value.Kind == DateTimeKind.Utc
                ? TimeZoneInfo.ConvertTimeFromUtc(value, BrazilTimeZone)
                : TimeZoneInfo.ConvertTime(value, BrazilTimeZone);

            // Write as ISO 8601 format without timezone indicator (treated as local time by frontend)
            writer.WriteStringValue(brazilTime.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }
}
