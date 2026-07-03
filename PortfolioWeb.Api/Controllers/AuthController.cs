using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Api.Security;
using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Application.Contract.Exceptions.Auth;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto registerUserDto, CancellationToken cancellationToken) =>
        Ok(await authService.Register(registerUserDto, cancellationToken));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserDto loginUserDto, CancellationToken cancellationToken) =>
        Ok(await authService.Login(loginUserDto, cancellationToken));

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        if (!User.TryGetEmail(out var email))
        {
            throw new InvalidCredentialsException();
        }

        return Ok(await authService.GetCurrentUser(email, cancellationToken));
    }
}

