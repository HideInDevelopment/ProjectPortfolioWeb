using System.ComponentModel.DataAnnotations;

namespace PortfolioWeb.Application.Contract.DTOs;

public class UpdateProjectDTO
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsInDevelopment { get; set; }
}
