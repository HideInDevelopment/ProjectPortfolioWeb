using System.ComponentModel.DataAnnotations;

namespace PortfolioWeb.Application.Contract.DTOs;

public class RegisterUserDTO
{
    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;
}
