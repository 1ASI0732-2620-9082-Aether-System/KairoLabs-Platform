using Microsoft.AspNetCore.Mvc;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Transform;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using ProblemDetailsFactory = TechnoByteLambders.MediTrackSensor.Platform.Shared.Interfaces.REST.ProblemDetails.ProblemDetailsFactory;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST;

[ApiController]
[Route("api/v1/admins/{adminId:int}/establishments")]
[Tags("Establishments")]
public class AdminEstablishmentsController(
    IEstablishmentCommandService establishmentCommandService,
    IEstablishmentQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByAdmin(int adminId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllEstablishmentsQuery(), ct);
        return Ok(items
            .Where(e => e.AdminId.Value == adminId)
            .Select(EstablishmentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int adminId,
        [FromBody] CreateNestedEstablishmentResource resource,
        CancellationToken cancellationToken)
    {
        var command = new CreateEstablishmentCommand(
            resource.EstablishmentName,
            resource.EstablishmentType,
            new Address(resource.Address, resource.District, resource.CityRegion, resource.Country),
            new Location(resource.Latitude, resource.Longitude),
            resource.Phone,
            resource.Email,
            resource.Website,
            adminId);

        var result = await establishmentCommandService.Handle(command, cancellationToken);

        return result switch
        {
            Result<Domain.Model.Aggregates.Establishment, EstablishmentsError>.Success success =>
                Created($"/api/v1/establishments/{success.Value.Id}",
                    EstablishmentResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<Domain.Model.Aggregates.Establishment, EstablishmentsError>.Failure failure =>
                failure.Error switch
                {
                    EstablishmentsError.EstablishmentCreationFailed => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status400BadRequest,
                        EstablishmentsError.EstablishmentCreationFailed,
                        EstablishmentsErrors.EstablishmentCreationFailed.Description),
                    _ => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status500InternalServerError,
                        EstablishmentsError.InternalServerError,
                        EstablishmentsErrors.InternalServerError.Description)
                },
            _ => problemDetailsFactory.CreateProblemDetails(
                this,
                StatusCodes.Status500InternalServerError,
                EstablishmentsError.InternalServerError,
                EstablishmentsErrors.InternalServerError.Description)
        };
    }
}
