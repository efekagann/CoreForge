using CoreForge.Application.AuditLog.DTOs;
using CoreForge.Application.AuditLog.Queries.GetAuditLog;
using CoreForge.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreForge.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = Roles.SuperAdmin)]
public class AuditLogController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IList<AuditEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? entityName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAuditLogQuery(tenantId, entityName, page, pageSize),
            cancellationToken);

        return Ok(result);
    }
}
