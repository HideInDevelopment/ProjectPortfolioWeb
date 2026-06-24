using Microsoft.Extensions.Logging;

namespace PortfolioWeb.Application.Logging;

internal static partial class AuthServiceLogMessages
{
    [LoggerMessage(EventId = 3001, Level = LogLevel.Warning, Message = "Registration rejected because email is empty.")]
    public static partial void RegistrationRejectedBecauseEmailIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Warning, Message = "Registration rejected because password is empty.")]
    public static partial void RegistrationRejectedBecausePasswordIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Warning, Message = "Registration rejected because author name is empty.")]
    public static partial void RegistrationRejectedBecauseAuthorNameIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Information, Message = "Registering user. Email: {Email}")]
    public static partial void RegisteringUser(this ILogger logger, string email);

    [LoggerMessage(EventId = 3005, Level = LogLevel.Warning, Message = "Registration rejected because the email is already registered. Email: {Email}")]
    public static partial void RegistrationRejectedBecauseEmailAlreadyExists(this ILogger logger, string email);

    [LoggerMessage(EventId = 3006, Level = LogLevel.Information, Message = "User registered successfully. UserId: {UserId}, AuthorId: {AuthorId}")]
    public static partial void UserRegisteredSuccessfully(this ILogger logger, Guid userId, Guid authorId);

    [LoggerMessage(EventId = 3007, Level = LogLevel.Warning, Message = "Login rejected because email is empty.")]
    public static partial void LoginRejectedBecauseEmailIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 3008, Level = LogLevel.Warning, Message = "Login rejected because password is empty.")]
    public static partial void LoginRejectedBecausePasswordIsEmpty(this ILogger logger);

    [LoggerMessage(EventId = 3009, Level = LogLevel.Information, Message = "Logging in user. Email: {Email}")]
    public static partial void LoggingInUser(this ILogger logger, string email);

    [LoggerMessage(EventId = 3010, Level = LogLevel.Warning, Message = "Login rejected because the user was not found. Email: {Email}")]
    public static partial void LoginRejectedBecauseUserWasNotFound(this ILogger logger, string email);

    [LoggerMessage(EventId = 3011, Level = LogLevel.Warning, Message = "Login rejected because the user is inactive. UserId: {UserId}")]
    public static partial void LoginRejectedBecauseUserIsInactive(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 3012, Level = LogLevel.Warning, Message = "Login rejected because the password is invalid. UserId: {UserId}")]
    public static partial void LoginRejectedBecausePasswordIsInvalid(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 3013, Level = LogLevel.Information, Message = "User logged in successfully. UserId: {UserId}, AuthorId: {AuthorId}")]
    public static partial void UserLoggedInSuccessfully(this ILogger logger, Guid userId, Guid authorId);
}
