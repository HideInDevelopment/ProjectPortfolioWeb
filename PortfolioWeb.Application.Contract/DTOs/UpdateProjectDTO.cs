namespace PortfolioWeb.Application.Contract.DTOs;

public class UpdateProjectDTO
{
    // TODO: Add DataAnnotations validation for Title and Description so invalid lengths are rejected before reaching EF Core.
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsInDevelopment { get; set; }
}
