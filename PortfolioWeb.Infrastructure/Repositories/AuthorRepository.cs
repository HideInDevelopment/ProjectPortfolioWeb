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

    public async Task<Author> Update(Author author, CancellationToken cancellationToken = default)
    {
        dbContext.Authors.Update(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        return author;
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await dbContext.Authors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (author is null)
        {
            return;
        }

        dbContext.Authors.Remove(author);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
