using Microsoft.AspNetCore.Mvc;
using Moq;
using PortfolioWeb.Api.Controllers;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Tests.Controllers;

public class AuthControllerTest
{
    [Test]
    public async Task Register_ShouldReturnOkWithAuthResponse()
    {
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var registerUserDto = new RegisterUserDTO
        {
            Email = "manuel@portfolio.local",
            Password = "password",
            AuthorName = "Manuel"
        };
        var authResponse = new AuthResponseDTO
        {
            AccessToken = "token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        authService
            .Setup(service => service.Register(registerUserDto, cancellationToken))
            .ReturnsAsync(authResponse);

        var controller = new AuthController(authService.Object);

        var result = await controller.Register(registerUserDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(authResponse));
        authService.Verify(service => service.Register(registerUserDto, cancellationToken), Times.Once);
    }

    [Test]
    public async Task Login_ShouldReturnOkWithAuthResponse()
    {
        var authService = new Mock<IAuthService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var loginUserDto = new LoginUserDTO
        {
            Email = "manuel@portfolio.local",
            Password = "password"
        };
        var authResponse = new AuthResponseDTO
        {
            AccessToken = "token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        authService
            .Setup(service => service.Login(loginUserDto, cancellationToken))
            .ReturnsAsync(authResponse);

        var controller = new AuthController(authService.Object);

        var result = await controller.Login(loginUserDto, cancellationToken);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(authResponse));
        authService.Verify(service => service.Login(loginUserDto, cancellationToken), Times.Once);
    }
}
