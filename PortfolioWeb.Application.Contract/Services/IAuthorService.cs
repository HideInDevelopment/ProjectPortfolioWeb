using PortfolioWeb.Application.Contract.DTOs;

namespace PortfolioWeb.Application.Contract.Services;

public interface IAuthorService
{
    Task<AuthorDTO> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<AuthorDTO>> GetAll(CancellationToken cancellationToken = default);

    Task<AuthorDTO> Update(Guid id, PersistAuthorDTO authorDto, Guid currentAuthorId, CancellationToken cancellationToken = default);

    Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default);
}
