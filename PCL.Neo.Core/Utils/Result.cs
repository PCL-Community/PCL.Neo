namespace PCL.Neo.Core.Utils;

public class Result<TOk, TError>
{
    private readonly TError _error;
    private readonly TOk _ok;

    private Result(TOk ok)
    {
        IsSuccess = true;
        _ok = ok;
        _error = default;
    }

    private Result(TError error)
    {
        IsSuccess = false;
        _ok = default;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public TOk Value => IsSuccess ? _ok : throw new InvalidOperationException("Result is a failure.");
    public TError Error => IsFailure ? _error : throw new InvalidOperationException("Result is success.");

    public static Result<TOk, TError> Ok(TOk value)
    {
        return new Result<TOk, TError>(value);
    }

    public static Result<TOk, TError> Fail(TError error)
    {
        return new Result<TOk, TError>(error);
    }

    public TResult Match<TResult>(Func<TOk, TResult> onSuccess, Func<TError, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(_ok) : onFailure(_error);
    }

    public void Switch(Action<TOk> onSuccess, Action<TError> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
        {
            onSuccess(_ok);
        }
        else
        {
            onFailure(_error);
        }
    }

    public Result<TNewSuccess, TError> Map<TNewSuccess>(Func<TOk, TNewSuccess> mapFunc)
    {
        ArgumentNullException.ThrowIfNull(mapFunc);
        return IsSuccess
            ? Result<TNewSuccess, TError>.Ok(mapFunc(_ok))
            : Result<TNewSuccess, TError>.Fail(_error);
    }

    public Result<TOk, TNewError> MapError<TNewError>(Func<TError, TNewError> mapFunc)
    {
        ArgumentNullException.ThrowIfNull(mapFunc);
        return IsFailure
            ? Result<TOk, TNewError>.Fail(mapFunc(_error))
            : Result<TOk, TNewError>.Ok(_ok);
    }

    public Result<TNewSuccess, TError> AndThen<TNewSuccess>(
        Func<TOk, Result<TNewSuccess, TError>> continuationFunc)
    {
        ArgumentNullException.ThrowIfNull(continuationFunc);
        return IsSuccess ? continuationFunc(_ok) : Result<TNewSuccess, TError>.Fail(_error);
    }

    public static implicit operator Result<TOk, TError>(TOk value)
    {
        return Ok(value);
    }
}
