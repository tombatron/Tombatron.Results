namespace Tombatron.Results;

public class Result<T> where T : notnull
{
    internal Result() { }
    
    public static Result<T> Ok(T value) => 
        new Ok<T>(value);
    
    public static Result<T> Error(string[] messages) => 
        new Error<T>(messages);
        
    public static Result<T> Error(string message) => 
        new Error<T>([message]);

    public T Unwrap() => this switch
    {
        Ok<T> ok => ok.Value,
        Error<T> { Messages.Length: 1 } error => 
            throw new ResultUnwrapException($"[ERROR] [{GetType().Name}]: {error.Messages[0]}"),
        Error<T> error => 
            throw new ResultUnwrapAggregateException($"[MULTIPLE ERRORS] [{GetType().Name}]", 
                error.Messages.Select(m => new ResultUnwrapException(m))),
        _ => throw new ResultUnwrapException($"[ERROR] [{GetType().Name}]: Unexpected result type.")
    };

    public T UnwrapOr(T defaultValue) =>
        this is Ok<T> ok ? ok.Value : defaultValue;    
}

public abstract class Result
{
    public static readonly Result Ok = new Ok();

    public static Result Error(string message) =>
        new Error([message]);
        
    public static Result Error(string[] messages) => 
        new Error(messages);
}