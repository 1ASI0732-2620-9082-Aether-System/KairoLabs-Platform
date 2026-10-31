using TechnoByteLambders.MediTrackSensor.Platform.Shared.Domain.Model;

namespace TechnoByteLambders.MediTrackSensor.Platform.Iam.Domain.Model.Errors;

public static class IamErrors
{
    public static readonly Error UserNotFound = new("Iam.UserNotFound", "User not found.");
    public static readonly Error UserAlreadyExists = new("Iam.UserAlreadyExists", "A user with this email already exists.");
    public static readonly Error InvalidCredentials = new("Iam.InvalidCredentials", "Invalid email or password.");
    public static readonly Error UserCreationFailed = new("Iam.UserCreationFailed", "An error occurred while creating the user.");
    public static readonly Error UserUpdateFailed = new("Iam.UserUpdateFailed", "An error occurred while updating the user.");
    public static readonly Error AdminNotFound = new("Iam.AdminNotFound", "Admin not found.");
    public static readonly Error AdminAlreadyExists = new("Iam.AdminAlreadyExists", "An admin for this user already exists.");
    public static readonly Error AdminCreationFailed = new("Iam.AdminCreationFailed", "An error occurred while creating the admin.");
    public static readonly Error AdminUpdateFailed = new("Iam.AdminUpdateFailed", "An error occurred while updating the admin.");
    public static readonly Error InternalServerError = new(
        "Iam.InternalServerError",
        "An unexpected error occurred.");
    
}