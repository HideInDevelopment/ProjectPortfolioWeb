using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Application.Logging;
using PortfolioWeb.Application.Security;
using PortfolioWeb.Core.Contracts.Repositories;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    ILogger<AuthService> logger,
    IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDTO> Register(RegisterUserDTO registerUserDto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeRequiredEmail(registerUserDto.Email, logger, true);
        var password = NormalizeRequiredPassword(registerUserDto.Password, logger, true);
        var authorName = NormalizeRequiredAuthorName(registerUserDto.AuthorName, logger);

        logger.RegisteringUser(normalizedEmail);

        var existingUser = await userRepository.GetByEmail(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            logger.RegistrationRejectedBecauseEmailAlreadyExists(normalizedEmail);
            throw new DuplicateEmailException(normalizedEmail);
        }

        var user = new User(
            normalizedEmail,
            PasswordHashing.HashPassword(password),
            DateTimeOffset.UtcNow,
            UserRole.User,
            true);
        var author = new Author(authorName)
        {
            User = user
        };

        user.Author = author;

        var createdUser = await userRepository.Create(user, cancellationToken);

        logger.UserRegisteredSuccessfully(createdUser.Id, createdUser.Author.Id);

        return JwtTokenFactory.Create(createdUser, configuration);
    }

    public async Task<AuthResponseDTO> Login(LoginUserDTO loginUserDto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeRequiredEmail(loginUserDto.Email, logger, false);
        var password = NormalizeRequiredPassword(loginUserDto.Password, logger, false);

        logger.LoggingInUser(normalizedEmail);

        var user = await userRepository.GetByEmail(normalizedEmail, cancellationToken);

        if (user is null)
        {
            logger.LoginRejectedBecauseUserWasNotFound(normalizedEmail);
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            logger.LoginRejectedBecauseUserIsInactive(user.Id);
            throw new InactiveUserException();
        }

        if (!PasswordHashing.VerifyPassword(user.PasswordHash, password))
        {
            logger.LoginRejectedBecausePasswordIsInvalid(user.Id);
            throw new InvalidCredentialsException();
        }

        logger.UserLoggedInSuccessfully(user.Id, user.Author.Id);

        return JwtTokenFactory.Create(user, configuration);
    }

    private static string NormalizeRequiredEmail(string email, ILogger logger, bool isRegistration)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            if (isRegistration)
            {
                logger.RegistrationRejectedBecauseEmailIsEmpty();
            }
            else
            {
                logger.LoginRejectedBecauseEmailIsEmpty();
            }

            throw new InvalidAuthRequestException("Email is required.");
        }

        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeRequiredPassword(string password, ILogger logger, bool isRegistration)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            if (isRegistration)
            {
                logger.RegistrationRejectedBecausePasswordIsEmpty();
            }
            else
            {
                logger.LoginRejectedBecausePasswordIsEmpty();
            }

            throw new InvalidAuthRequestException("Password is required.");
        }

        return password;
    }

    private static string NormalizeRequiredAuthorName(string authorName, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(authorName))
        {
            logger.RegistrationRejectedBecauseAuthorNameIsEmpty();
            throw new InvalidAuthRequestException("Author name is required.");
        }

        return authorName.Trim();
    }
}
