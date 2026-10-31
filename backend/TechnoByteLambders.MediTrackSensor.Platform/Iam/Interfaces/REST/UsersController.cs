using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.QueryServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Queries;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.ValueObjects;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Resources;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST.Transform;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using ProblemDetailsFactory = TechnoByteLambders.MediTrackSensor.Platform.Shared.Interfaces.REST.ProblemDetails.ProblemDetailsFactory;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Interfaces.REST;

[ApiController]
[Route("api/v1/users")]
[Tags("Users")]
public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await userQueryService.Handle(new GetAllUsersQuery(), ct);
        return Ok(users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource, CancellationToken ct)
    {
        var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);

        var result = resource.Role == UserRole.Admin && !string.IsNullOrWhiteSpace(resource.EntityName)
            ? await userCommandService.RegisterHealthEntityAsync(command, resource.EntityName.Trim(), ct)
            : await userCommandService.Handle(command, ct);

        if (result is Result<User, string>.Failure f)
            return BadRequest(new { error = f.Error });

        var success = (Result<User, string>.Success)result;

        return Created(
            $"/api/v1/users/{success.Value.Id}",
            UserResourceFromEntityAssembler.ToResourceFromEntity(success.Value)
        );
    }

    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(
        [FromBody] SignInResource resource,
        CancellationToken cancellationToken)
    {
        var result = await userCommandService.Handle(
            new SignInCommand(resource.Email, resource.Password),
            cancellationToken);

        return result switch
        {
            Result<(User User, string Token), IamError>.Success success =>
                Ok(new AuthResource(
                    UserResourceFromEntityAssembler.ToResourceFromEntity(success.Value.User),
                    success.Value.Token)),
            Result<(User User, string Token), IamError>.Failure failure =>
                failure.Error switch
                {
                    IamError.InvalidCredentials => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status401Unauthorized,
                        IamError.InvalidCredentials,
                        IamErrors.InvalidCredentials.Description),
                    _ => problemDetailsFactory.CreateProblemDetails(
                        this,
                        StatusCodes.Status500InternalServerError,
                        IamError.InternalServerError,
                        IamErrors.InternalServerError.Description)
                },
            _ => problemDetailsFactory.CreateProblemDetails(
                this,
                StatusCodes.Status500InternalServerError,
                IamError.InternalServerError,
                IamErrors.InternalServerError.Description)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await userCommandService.DeleteAsync(id, ct);
        if (result is Result<bool, string>.Failure f)
            return NotFound(new { error = f.Error });
        return NoContent();
    }
}
