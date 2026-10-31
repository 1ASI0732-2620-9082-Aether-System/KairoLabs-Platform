namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Errors;

public enum LogisticsError
{
    None,
    TransportNotFound,
    TransportCreationFailed,
    TransportUpdateFailed,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}
