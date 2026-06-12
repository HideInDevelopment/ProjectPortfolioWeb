using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Contract.Repositories;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Persistence;

namespace PortfolioWeb.Infrastructure.Repositories;

public class AuthorRepository(PortfolioWebDbContext dbContext) : IAuthorRepository
{
    public async Task<Author?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Authors
            .Include(author => author.Projects)
            .FirstOrDefaultAsync(author => author.Id == id, cancellationToken);
    }

    public async Task<List<Author>> GetAll(CancellationToken cancellationToken = default)
    {
        return await dbContext.Authors
            .Include(author => author.Projects)
            .ToListAsync(cancellationToken);
    }

    public async Task<Author> Create(Author author, CancellationToken cancellationToken = default)
    {
        await dbContext.Authors.AddAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return author;
    }

    public async Task<Author?> Update(Author author, CancellationToken cancellationToken = default)
    {
        var existingAuthor = await dbContext.Authors
            .Include(x => x.Projects)
            .FirstOrDefaultAsync(x => x.Id == author.Id, cancellationToken);

        if (existingAuthor is null)
        {
            return null;
        }

        dbContext.Entry(existingAuthor).CurrentValues.SetValues(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        return existingAuthor;
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await dbContext.Authors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (author is null)
        {
            return false;
        }

        dbContext.Authors.Remove(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
