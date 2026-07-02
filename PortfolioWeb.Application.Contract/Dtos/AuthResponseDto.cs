namespace PortfolioWeb.Application.Contract.Dtos;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
}

