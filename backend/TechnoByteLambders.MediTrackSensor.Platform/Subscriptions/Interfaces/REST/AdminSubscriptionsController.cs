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
[Route("api/v1/admins/{adminId:int}/subscriptions")]
[Tags("Subscriptions")]
public class AdminSubscriptionsController(
    ISubscriptionCommandService commandService,
    ISubscriptionQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByAdmin(int adminId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllSubscriptionsQuery(), ct);
        return Ok(items
            .Where(s => s.AdminId.Value == adminId)
            .Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int adminId,
        [FromBody] CreateNestedSubscriptionResource resource,
        CancellationToken ct)
    {
        if (!Enum.TryParse<SubscriptionPlan>(resource.Plan, true, out var plan))
            return BadRequest(new { error = "Invalid Plan value." });

        var result = await commandService.Handle(
            new CreateSubscriptionCommand(plan, resource.StartDate, resource.EndDate, adminId), ct);
        if (result.IsFailure) return BadRequest(new { error = ((dynamic)result).Error });
        return Created(
            $"/api/v1/admins/{adminId}/subscriptions/{((dynamic)result).Value.Id}",
            SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(((dynamic)result).Value));
    }

    [HttpDelete("{subscriptionId:int}")]
    public async Task<IActionResult> Delete(int adminId, int subscriptionId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllSubscriptionsQuery(), ct);
        if (items.All(s => s.Id != subscriptionId || s.AdminId.Value != adminId))
            return NotFound();

        var result = await commandService.DeleteAsync(subscriptionId, ct);
        if (result.IsFailure) return NotFound(new { error = ((dynamic)result).Error });
        return NoContent();
    }
}
