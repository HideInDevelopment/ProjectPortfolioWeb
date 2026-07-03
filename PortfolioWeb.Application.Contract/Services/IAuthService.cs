using PortfolioWeb.Application.Contract.Dtos;

namespace PortfolioWeb.Application.Contract.Services;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterUserDto registerUserDto, CancellationToken cancellationToken = default);

    Task<AuthResponseDto> Login(LoginUserDto loginUserDto, CancellationToken cancellationToken = default);

    Task<CurrentUserDto> GetCurrentUser(string email, CancellationToken cancellationToken = default);

    Task EnsureCurrentUserIsActive(string email, CancellationToken cancellationToken = default);
}

