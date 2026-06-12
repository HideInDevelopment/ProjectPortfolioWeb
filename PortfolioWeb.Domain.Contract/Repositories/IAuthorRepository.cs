using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Domain.Contract.Repositories;

public interface IAuthorRepository
{
    Task<Author?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<Author>> GetAll(CancellationToken cancellationToken = default);

    Task<Author> Create(Author author, CancellationToken cancellationToken = default);

    Task<Author?> Update(Author author, CancellationToken cancellationToken = default);

    Task<bool> Delete(Guid id, CancellationToken cancellationToken = default);
}
