namespace Tombatron.Results;

public interface IErrorDetails
{
    IErrorResult? ChildError { get; }
    string[] Messages { get; }
    string CallerFilePath { get; }
    int CallerLineNumber { get; }
}