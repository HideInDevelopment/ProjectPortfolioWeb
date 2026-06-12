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
        try
        {
            var authors = await authorService.GetAll(cancellationToken);

            return Ok(authors);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuthorDTO?>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var author = await authorService.GetById(id, cancellationToken);

            if (author is null)
            {
                return NotFound();
            }

            return Ok(author);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDTO>> Create([FromBody] CreateAuthorDTO authorDto, CancellationToken cancellationToken)
    {
        try
        {
            var createdAuthor = await authorService.Create(authorDto, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = createdAuthor.Id }, createdAuthor);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AuthorDTO>> Update(Guid id, [FromBody] CreateAuthorDTO authorDto, CancellationToken cancellationToken)
    {
        try
        {
            var updatedAuthor = await authorService.Update(id, authorDto, cancellationToken);

            if (updatedAuthor is null)
            {
                return NotFound();
            }

            return Ok(updatedAuthor);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var wasDeleted = await authorService.Delete(id, cancellationToken);

            if (!wasDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
