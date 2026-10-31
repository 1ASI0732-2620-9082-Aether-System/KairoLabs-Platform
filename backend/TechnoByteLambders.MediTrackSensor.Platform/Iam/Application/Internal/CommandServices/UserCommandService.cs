using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.OutboundServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    IAdminRepository adminRepository,
    AppDbContext context,
    IUnitOfWork unitOfWork,
    IHashingService hashingService,
    ITokenService tokenService) : IUserCommandService
{
    public async Task<Result<User, string>> Handle(SignUpCommand command, CancellationToken cancellationToken = default)
    {
        var email = command.Email.ToLowerInvariant().Trim();
        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
            return new Result<User, string>.Failure(IamErrors.UserAlreadyExists.Description);

        var passwordHash = hashingService.HashPassword(command.Password);
        var user = new User(command with { Email = email }, passwordHash);
        try
        {
            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<User, string>.Success(user);
        }
        catch (OperationCanceledException) { return new Result<User, string>.Failure(IamErrors.UserCreationFailed.Description); }
        catch (DbUpdateException) { return new Result<User, string>.Failure(IamErrors.UserCreationFailed.Description); }
        catch (Exception) { return new Result<User, string>.Failure(IamErrors.UserCreationFailed.Description); }
    }

    public async Task<Result<User, string>> RegisterHealthEntityAsync(
        SignUpCommand command,
        string entityName,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.ToLowerInvariant().Trim();
        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
            return new Result<User, string>.Failure(IamErrors.UserAlreadyExists.Description);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var passwordHash = hashingService.HashPassword(command.Password);
            var user = new User(command with { Email = email }, passwordHash);
            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            var admin = new Admin(new CreateAdminCommand(
                entityName.Trim(),
                $"ENT-{user.Id}",
                string.Empty,
                user.Id));
            await adminRepository.AddAsync(admin, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return new Result<User, string>.Success(user);
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new Result<User, string>.Failure(IamErrors.UserCreationFailed.Description);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new Result<User, string>.Failure(IamErrors.AdminCreationFailed.Description);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new Result<User, string>.Failure(IamErrors.UserCreationFailed.Description);
        }
    }

    public async Task<Result<(User User, string Token), IamError>> Handle(
        SignInCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.ToLowerInvariant().Trim();
        var user = await userRepository.FindByEmailAsync(email, cancellationToken);

        if (user is null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            return new Result<(User, string), IamError>.Failure(IamError.InvalidCredentials);

        try
        {
            var token = tokenService.GenerateToken(user);
            return new Result<(User, string), IamError>.Success((user, token));
        }
        catch (Exception)
        {
            return new Result<(User, string), IamError>.Failure(IamError.InternalServerError);
        }
    }

    public async Task<Result<bool, string>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(id, cancellationToken);
        if (user is null)
            return new Result<bool, string>.Failure(IamErrors.UserNotFound.Description);

        userRepository.Remove(user);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new Result<bool, string>.Success(true);
    }
}