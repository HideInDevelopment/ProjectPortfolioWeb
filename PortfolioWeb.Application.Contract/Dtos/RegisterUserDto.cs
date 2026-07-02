using System.ComponentModel.DataAnnotations;

namespace PortfolioWeb.Application.Contract.Dtos;

public class RegisterUserDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(320, MinimumLength = 3, ErrorMessage = "Email must be between 3 and 320 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(200, ErrorMessage = "Password must not exceed 200 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Author name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Author name must be between 1 and 200 characters.")]
    public string AuthorName { get; set; } = string.Empty;
}

