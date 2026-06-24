using Microsoft.Extensions.Logging;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
using PortfolioWeb.Application.Contract.Exceptions.Author;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Application.Logging;
using PortfolioWeb.Application.Mappers;
using PortfolioWeb.Core.Contracts.Repositories;

namespace PortfolioWeb.Application.Services;

public class AuthorService(
    IAuthorRepository authorRepository,
    ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<AuthorDTO> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.AuthorRetrievalRejectedBecauseIdIsEmpty();
            throw new InvalidAuthorIdException();
        }

        var author = await authorRepository.GetById(id, cancellationToken);

        if (author is null)
        {
            logger.AuthorNotFoundDuringRetrieval(id);
            throw new AuthorNotFoundException(id);
        }

        return AuthorMapper.MapToDTO(author);
    }

    public async Task<List<AuthorDTO>> GetAll(CancellationToken cancellationToken = default)
    {
        var authors = await authorRepository.GetAll(cancellationToken);

        return authors
            .Select(AuthorMapper.MapToDTO)
            .ToList();
    }

    public async Task<AuthorDTO> Update(Guid id, PersistAuthorDTO authorDto, Guid currentAuthorId, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.AuthorUpdateRejectedBecauseIdIsEmpty();
            throw new InvalidAuthorIdException();
        }

        if (currentAuthorId != id)
        {
            logger.AuthorUpdateRejectedBecauseResourceIsForbidden(currentAuthorId, id);
            throw new ForbiddenResourceAccessException();
        }

        logger.UpdatingAuthor(id, authorDto.Name);

        var author = AuthorMapper.MapToEntity(authorDto);
        author.Id = id;

        var updatedAuthor = await authorRepository.Update(author, cancellationToken);

        if (updatedAuthor is null)
        {
            logger.AuthorNotFoundDuringUpdate(id);
            throw new AuthorNotFoundException(id);
        }

        logger.AuthorUpdatedSuccessfully(updatedAuthor.Id);

        return AuthorMapper.MapToDTO(updatedAuthor);
    }

    public async Task Delete(Guid id, Guid currentAuthorId, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.AuthorDeletionRejectedBecauseIdIsEmpty();
            throw new InvalidAuthorIdException();
        }

        if (currentAuthorId != id)
        {
            logger.AuthorDeletionRejectedBecauseResourceIsForbidden(currentAuthorId, id);
            throw new ForbiddenResourceAccessException();
        }

        logger.DeletingAuthor(id);

        var author = await authorRepository.GetById(id, cancellationToken);

        if (author is null)
        {
            logger.AuthorNotFoundDuringDeletion(id);
            throw new AuthorNotFoundException(id);
        }

        await authorRepository.Delete(author, cancellationToken);

        logger.AuthorDeletedSuccessfully(id);
    }
}
