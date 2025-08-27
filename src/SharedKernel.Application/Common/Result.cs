namespace SharedKernel.Application.Common;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Gets the errors that occurred during the operation.
    /// </summary>
    public string[] Errors { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, new[] { error });
    }

    /// <summary>
    /// Implicit conversion from bool to Result.
    /// </summary>
    public static implicit operator Result(bool succeeded)
    {
        return succeeded ? Success() : Failure("Operation failed");
    }
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
/// <typeparam name="T">The type of the return value.</typeparam>
public class Result<T> : Result
{
    internal Result(bool succeeded, T data, IEnumerable<string> errors) 
        : base(succeeded, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Gets the data returned by the operation.
    /// </summary>
    public T Data { get; init; }

    /// <summary>
    /// Creates a successful result with the specified data.
    /// </summary>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    public static new Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default!, errors);
    }

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static new Result<T> Failure(string error)
    {
        return new Result<T>(false, default!, new[] { error });
    }

    /// <summary>
    /// Implicit conversion from T to Result&lt;T&gt;.
    /// </summary>
    public static implicit operator Result<T>(T data)
    {
        return Success(data);
    }

    /// <summary>
    /// Creates a failed Result&lt;T&gt; from a failed Result.
    /// </summary>
    public static Result<T> FromResult(Result result)
    {
        return new Result<T>(result.Succeeded, default!, result.Errors);
    }
}