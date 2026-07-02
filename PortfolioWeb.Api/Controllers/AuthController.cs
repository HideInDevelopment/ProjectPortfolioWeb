using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterUserDto registerUserDto, CancellationToken cancellationToken) =>
        Ok(await authService.Register(registerUserDto, cancellationToken));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginUserDto loginUserDto, CancellationToken cancellationToken) =>
        Ok(await authService.Login(loginUserDto, cancellationToken));
}

