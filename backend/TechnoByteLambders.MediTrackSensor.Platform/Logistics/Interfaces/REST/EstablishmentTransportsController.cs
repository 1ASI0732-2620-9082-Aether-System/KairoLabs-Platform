using Microsoft.AspNetCore.Mvc;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST.Transform;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Interfaces.REST;

[ApiController]
[Route("api/v1/establishments/{establishmentId:int}/transports")]
[Tags("Transports")]
public class EstablishmentTransportsController(
    ITransportCommandService commandService,
    ITransportQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByEstablishment(int establishmentId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllTransportsQuery(), ct);
        return Ok(items
            .Where(t => t.EstablishmentId.Value == establishmentId)
            .Select(TransportResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int establishmentId,
        [FromBody] CreateNestedTransportResource resource,
        CancellationToken ct)
    {
        if (!Enum.TryParse<TypeOfMedication>(resource.TypeOfMedication, true, out var medication))
            return BadRequest(new { error = "Invalid TypeOfMedication value." });

        var result = await commandService.Handle(
            new CreateTransportCommand(resource.TypeOfTransport, medication, establishmentId, resource.EnabledSensors ?? ""),
            ct);

        if (result is Result<Transport, string>.Failure failure)
            return BadRequest(new { error = failure.Error });
        if (result is Result<Transport, string>.Success success)
            return Created(
                $"/api/v1/establishments/{establishmentId}/transports/{success.Value.Id}",
                TransportResourceFromEntityAssembler.ToResourceFromEntity(success.Value));
        return BadRequest(new { error = "Unknown error." });
    }

    [HttpPut("{transportId:int}/sensor-data")]
    public async Task<IActionResult> UpdateSensorData(
        int establishmentId,
        int transportId,
        [FromBody] UpdateTransportSensorDataResource resource,
        CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllTransportsQuery(), ct);
        if (items.All(t => t.Id != transportId || t.EstablishmentId.Value != establishmentId))
            return NotFound();

        if (!Enum.TryParse<DoorStatus>(resource.DoorStatus, true, out var doorStatus))
            return BadRequest(new { error = "Invalid DoorStatus value." });

        var reading = new SensorReading(resource.Temperature, resource.Humidity, resource.LightIntensity,
            resource.AirQuality, resource.Vibration, resource.AtmosphericPressure, resource.SuspendedParticles);

        var result = await commandService.Handle(new UpdateTransportSensorDataCommand(transportId, reading, doorStatus), ct);
        if (result is Result<Transport, string>.Failure failure)
        {
            if (failure.Error == LogisticsErrors.TransportNotFound.Description)
                return NotFound(new { error = failure.Error });
            return BadRequest(new { error = failure.Error });
        }
        if (result is Result<Transport, string>.Success success)
            return Ok(TransportResourceFromEntityAssembler.ToResourceFromEntity(success.Value));
        return BadRequest(new { error = "Unknown error." });
    }

    [HttpDelete("{transportId:int}")]
    public async Task<IActionResult> Delete(int establishmentId, int transportId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllTransportsQuery(), ct);
        if (items.All(t => t.Id != transportId || t.EstablishmentId.Value != establishmentId))
            return NotFound();

        var result = await commandService.DeleteAsync(transportId, ct);
        if (result.IsFailure) return NotFound(new { error = ((dynamic)result).Error });
        return NoContent();
    }
}
