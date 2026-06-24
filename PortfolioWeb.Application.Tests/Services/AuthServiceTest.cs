using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
using PortfolioWeb.Application.Services;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Tests.Services;

public class AuthServiceTest
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private AuthService _authService = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.SetupGet(x => x["Authentication:Issuer"]).Returns("PortfolioWeb");
        _configurationMock.SetupGet(x => x["Authentication:Audience"]).Returns("PortfolioWebClient");
        _configurationMock.SetupGet(x => x["Authentication:SigningKey"]).Returns("ThisIsATestSigningKeyWithEnoughLength123!");
        _configurationMock.SetupGet(x => x["Authentication:ExpirationMinutes"]).Returns("60");

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);
    }

    [Test]
    public void Register_ShouldThrowInvalidAuthRequestException_WhenEmailIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidAuthRequestException>(async () =>
            await _authService.Register(new RegisterUserDTO
            {
                Email = " ",
                Password = "password",
                AuthorName = "Manuel"
            }));

        Assert.That(exception!.Message, Is.EqualTo("Email is required."));
        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Register_ShouldThrowInvalidAuthRequestException_WhenPasswordIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidAuthRequestException>(async () =>
            await _authService.Register(new RegisterUserDTO
            {
                Email = "manuel@portfolio.local",
                Password = " ",
                AuthorName = "Manuel"
            }));

        Assert.That(exception!.Message, Is.EqualTo("Password is required."));
        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Register_ShouldThrowInvalidAuthRequestException_WhenAuthorNameIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidAuthRequestException>(async () =>
            await _authService.Register(new RegisterUserDTO
            {
                Email = "manuel@portfolio.local",
                Password = "password",
                AuthorName = " "
            }));

        Assert.That(exception!.Message, Is.EqualTo("Author name is required."));
        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Register_ShouldThrowDuplicateEmailException_WhenEmailAlreadyExists()
    {
        const string email = "manuel@portfolio.local";

        _userRepositoryMock
            .Setup(x => x.GetByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUserWithAuthor(Guid.NewGuid(), Guid.NewGuid(), email, "Manuel", true));

        var exception = Assert.ThrowsAsync<DuplicateEmailException>(async () =>
            await _authService.Register(new RegisterUserDTO
            {
                Email = email,
                Password = "password",
                AuthorName = "Manuel"
            }));

        Assert.That(exception!.Message, Is.EqualTo($"The email '{email}' is already registered."));
        _userRepositoryMock.Verify(x => x.Create(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Register_ShouldCreateUserAuthorGraphAndReturnJwt_WhenRequestIsValid()
    {
        User? createdUserArgument = null;
        var persistedUser = CreateUserWithAuthor(Guid.NewGuid(), Guid.NewGuid(), "manuel@portfolio.local", "Manuel", true);

        _userRepositoryMock
            .Setup(x => x.GetByEmail("manuel@portfolio.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.Create(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUserArgument = user)
            .ReturnsAsync(persistedUser);

        var result = await _authService.Register(new RegisterUserDTO
        {
            Email = "  Manuel@Portfolio.Local  ",
            Password = "password",
            AuthorName = "  Manuel  "
        });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.That(createdUserArgument, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(createdUserArgument!.Email, Is.EqualTo("manuel@portfolio.local"));
            Assert.That(createdUserArgument.PasswordHash, Is.Not.EqualTo("password"));
            Assert.That(createdUserArgument.Author.Name, Is.EqualTo("Manuel"));
            Assert.That(createdUserArgument.Author.User, Is.SameAs(createdUserArgument));
            Assert.That(result.AccessToken, Is.Not.Empty);
            Assert.That(result.ExpiresAt, Is.GreaterThan(DateTimeOffset.UtcNow));
            Assert.That(token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(persistedUser.Id.ToString()));
            Assert.That(token.Claims.Single(claim => claim.Type == "authorId").Value, Is.EqualTo(persistedUser.Author.Id.ToString()));
        });
    }

    [Test]
    public void Login_ShouldThrowInvalidAuthRequestException_WhenEmailIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidAuthRequestException>(async () =>
            await _authService.Login(new LoginUserDTO
            {
                Email = " ",
                Password = "password"
            }));

        Assert.That(exception!.Message, Is.EqualTo("Email is required."));
        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Login_ShouldThrowInvalidAuthRequestException_WhenPasswordIsEmpty()
    {
        var exception = Assert.ThrowsAsync<InvalidAuthRequestException>(async () =>
            await _authService.Login(new LoginUserDTO
            {
                Email = "manuel@portfolio.local",
                Password = " "
            }));

        Assert.That(exception!.Message, Is.EqualTo("Password is required."));
        _userRepositoryMock.Verify(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Login_ShouldThrowInvalidCredentialsException_WhenUserDoesNotExist()
    {
        _userRepositoryMock
            .Setup(x => x.GetByEmail("manuel@portfolio.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var exception = Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.Login(new LoginUserDTO
            {
                Email = "manuel@portfolio.local",
                Password = "password"
            }));

        Assert.That(exception!.Message, Is.EqualTo("The provided credentials are not valid."));
    }

    [Test]
    public void Login_ShouldThrowInactiveUserException_WhenUserIsInactive()
    {
        var user = CreateUserWithAuthor(Guid.NewGuid(), Guid.NewGuid(), "manuel@portfolio.local", "Manuel", false);

        _userRepositoryMock
            .Setup(x => x.GetByEmail("manuel@portfolio.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var exception = Assert.ThrowsAsync<InactiveUserException>(async () =>
            await _authService.Login(new LoginUserDTO
            {
                Email = "manuel@portfolio.local",
                Password = "password"
            }));

        Assert.That(exception!.Message, Is.EqualTo("The user account is inactive."));
    }

    [Test]
    public void Login_ShouldThrowInvalidCredentialsException_WhenPasswordIsInvalid()
    {
        var user = CreateUserWithAuthor(Guid.NewGuid(), Guid.NewGuid(), "manuel@portfolio.local", "Manuel", true);

        _userRepositoryMock
            .Setup(x => x.GetByEmail("manuel@portfolio.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var exception = Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.Login(new LoginUserDTO
            {
                Email = "manuel@portfolio.local",
                Password = "wrong-password"
            }));

        Assert.That(exception!.Message, Is.EqualTo("The provided credentials are not valid."));
    }

    [Test]
    public async Task Login_ShouldReturnJwt_WhenCredentialsAreValid()
    {
        var user = CreateUserWithAuthor(Guid.NewGuid(), Guid.NewGuid(), "manuel@portfolio.local", "Manuel", true);

        _userRepositoryMock
            .Setup(x => x.GetByEmail("manuel@portfolio.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _authService.Login(new LoginUserDTO
        {
            Email = "  Manuel@Portfolio.Local ",
            Password = "password"
        });

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.AccessToken, Is.Not.Empty);
            Assert.That(result.ExpiresAt, Is.GreaterThan(DateTimeOffset.UtcNow));
            Assert.That(token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(user.Id.ToString()));
            Assert.That(token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value, Is.EqualTo("manuel@portfolio.local"));
            Assert.That(token.Claims.Single(claim => claim.Type == "authorId").Value, Is.EqualTo(user.Author.Id.ToString()));
        });
    }

    private static User CreateUserWithAuthor(Guid userId, Guid authorId, string email, string authorName, bool isActive)
    {
        var user = new User(
            email,
            HashForTests("password"),
            new DateTimeOffset(2026, 06, 24, 0, 0, 0, TimeSpan.Zero),
            UserRole.User,
            isActive)
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

    private static string HashForTests(string password)
    {
        var salt = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
        var hash = Convert.ToBase64String(System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            password,
            Convert.FromBase64String(salt),
            100_000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32));

        return $"100000.{salt}.{hash}";
    }
}
