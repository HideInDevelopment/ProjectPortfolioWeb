using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PortfolioWeb.Application.Contract.Serialization;

namespace PortfolioWeb.Application.Contract.Dtos;

public class CreateProjectDto : IValidatableObject
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 2000 characters.")]
    public string Description { get; init; } = string.Empty;

    [JsonConverter(typeof(FlexibleDateTimeOffsetJsonConverter))]
    public DateTimeOffset ReleaseDate { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Version must be zero or greater.")]
    public int Version { get; init; }

    public bool IsInDevelopment { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ReleaseDate == default)
        {
            yield return new ValidationResult(
                "Release date is required.",
                [nameof(ReleaseDate)]);
        }
    }
}

