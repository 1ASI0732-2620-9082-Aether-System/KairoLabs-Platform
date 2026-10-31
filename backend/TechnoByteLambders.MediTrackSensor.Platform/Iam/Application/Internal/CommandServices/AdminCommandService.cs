using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.CommandServices;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Commands;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Errors;
using TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Repositories;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;
using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Repositories;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Application.Internal.CommandServices;

public class AdminCommandService(
    IAdminRepository adminRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IAdminCommandService
{
    // Crea el perfil de admin vinculado a un usuario existente
    // Reglas del frontend:
    // - El users_id debe existir en la tabla users
    // - Solo puede haber un admin por usuario (ExistsByUserId)
    // - El entity_code se usa para que operadores se registren bajo este admin
    public async Task<Result<Admin, string>> Handle(CreateAdminCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await userRepository.FindByIdAsync(command.UserId, cancellationToken);
        if (userExists is null)
            return new Result<Admin, string>.Failure(IamErrors.UserNotFound.Description);

        if (await adminRepository.ExistsByUserIdAsync(command.UserId, cancellationToken))
            return new Result<Admin, string>.Failure(IamErrors.AdminAlreadyExists.Description);

        var admin = new Admin(command);

        try
        {
            await adminRepository.AddAsync(admin, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Admin, string>.Success(admin);
        }
        catch (OperationCanceledException)
        {
            return new Result<Admin, string>.Failure(IamErrors.AdminCreationFailed.Description);
        }
        catch (DbUpdateException)
        {
            return new Result<Admin, string>.Failure(IamErrors.AdminCreationFailed.Description);
        }
        catch (Exception)
        {
            return new Result<Admin, string>.Failure(IamErrors.AdminCreationFailed.Description);
        }
    }

}
