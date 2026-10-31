using Microsoft.AspNetCore.Mvc;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST.Transform;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using ProblemDetailsFactory = TechnoByteLambders.MediTrackSensor.Platform.Shared.Interfaces.REST.ProblemDetails.ProblemDetailsFactory;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Interfaces.REST;

[ApiController]
[Route("api/v1/establishments/{establishmentId:int}/operators")]
[Tags("Operators")]
public class EstablishmentOperatorsController(
    IOperatorCommandService operatorCommandService,
    IOperatorQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByEstablishment(int establishmentId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllOperatorsQuery(), ct);
        return Ok(items
            .Where(o => o.EstablishmentId == establishmentId)
            .Select(OperatorResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int establishmentId,
        [FromBody] CreateNestedOperatorResource resource,
        CancellationToken cancellationToken)
    {
        var result = await operatorCommandService.Handle(
            new CreateOperatorCommand(resource.Schedule, establishmentId, resource.UsersId),
            cancellationToken);

        return ToOperatorResult(result);
    }

    [HttpPut("{operatorId:int}")]
    public async Task<IActionResult> Update(
        int establishmentId,
        int operatorId,
        [FromBody] UpdateNestedOperatorResource resource,
        CancellationToken cancellationToken)
    {
        var result = await operatorCommandService.Handle(
            new UpdateOperatorCommand(operatorId, resource.Schedule, establishmentId),
            cancellationToken);

        return ToOperatorResult(result);
    }

    [HttpPut("{operatorId:int}/alert-answered")]
    public async Task<IActionResult> IncrementAlert(int establishmentId, int operatorId, CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllOperatorsQuery(), ct);
        if (items.All(o => o.Id != operatorId || o.EstablishmentId != establishmentId))
            return NotFound();

        var result = await operatorCommandService.Handle(new IncrementOperatorAlertCommand(operatorId), ct);
        return ToOperatorResult(result);
    }

    [HttpDelete("{operatorId:int}")]
    public async Task<IActionResult> Delete(int establishmentId, int operatorId, CancellationToken cancellationToken)
    {
        var items = await queryService.Handle(new GetAllOperatorsQuery(), cancellationToken);
        if (items.All(o => o.Id != operatorId || o.EstablishmentId != establishmentId))
            return NotFound();

        var result = await operatorCommandService.Handle(new DeleteOperatorCommand(operatorId), cancellationToken);

        return result switch
        {
            Result<bool, EstablishmentsError>.Success => NoContent(),
            Result<bool, EstablishmentsError>.Failure failure =>
                failure.Error switch
                {
                    EstablishmentsError.OperatorNotFound => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status404NotFound,
                        EstablishmentsError.OperatorNotFound,
                        EstablishmentsErrors.OperatorNotFound.Description),
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

    private IActionResult ToOperatorResult(Result<Domain.Model.Aggregates.Operator, EstablishmentsError> result) =>
        result switch
        {
            Result<Domain.Model.Aggregates.Operator, EstablishmentsError>.Success success =>
                Ok(OperatorResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<Domain.Model.Aggregates.Operator, EstablishmentsError>.Failure failure =>
                failure.Error switch
                {
                    EstablishmentsError.EstablishmentNotFound => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status404NotFound,
                        EstablishmentsError.EstablishmentNotFound,
                        EstablishmentsErrors.EstablishmentNotFound.Description),
                    EstablishmentsError.OperatorNotFound => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status404NotFound,
                        EstablishmentsError.OperatorNotFound,
                        EstablishmentsErrors.OperatorNotFound.Description),
                    EstablishmentsError.OperatorCreationFailed or EstablishmentsError.OperatorUpdateFailed =>
                        problemDetailsFactory.CreateProblemDetails(
                            this,
                            StatusCodes.Status400BadRequest,
                            failure.Error,
                            EstablishmentsErrors.OperatorCreationFailed.Description),
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
