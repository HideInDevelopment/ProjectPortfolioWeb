namespace PortfolioWeb.Application.Contract.DTOs;

public class UpdateProjectDTO
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsInDevelopment { get; set; }
}
