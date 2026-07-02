namespace PortfolioWeb.Application.Contract.Dtos;

public class AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; init; }
}

