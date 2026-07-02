using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.Dtos;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Api.Security;

namespace PortfolioWeb.Api.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class AuthorsController(
    IAuthorService authorService,
    IAuthService authService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuthorDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await authorService.GetAll(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuthorDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await authorService.GetById(id, cancellationToken));

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<AuthorDto>> Update([FromBody] PersistAuthorDto authorDto, CancellationToken cancellationToken)
    {
        if (!User.TryGetEmail(out var email))
        {
            return Unauthorized();
        }

        await authService.EnsureCurrentUserIsActive(email, cancellationToken);

        if (!User.TryGetAuthorId(out var currentAuthorId))
        {
            return Unauthorized();
        }

        var updatedAuthor = await authorService.Update(authorDto, currentAuthorId, cancellationToken);

        return Ok(updatedAuthor);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!User.TryGetEmail(out var email))
        {
            return Unauthorized();
        }

        await authService.EnsureCurrentUserIsActive(email, cancellationToken);

        if (!User.TryGetAuthorId(out var currentAuthorId))
        {
            return Unauthorized();
        }

        await authorService.Delete(id, currentAuthorId, cancellationToken);

        return NoContent();
    }
}

