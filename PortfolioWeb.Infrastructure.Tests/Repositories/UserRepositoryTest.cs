using Microsoft.EntityFrameworkCore;
using PortfolioWeb.Domain.Entities;
using PortfolioWeb.Infrastructure.Repositories;
using PortfolioWeb.Infrastructure.Tests.Helpers;

namespace PortfolioWeb.Infrastructure.Tests.Repositories;

public class UserRepositoryTest
{
    [Test]
    public void GetByEmail_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        using var context = InMemoryDbContextFactory.Create();
        var repository = new UserRepository(context);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.That(
            async () => await repository.GetByEmail("manuel@portfolio.local", cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task GetByEmail_ShouldReturnUserWithAuthor_WhenUserExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new UserRepository(context);
        var user = CreateUserWithAuthor("manuel@portfolio.local", "Manuel");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await repository.GetByEmail(user.Email);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Email, Is.EqualTo(user.Email));
            Assert.That(result.Author, Is.Not.Null);
            Assert.That(result.Author.Name, Is.EqualTo("Manuel"));
        });
    }

    [Test]
    public async Task GetByEmail_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new UserRepository(context);

        var result = await repository.GetByEmail("missing@portfolio.local");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Create_ShouldPersistUserAndAuthorGraph_WhenUserIsValid()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repository = new UserRepository(context);
        var user = CreateUserWithAuthor("manuel@portfolio.local", "Manuel");

        var result = await repository.Create(user);
        var persistedUser = await context.Users.Include(x => x.Author).SingleAsync(x => x.Id == user.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(user));
            Assert.That(persistedUser.Email, Is.EqualTo("manuel@portfolio.local"));
            Assert.That(persistedUser.Author.Name, Is.EqualTo("Manuel"));
        });
    }

    [Test]
    public void Create_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        using var context = InMemoryDbContextFactory.Create();
        var repository = new UserRepository(context);
        var user = CreateUserWithAuthor("manuel@portfolio.local", "Manuel");
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        Assert.That(
            async () => await repository.Create(user, cancellationTokenSource.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    private static User CreateUserWithAuthor(string email, string authorName)
    {
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var user = new User(
            email,
            "hash",
            new DateTimeOffset(2026, 06, 24, 0, 0, 0, TimeSpan.Zero),
            UserRole.User,
            true)
        {
            Id = userId
        };

        var author = new Author(authorName)
        {
            Id = authorId,
            UserId = userId,
            User = user
        };

        user.Author = author;

        return user;
    }
}
