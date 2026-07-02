using System.ComponentModel.DataAnnotations;

namespace PortfolioWeb.Application.Contract.Dtos;

public class PersistAuthorDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;
}

