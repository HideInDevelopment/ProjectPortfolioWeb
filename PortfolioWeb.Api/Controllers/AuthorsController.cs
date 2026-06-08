using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuthorDTO>>> GetAll(CancellationToken cancellationToken)
    {
        var authors = await authorService.GetAll(cancellationToken);

        return Ok(authors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuthorDTO?>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var author = await authorService.GetById(id, cancellationToken);

        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDTO>> Create(AuthorDTO authorDto, CancellationToken cancellationToken)
    {
        var createdAuthor = await authorService.Create(authorDto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = createdAuthor.Id }, createdAuthor);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AuthorDTO>> Update(Guid id, AuthorDTO authorDto, CancellationToken cancellationToken)
    {
        authorDto.Id = id;

        var updatedAuthor = await authorService.Update(authorDto, cancellationToken);

        return Ok(updatedAuthor);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await authorService.Delete(id, cancellationToken);

        return NoContent();
    }
}
