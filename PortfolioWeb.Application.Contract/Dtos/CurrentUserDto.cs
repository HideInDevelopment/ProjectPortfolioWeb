namespace PortfolioWeb.Application.Contract.Dtos;

public class CurrentUserDto
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public Guid AuthorId { get; init; }

    public string AuthorName { get; init; } = string.Empty;
}
