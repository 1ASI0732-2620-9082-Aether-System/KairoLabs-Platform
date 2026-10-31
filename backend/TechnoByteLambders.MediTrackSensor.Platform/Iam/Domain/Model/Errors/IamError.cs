namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Errors;

public enum IamError
{
    None,
    UserNotFound,
    UserAlreadyExists,
    InvalidCredentials,
    UserCreationFailed,
    UserUpdateFailed,
    AdminNotFound,
    AdminAlreadyExists,
    AdminCreationFailed,
    AdminUpdateFailed,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}