using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Core.Contracts.Repositories;

public interface IAuthorRepository
{
    Task<Author?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<Author>> GetAll(CancellationToken cancellationToken = default);

    Task<Author> Create(Author author, CancellationToken cancellationToken = default);

    Task<Author?> Update(Author author, CancellationToken cancellationToken = default);

    Task Delete(Author author, CancellationToken cancellationToken = default);
}
