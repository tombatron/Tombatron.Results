namespace Tombatron.Results;

public abstract class Result<T> where T : notnull
{
    public static Result<T> Ok(T value) => 
        new Ok<T>(value);
    
    public static Result<T> Error(string message) => 
        new Error<T>(message);

    public T Unwrap() => this switch
    {
        Ok<T> ok => ok.Value,

        Error<T> error => throw new ResultUnwrapException($"[ERROR] [{this.GetType().Name}]: {error.Message}"),

        _ => throw new ResultUnwrapException("Not sure how you did it, but congratulations.")
    };
    
    public T UnwrapOr(T defaultValue) => 
        this is Ok<T> ok ? ok.Value : defaultValue;    
}

public abstract class Result
{
    public static readonly Result Ok = new Ok();
    
    public static Result Error(string message) => 
        new Error(message);
    
    // Since the Ok (not-generic) type can't contain a value, does it make sense to have an "Unwrap" method?
}