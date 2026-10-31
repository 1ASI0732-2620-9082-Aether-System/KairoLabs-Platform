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
[Route("api/v1/devices")]
[Tags("Devices")]
public class DevicesController(
    IDeviceCommandService deviceCommandService,
    IDeviceQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllDevicesQuery(), ct);
        return Ok(items.Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceResource resource, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TypeOfMedication>(resource.TypeOfMedication, true, out var medication))
            return BadRequest(new { error = "Invalid TypeOfMedication value." });

        var result = await deviceCommandService.Handle(
            new CreateDeviceCommand(resource.ExactLocation, medication, resource.EstablishmentId, resource.EnabledSensors ?? ""), cancellationToken);

        if (result.IsFailure) return BadRequest(new { error = ((dynamic)result).Error });
        return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(((dynamic)result).Value));
    }

    [HttpPut("{id:int}/sensor-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UpdateSensorData(
        int id,
        [FromBody] UpdateDeviceSensorDataResource resource,
        CancellationToken cancellationToken)
    {
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
            new UpdateDeviceSensorDataCommand(id, reading, doorStatus),
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
                    MonitoringError.DeviceUpdateFailed => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status400BadRequest,
                        MonitoringError.DeviceUpdateFailed,
                        MonitoringErrors.DeviceUpdateFailed.Description),
                    _ => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status500InternalServerError,
                        MonitoringError.InternalServerError,
                        MonitoringErrors.InternalServerError.Description)
                },
            _ => problemDetailsFactory.CreateProblemDetails(
                this,
                StatusCodes.Status500InternalServerError,
                MonitoringError.InternalServerError,
                MonitoringErrors.InternalServerError.Description)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await deviceCommandService.DeleteAsync(id, ct);
        if (result.IsFailure) return NotFound(new { error = ((dynamic)result).Error });
        return NoContent();
    }
}
