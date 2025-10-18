using System.Runtime.CompilerServices;

namespace Tombatron.Results.Analyzers.Sample;

public class ErrorDetailsExample
{
    public string SampleMethod()
    {
        var result = GetTypedError();

        if (result is Error<string> { Details: SocketTimeoutError })
        {
            return "There was totally a socket timeout.";
        }

        if (result is Error<string> { Details: OtherConnectionError })
        {
            return "There was totally some other kind of thing happening.";
        }

        return result.Unwrap();
    }

    public Result<string> GetTypedError()
    {
        return SocketTimeoutError.Create<string>("This is a sample error.");
    }
}

public class SocketTimeoutError : IErrorDetails
{
    public IErrorResult? ChildError { get; }
    public string[] Messages { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    public SocketTimeoutError(IErrorResult? childError, string[] messages, string callerFilePath, int callerLineNumber)
    {
        ChildError = childError;
        Messages = messages;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public static Result<T> Create<T>(
        string message,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        // ReSharper disable twice ExplicitCallerInfoArgument
        Create<T>([message], childError, callerFilePath, callerLineNumber);

    public static Result<T> Create<T>(string[] messages, IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0) =>
        Result<T>.Error(new SocketTimeoutError(childError, messages, callerFilePath, callerLineNumber));
}

public class OtherConnectionError : IErrorDetails
{
    public IErrorResult? ChildError { get; }
    public string[] Messages { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    public OtherConnectionError(IErrorResult? childError, string[] messages, string callerFilePath,
        int callerLineNumber)
    {
        ChildError = childError;
        Messages = messages;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public static Result<T> Create<T>(
        string message,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        Create<T>([message], childError, callerFilePath, callerLineNumber);

    public static Result<T> Create<T>(
        string[] messages,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        Result<T>.Error(new OtherConnectionError(childError, messages, callerFilePath, callerLineNumber));
}