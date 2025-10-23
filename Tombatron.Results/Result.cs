using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;

namespace Tombatron.Results;

public class Result<T> where T : notnull
{
    internal Result() { }
    
    public static Result<T> Ok(T value) => 
        new Ok<T>(value);
    
    public static Result<T> Error(IErrorDetails errorDetails) => new Error<T>(errorDetails);
    
    public static Result<T> Error(
        string[] messages, 
        IErrorResult? childError = null, 
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0) => 
        // ReSharper disable twice ExplicitCallerInfoArgument
        new Error<T>(messages, childError, callerFilePath, callerLineNumber);
        
    public static Result<T> Error(
        string message,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) => 
        // ReSharper disable twice ExplicitCallerInfoArgument
        new Error<T>([message], childError, callerFilePath, callerLineNumber);

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
    public void VerifyOk() 
    {
        if (this is Ok)
        {
            return;
        }

        if (this is Error error)
        {
            throw new ResultUnwrapException($"[ERROR] [{GetType().Name}]: {error.Messages[0]}");
        }
    }
    
    public static readonly Result Ok = new Ok();

    public static Result Error(
        string message, 
        IErrorResult? childError = null, 
        [CallerFilePath] string callerFilePath = "", 
        [CallerLineNumber] int callerLineNumber = 0) =>
        // ReSharper disable twice ExplicitCallerInfoArgument
        new Error([message], childError, callerFilePath, callerLineNumber);
        
    public static Result Error(
        string[] messages,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) => 
        // ReSharper disable twice ExplicitCallerInfoArgumentB
        new Error(messages, childError, callerFilePath, callerLineNumber);
}