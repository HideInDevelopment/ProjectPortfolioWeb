using Microsoft.AspNetCore.Mvc;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Application.Contract.Services;

namespace PortfolioWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProjectDTO>>> GetAll(CancellationToken cancellationToken)
    {
        var projects = await projectService.GetAll(cancellationToken);

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDTO?>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await projectService.GetById(id, cancellationToken);

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDTO>> Create(CreateProjectDTO projectDto, CancellationToken cancellationToken)
    {
        var createdProject = await projectService.Create(projectDto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = createdProject.Id }, createdProject);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDTO>> Update(Guid id, CreateProjectDTO projectDto, CancellationToken cancellationToken)
    {
        var updatedProject = await projectService.Update(id, projectDto, cancellationToken);

        return Ok(updatedProject);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await projectService.Delete(id, cancellationToken);

        return NoContent();
    }
}
