using Microsoft.Extensions.Logging;
using PortfolioWeb.Application.Contract.Dtos;
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
    public async Task<AuthorDto> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            logger.AuthorRetrievalRejectedBecauseIdIsEmpty();
            throw new InvalidAuthorIdException();
        }

        var author = await authorRepository.GetById(id, cancellationToken);

        if (author is not null)
        {
            return AuthorMapper.MapToDto(author);
        }
        
        logger.AuthorNotFoundDuringRetrieval(id);
        throw new AuthorNotFoundException(id);

    }

    public async Task<List<AuthorDto>> GetAll(CancellationToken cancellationToken = default)
    {
        var authors = await authorRepository.GetAll(cancellationToken);

        return authors
            .Select(AuthorMapper.MapToDto)
            .ToList();
    }

    public async Task<AuthorDto> Update(PersistAuthorDto authorDto, Guid currentAuthorId, CancellationToken cancellationToken = default)
    {
        if (currentAuthorId == Guid.Empty)
        {
            logger.AuthorUpdateRejectedBecauseIdIsEmpty();
            throw new InvalidAuthorIdException();
        }

        logger.UpdatingAuthor(currentAuthorId, authorDto.Name);

        var author = AuthorMapper.MapToEntity(authorDto);
        author.Id = currentAuthorId;

        var updatedAuthor = await authorRepository.Update(author, cancellationToken);

        if (updatedAuthor is null)
        {
            logger.AuthorNotFoundDuringUpdate(currentAuthorId);
            throw new AuthorNotFoundException(currentAuthorId);
        }

        logger.AuthorUpdatedSuccessfully(updatedAuthor.Id);

        return AuthorMapper.MapToDto(updatedAuthor);
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

