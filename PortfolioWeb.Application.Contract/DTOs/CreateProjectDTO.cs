using System.Text.Json.Serialization;
using PortfolioWeb.Application.Contract.Serialization;

namespace PortfolioWeb.Application.Contract.DTOs;

public class CreateProjectDTO
{
    // TODO: Add DataAnnotations validation for Title and Description so invalid lengths are rejected before reaching EF Core.
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [JsonConverter(typeof(FlexibleDateTimeOffsetJsonConverter))]
    public DateTimeOffset ReleaseDate { get; set; }

    public int Version { get; set; }

    public Guid AuthorId { get; set; }

    public bool IsInDevelopment { get; set; }
}
