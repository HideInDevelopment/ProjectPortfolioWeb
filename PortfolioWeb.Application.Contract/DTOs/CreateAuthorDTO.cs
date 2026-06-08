namespace PortfolioWeb.Application.Contract.DTOs;

public class CreateAuthorDTO
{
    public string Name { get; set; } = string.Empty;

    public List<CreateProjectDTO> Projects { get; set; } = [];
}
