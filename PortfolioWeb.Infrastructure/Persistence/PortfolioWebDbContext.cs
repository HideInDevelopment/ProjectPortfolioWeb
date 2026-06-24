using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Infrastructure.Persistence;

public class PortfolioWebDbContext(DbContextOptions<PortfolioWebDbContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioWebDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
