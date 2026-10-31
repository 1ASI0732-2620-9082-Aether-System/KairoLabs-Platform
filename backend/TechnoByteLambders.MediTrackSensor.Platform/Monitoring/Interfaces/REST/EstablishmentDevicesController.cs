using Microsoft.AspNetCore.Mvc;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST.Transform;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using ProblemDetailsFactory = TechnoByteLambders.MediTrackSensor.Platform.Shared.Interfaces.REST.ProblemDetails.ProblemDetailsFactory;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Interfaces.REST;

[ApiController]
[Route("api/v1/establishments/{establishmentId:int}/devices")]
[Tags("Devices")]
public class EstablishmentDevicesController(
    IDeviceCommandService deviceCommandService,
    IDeviceQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByEstablishment(int establishmentId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllDevicesQuery(), ct);
        return Ok(items
            .Where(d => d.EstablishmentId.Value == establishmentId)
            .Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int establishmentId,
        [FromBody] CreateNestedDeviceResource resource,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TypeOfMedication>(resource.TypeOfMedication, true, out var medication))
            return BadRequest(new { error = "Invalid TypeOfMedication value." });

        var result = await deviceCommandService.Handle(
            new CreateDeviceCommand(resource.ExactLocation, medication, establishmentId, resource.EnabledSensors ?? ""),
            cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = ((dynamic)result).Error });
        return Created(
            $"/api/v1/establishments/{establishmentId}/devices/{((dynamic)result).Value.Id}",
            DeviceResourceFromEntityAssembler.ToResourceFromEntity(((dynamic)result).Value));
    }

    [HttpPut("{deviceId:int}/sensor-data")]
    public async Task<IActionResult> UpdateSensorData(
        int establishmentId,
        int deviceId,
        [FromBody] UpdateDeviceSensorDataResource resource,
        CancellationToken cancellationToken)
    {
        var items = await queryService.Handle(new GetAllDevicesQuery(), cancellationToken);
        if (items.All(d => d.Id != deviceId || d.EstablishmentId.Value != establishmentId))
            return NotFound();

        if (!Enum.TryParse<DoorStatus>(resource.DoorStatus, true, out var doorStatus))
        {
            return problemDetailsFactory.CreateProblemDetails(
                this,
                StatusCodes.Status400BadRequest,
                MonitoringError.DeviceUpdateFailed,
                "Invalid DoorStatus value.");
        }

        var reading = new SensorReading(
            resource.Temperature,
            resource.Humidity,
            resource.LightIntensity,
            resource.AirQuality,
            resource.Vibration,
            resource.AtmosphericPressure,
            resource.SuspendedParticles);

        var result = await deviceCommandService.Handle(
            new UpdateDeviceSensorDataCommand(deviceId, reading, doorStatus),
            cancellationToken);

        return result switch
        {
            Result<Domain.Model.Aggregates.Device, MonitoringError>.Success success =>
                Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<Domain.Model.Aggregates.Device, MonitoringError>.Failure failure =>
                failure.Error switch
                {
                    MonitoringError.DeviceNotFound => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status404NotFound,
                        MonitoringError.DeviceNotFound,
                        MonitoringErrors.DeviceNotFound.Description),
                    _ => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status400BadRequest,
                        MonitoringError.DeviceUpdateFailed,
                        MonitoringErrors.DeviceUpdateFailed.Description)
                },
            _ => problemDetailsFactory.CreateProblemDetails(
                this,
                StatusCodes.Status500InternalServerError,
                MonitoringError.InternalServerError,
                MonitoringErrors.InternalServerError.Description)
        };
    }

    [HttpDelete("{deviceId:int}")]
    public async Task<IActionResult> Delete(int establishmentId, int deviceId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllDevicesQuery(), ct);
        if (items.All(d => d.Id != deviceId || d.EstablishmentId.Value != establishmentId))
            return NotFound();

        var result = await deviceCommandService.DeleteAsync(deviceId, ct);
        if (result.IsFailure) return NotFound(new { error = ((dynamic)result).Error });
        return NoContent();
    }
}
