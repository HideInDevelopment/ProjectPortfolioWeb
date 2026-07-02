using System.Text.Json.Serialization;
using PortfolioWeb.Application.Contract.Serialization;

namespace PortfolioWeb.Application.Contract.Dtos;

public class ProjectDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleDateTimeOffsetJsonConverter))]
    public DateTimeOffset ReleaseDate { get; init; }

    public int Version { get; init; }

    public Guid AuthorId { get; init; }

    public bool IsInDevelopment { get; init; }
}

