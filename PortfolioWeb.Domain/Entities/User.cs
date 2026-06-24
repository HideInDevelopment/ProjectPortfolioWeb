namespace PortfolioWeb.Domain.Entities;

public class User
{
    // Required by EF Core when materializing the entity from persistence.
    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Author = null!;
    }

    public User(
        string email,
        string passwordHash,
        DateTimeOffset createdDate,
        UserRole role,
        bool isActive)
    {
        Email = email;
        PasswordHash = passwordHash;
        CreatedDate = createdDate;
        Role = role;
        IsActive = isActive;
        Author = null!;
    }

    public Guid Id { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public Author Author { get; set; }
}
