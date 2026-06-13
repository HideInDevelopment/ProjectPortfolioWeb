using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Application.Mappers;
using PortfolioWeb.Domain.Contract.Repositories;

namespace PortfolioWeb.Application.Services;

public class AuthorService(IAuthorRepository authorRepository) : IAuthorService
{
    public async Task<AuthorDTO> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidAuthorIdException();
        }

        var author = await authorRepository.GetById(id, cancellationToken);

        return author is null 
            ? throw new AuthorNotFoundException(id) 
            : AuthorMapper.MapToDTO(author);
    }

    public async Task<List<AuthorDTO>> GetAll(CancellationToken cancellationToken = default)
    {
        var authors = await authorRepository.GetAll(cancellationToken);

        return authors
            .Select(AuthorMapper.MapToDTO)
            .ToList();
    }

    public async Task<AuthorDTO> Create(PersistAuthorDTO authorDto, CancellationToken cancellationToken = default)
    {
        var author = AuthorMapper.MapToEntity(authorDto);
        var createdAuthor = await authorRepository.Create(author, cancellationToken);

        return AuthorMapper.MapToDTO(createdAuthor);
    }

    public async Task<AuthorDTO> Update(Guid id, PersistAuthorDTO authorDto, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidAuthorIdException();
        }

        var author = AuthorMapper.MapToEntity(authorDto);
        author.Id = id;

        var updatedAuthor = await authorRepository.Update(author, cancellationToken);

        return updatedAuthor is null 
            ? throw new AuthorNotFoundException(id) 
            : AuthorMapper.MapToDTO(updatedAuthor);
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidAuthorIdException();
        }

        var author = await authorRepository.GetById(id, cancellationToken);

        if (author is null)
        {
            throw new AuthorNotFoundException(id);
        }

        var wasDeleted = await authorRepository.Delete(author, cancellationToken);

        return wasDeleted 
            ? true 
            : throw new Exception("An unexpected error occurred while deleting the author.");
    }
}
