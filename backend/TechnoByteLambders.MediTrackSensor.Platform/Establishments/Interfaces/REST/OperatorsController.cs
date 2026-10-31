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
[Route("api/v1/operators")]
[Tags("Operators")]
public class OperatorsController(
    IOperatorCommandService operatorCommandService,
    IOperatorQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllOperatorsQuery(), ct);
        return Ok(items.Select(OperatorResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOperatorResource resource,
        CancellationToken cancellationToken)
    {
        var result = await operatorCommandService.Handle(
            new CreateOperatorCommand(resource.Schedule, resource.EstablishmentId, resource.UsersId),
            cancellationToken);

        return result switch
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
                    EstablishmentsError.OperatorCreationFailed => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status400BadRequest,
                        EstablishmentsError.OperatorCreationFailed,
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

    [HttpPut("{id:int}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateOperatorResource resource,
        CancellationToken cancellationToken)
    {
        var result = await operatorCommandService.Handle(
            new UpdateOperatorCommand(id, resource.Schedule, resource.EstablishmentId),
            cancellationToken);

        return result switch
        {
            Result<Domain.Model.Aggregates.Operator, EstablishmentsError>.Success success =>
                Ok(OperatorResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<Domain.Model.Aggregates.Operator, EstablishmentsError>.Failure failure =>
                failure.Error switch
                {
                    EstablishmentsError.OperatorNotFound => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status404NotFound,
                        EstablishmentsError.OperatorNotFound,
                        EstablishmentsErrors.OperatorNotFound.Description),
                    EstablishmentsError.OperatorUpdateFailed => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status400BadRequest,
                        EstablishmentsError.OperatorUpdateFailed,
                        EstablishmentsErrors.OperatorUpdateFailed.Description),
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

    [HttpPut("{id:int}/alert-answered")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> IncrementAlert(int id, CancellationToken cancellationToken)
    {
        var result = await operatorCommandService.Handle(new IncrementOperatorAlertCommand(id), cancellationToken);

        return result switch
        {
            Result<Domain.Model.Aggregates.Operator, EstablishmentsError>.Success success =>
                Ok(OperatorResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<Domain.Model.Aggregates.Operator, EstablishmentsError>.Failure failure =>
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await operatorCommandService.Handle(new DeleteOperatorCommand(id), cancellationToken);

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
                    EstablishmentsError.OperatorDeleteFailed => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status400BadRequest,
                        EstablishmentsError.OperatorDeleteFailed,
                        EstablishmentsErrors.OperatorDeleteFailed.Description),
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
