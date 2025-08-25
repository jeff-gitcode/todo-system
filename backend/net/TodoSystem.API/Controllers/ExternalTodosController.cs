using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;

namespace TodoSystem.API.Controllers;

[ApiController]
[Route("api/v1/externaltodos")]
[Authorize]
public class ExternalTodosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExternalTodosController> _logger;

    public ExternalTodosController(IMediator mediator, ILogger<ExternalTodosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // GET /api/v1/externaltodos
    [HttpGet]
    [OutputCache(Duration = 30)]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetAll(CancellationToken ct)
    {
        _logger.LogInformation("TodosController.GetAll called at {Time}", DateTime.UtcNow);
        var result = await _mediator.Send(new TodoSystem.Application.Todos.Queries.GetExternalTodosQuery(), ct);
        return Ok(result);
    }

    // GET /api/v1/externaltodos/{id}
    // [IgnoreAntiforgeryToken]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new TodoSystem.Application.Todos.Queries.GetExternalTodoByIdQuery(id), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    // POST /api/v1/externaltodos
    [HttpPost]
    public async Task<ActionResult<TodoDto>> Create([FromBody] TodoSystem.Application.Todos.Commands.CreateExternalTodoCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        // We don't have a stable external int id from JSONPlaceholder; return collection URI like the minimal API did
        return Created("/api/v1/externaltodos", result);
    }

    // PUT /api/v1/externaltodos/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TodoDto>> Update(int id, [FromBody] TodoSystem.Application.Todos.Commands.UpdateExternalTodoCommand command, CancellationToken ct)
    {
        if (id != command.ExternalId)
            return BadRequest(new { message = "ID in URL and body do not match." });

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    // DELETE /api/v1/externaltodos/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.Send(new TodoSystem.Application.Todos.Commands.DeleteExternalTodoCommand { ExternalId = id }, ct);
        return deleted ? NoContent() : NotFound();
    }
}