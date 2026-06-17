using PortfolioWeb.Application.Contract.Serialization;
using System.Globalization;
using System.Text.Json;

namespace PortfolioWeb.Application.Tests.Serialization;

public class FlexibleDateTimeOffsetJsonConverterTest
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new FlexibleDateTimeOffsetJsonConverter() }
    };

    [Test]
    public void Read_ShouldParseDateUsingSupportedExactFormat()
    {
        var result = JsonSerializer.Deserialize<DateTimeOffset>("\"01/07/2026\"", SerializerOptions);

        Assert.That(result, Is.EqualTo(new DateTimeOffset(2026, 07, 01, 0, 0, 0, TimeSpan.Zero)));
    }

    [Test]
    public void Read_ShouldParseDateUsingFallbackParsing()
    {
        var result = JsonSerializer.Deserialize<DateTimeOffset>(
            "\"Wed, 01 Jul 2026 14:30:00 GMT\"",
            SerializerOptions);

        Assert.That(result, Is.EqualTo(new DateTimeOffset(2026, 07, 01, 14, 30, 0, TimeSpan.Zero)));
    }

    [Test]
    public void Read_ShouldThrowJsonException_WhenDateIsNullOrWhitespace()
    {
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DateTimeOffset>("\"   \"", SerializerOptions));

        Assert.That(exception!.Message, Is.EqualTo("releaseDate cannot be null or empty."));
    }

    [Test]
    public void Read_ShouldThrowJsonException_WhenDateIsInvalid()
    {
        var exception = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DateTimeOffset>("\"not-a-date\"", SerializerOptions));

        Assert.That(exception!.Message, Is.EqualTo("releaseDate 'not-a-date' is not a valid date."));
    }

    [Test]
    public void Write_ShouldSerializeUsingRoundTripFormat()
    {
        var value = new DateTimeOffset(2026, 07, 01, 14, 30, 45, 123, TimeSpan.Zero)
            .AddTicks(4567);

        var result = JsonSerializer.Serialize(value, SerializerOptions);

        Assert.That(
            JsonSerializer.Deserialize<string>(result),
            Is.EqualTo(value.ToString("O", CultureInfo.InvariantCulture)));
    }
}
