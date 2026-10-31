namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Application.Patterns;

/// <summary>
///     Represents the outcome of an operation that can succeed with a value or fail with an error.
/// </summary>
public abstract record Result<TValue, TError>
{
    public sealed record Success(TValue Value) : Result<TValue, TError>;
    public sealed record Failure(TError Error) : Result<TValue, TError>;

    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;

    public Result<TNext, TError> Map<TNext>(Func<TValue, TNext> onSuccess) =>
        this switch
        {
            Success s => new Result<TNext, TError>.Success(onSuccess(s.Value)),
            Failure f => new Result<TNext, TError>.Failure(f.Error),
            _ => throw new InvalidOperationException("Unknown Result type")
        };

    public TResult Fold<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Value),
            Failure f => onFailure(f.Error),
            _ => throw new InvalidOperationException("Unknown Result type")
        };

    public void Match(Action<TValue> onSuccess, Action<TError> onFailure)
    {
        if (this is Success s) onSuccess(s.Value);
        else if (this is Failure f) onFailure(f.Error);
    }
}
