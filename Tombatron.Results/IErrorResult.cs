namespace Tombatron.Results;

public interface IErrorResult
{
    IErrorResult? ChildError { get; }
    string Message { get; }
    string[] Messages { get; }
    
    string CallerFilePath { get; }
    
    int CallerLineNumber { get; }

    string ToErrorString();
}