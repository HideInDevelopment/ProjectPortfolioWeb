using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PortfolioWeb.Application.Contract.Serialization;

namespace PortfolioWeb.Application.Contract.DTOs;

public class CreateProjectDTO
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [JsonConverter(typeof(FlexibleDateTimeOffsetJsonConverter))]
    public DateTimeOffset ReleaseDate { get; set; }

    public int Version { get; set; }

    public bool IsInDevelopment { get; set; }
}
