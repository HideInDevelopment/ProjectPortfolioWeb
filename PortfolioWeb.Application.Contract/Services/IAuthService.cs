using PortfolioWeb.Application.Contract.DTOs;

namespace PortfolioWeb.Application.Contract.Services;

public interface IAuthService
{
    Task<AuthResponseDTO> Register(RegisterUserDTO registerUserDto, CancellationToken cancellationToken = default);

    Task<AuthResponseDTO> Login(LoginUserDTO loginUserDto, CancellationToken cancellationToken = default);

    Task EnsureCurrentUserIsActive(string email, CancellationToken cancellationToken = default);
}
