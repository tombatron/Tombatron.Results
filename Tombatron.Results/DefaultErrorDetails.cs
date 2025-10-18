namespace Tombatron.Results;

public class DefaultErrorDetails : IErrorDetails
{
    public IErrorResult? ChildError { get; }
    public string[] Messages { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    internal DefaultErrorDetails(IErrorResult? childError, string[] messages, string callerFilePath, int callerLineNumber)
    {
        ChildError = childError;
        Messages = messages;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }
    
    internal static IErrorDetails Create(IErrorResult? childError, string[] messages, string callerFilePath, int callerLineNumber) =>
        new DefaultErrorDetails(childError, messages, callerFilePath, callerLineNumber);
}