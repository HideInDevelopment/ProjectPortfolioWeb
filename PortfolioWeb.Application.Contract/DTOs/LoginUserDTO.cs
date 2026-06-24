using System.ComponentModel.DataAnnotations;

namespace PortfolioWeb.Application.Contract.DTOs;

public class LoginUserDTO
{
    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
