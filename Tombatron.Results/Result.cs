namespace Tombatron.Results;

public class Result<T> where T : notnull
{
    internal Result() { }
    
    public static Result<T> Ok(T value) => 
        new Ok<T>(value);
    
    public static Result<T> Error(string message) => 
        new Error<T>(message);

    public T Unwrap() => this is Ok<T> ok 
        ? ok.Value 
        : throw new ResultUnwrapException($"[ERROR] [{GetType().Name}]: {(this is Error<T> e ? e.Message : "Unexpected result type.")}");
    
    public T UnwrapOr(T defaultValue) => 
        this is Ok<T> ok ? ok.Value : defaultValue;    
}

public abstract class Result
{
    public static readonly Result Ok = new Ok();
    
    public static Result Error(string message) => 
        new Error(message);
}