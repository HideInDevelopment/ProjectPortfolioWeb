using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;
using PortfolioWeb.Api.Security;

namespace PortfolioWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProjectDTO>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await projectService.GetAll(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDTO>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await projectService.GetById(id, cancellationToken));

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProjectDTO>> Create([FromBody] CreateProjectDTO projectDto, CancellationToken cancellationToken)
    {
        if (!User.TryGetAuthorId(out var currentAuthorId))
        {
            return Unauthorized();
        }

        var createdProject = await projectService.Create(projectDto, currentAuthorId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = createdProject.Id }, createdProject);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDTO>> Update(Guid id, [FromBody] UpdateProjectDTO projectDto, CancellationToken cancellationToken)
    {
        if (!User.TryGetAuthorId(out var currentAuthorId))
        {
            return Unauthorized();
        }

        var updatedProject = await projectService.Update(id, projectDto, currentAuthorId, cancellationToken);

        return Ok(updatedProject);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!User.TryGetAuthorId(out var currentAuthorId))
        {
            return Unauthorized();
        }

        await projectService.Delete(id, currentAuthorId, cancellationToken);

        return NoContent();
    }
}
