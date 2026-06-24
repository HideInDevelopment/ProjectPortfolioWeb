using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterUserDTO registerUserDto, CancellationToken cancellationToken) =>
        Ok(await authService.Register(registerUserDto, cancellationToken));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginUserDTO loginUserDto, CancellationToken cancellationToken) =>
        Ok(await authService.Login(loginUserDto, cancellationToken));
}
