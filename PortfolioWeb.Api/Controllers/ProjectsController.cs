using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Exceptions.Project;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProjectDTO>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var projects = await projectService.GetAll(cancellationToken);

            return Ok(projects);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDTO>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var project = await projectService.GetById(id, cancellationToken);

            return Ok(project);
        }
        catch (InvalidProjectIdException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (ProjectNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDTO>> Create([FromBody] CreateProjectDTO projectDto, CancellationToken cancellationToken)
    {
        try
        {
            var createdProject = await projectService.Create(projectDto, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = createdProject.Id }, createdProject);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDTO>> Update(Guid id, [FromBody] UpdateProjectDTO projectDto, CancellationToken cancellationToken)
    {
        try
        {
            var updatedProject = await projectService.Update(id, projectDto, cancellationToken);

            return Ok(updatedProject);
        }
        catch (InvalidProjectIdException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (ProjectNotFoundException exception)
        {
            return NotFound(exception.Message);
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
            await projectService.Delete(id, cancellationToken);

            return NoContent();
        }
        catch (InvalidProjectIdException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (ProjectNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
