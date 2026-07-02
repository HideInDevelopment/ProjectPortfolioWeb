using System.Text.Json.Serialization;
using PortfolioWeb.Application.Contract.Serialization;

namespace PortfolioWeb.Application.Contract.Dtos;

public class ProjectDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [JsonConverter(typeof(FlexibleDateTimeOffsetJsonConverter))]
    public DateTimeOffset ReleaseDate { get; set; }

    public int Version { get; set; }

    public Guid AuthorId { get; set; }

    public bool IsInDevelopment { get; set; }
}

