using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortfolioWeb.Application.Contract.Serialization;

public class FlexibleDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    private static readonly string[] SupportedFormats =
    [
        "dd/MM/yyyy",
        "d/M/yyyy",
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"
    ];

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("releaseDate cannot be null or empty.");
        }

        if (DateTimeOffset.TryParseExact(
                value,
                SupportedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var exactDate))
        {
            return exactDate;
        }

        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var parsedDate))
        {
            return parsedDate;
        }

        throw new JsonException($"releaseDate '{value}' is not a valid date.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
    }
}
