using PortfolioWeb.Application.Contract.Dtos;

namespace PortfolioWeb.Application.Contract.Services;

public interface IAuthorService
{
    Task<AuthorDto> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<List<AuthorDto>> GetAll(CancellationToken cancellationToken = default);

    Task<AuthorDto> Update(PersistAuthorDto authorDto, Guid currentAuthorId, CancellationToken cancellationToken = default);

    Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default);
}

