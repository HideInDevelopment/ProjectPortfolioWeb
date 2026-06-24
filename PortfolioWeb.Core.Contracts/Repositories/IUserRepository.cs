using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Core.Contracts.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);

    Task<User> Create(User user, CancellationToken cancellationToken = default);
}
