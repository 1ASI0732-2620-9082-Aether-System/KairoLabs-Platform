using Microsoft.AspNetCore.Mvc;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST.Transform;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Interfaces.REST;

[ApiController]
[Route("api/v1/subscriptions")]
[Tags("Subscriptions")]
public class SubscriptionsController(
    ISubscriptionCommandService commandService,
    ISubscriptionQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllSubscriptionsQuery(), ct);
        return Ok(items.Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionResource resource, CancellationToken ct)
    {
        if (!Enum.TryParse<SubscriptionPlan>(resource.Plan, true, out var plan))
            return BadRequest(new { error = "Invalid Plan value." });

        var result = await commandService.Handle(
            new CreateSubscriptionCommand(plan, resource.StartDate, resource.EndDate, resource.AdminId), ct);
        if (result.IsFailure) return BadRequest(new { error = ((dynamic)result).Error });
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(((dynamic)result).Value));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandService.DeleteAsync(id, ct);
        if (result.IsFailure) return NotFound(new { error = ((dynamic)result).Error });
        return NoContent();
    }
}
