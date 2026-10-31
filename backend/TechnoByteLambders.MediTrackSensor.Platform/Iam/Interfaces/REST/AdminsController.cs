using Microsoft.AspNetCore.Mvc;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Transform;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST;

[ApiController]
[Route("api/v1/admins")]
[Tags("Admins")]
public class AdminsController(IAdminCommandService adminCommandService, IAdminQueryService adminQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var admins = await adminQueryService.Handle(new GetAllAdminsQuery(), ct);
        return Ok(admins.Select(AdminResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminResource resource, CancellationToken ct)
    {
        var result = await adminCommandService.Handle(
            new CreateAdminCommand(resource.EntityName, resource.EntityCode, resource.Schedule, resource.UserId), ct);
        if (result is Result<Admin, string>.Failure failure)
            return BadRequest(new { error = failure.Error });
        if (result is Result<Admin, string>.Success success)
            return Ok(AdminResourceFromEntityAssembler.ToResourceFromEntity(success.Value));
        return BadRequest(new { error = "Unknown error." });    }
}
