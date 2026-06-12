using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Domain.Contract.Repositories;

namespace PortfolioWeb.Application.Services;

public class AuthorService(IAuthorRepository authorRepository) : IAuthorService
{
    public async Task<AuthorDTO?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await authorRepository.GetById(id, cancellationToken);

        return author is null ? null : AuthorMapper.MapToDTO(author);
    }

    public async Task<List<AuthorDTO>> GetAll(CancellationToken cancellationToken = default)
    {
        var authors = await authorRepository.GetAll(cancellationToken);

        return authors
            .Select(AuthorMapper.MapToDTO)
            .ToList();
    }

    public async Task<AuthorDTO> Create(CreateAuthorDTO authorDto, CancellationToken cancellationToken = default)
    {
        var author = AuthorMapper.MapToEntity(authorDto);
        var createdAuthor = await authorRepository.Create(author, cancellationToken);

        return AuthorMapper.MapToDTO(createdAuthor);
    }

    public async Task<AuthorDTO?> Update(Guid id, CreateAuthorDTO authorDto, CancellationToken cancellationToken = default)
    {
        var author = AuthorMapper.MapToEntity(authorDto);
        author.Id = id;

        var updatedAuthor = await authorRepository.Update(author, cancellationToken);

        return updatedAuthor is null ? null : AuthorMapper.MapToDTO(updatedAuthor);
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        return await authorRepository.Delete(id, cancellationToken);
    }
}
