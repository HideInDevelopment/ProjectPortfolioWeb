namespace PortfolioWeb.Application.Contract.DTOs;

public class AuthResponseDTO
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
}
