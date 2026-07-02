using System.ComponentModel.DataAnnotations;

namespace PortfolioWeb.Application.Contract.Dtos;

public class LoginUserDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(320, MinimumLength = 3, ErrorMessage = "Email must be between 3 and 320 characters.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(200, ErrorMessage = "Password must not exceed 200 characters.")]
    public string Password { get; init; } = string.Empty;
}

