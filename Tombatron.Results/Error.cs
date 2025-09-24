using System.Runtime.CompilerServices;
using System.Text;
using static Tombatron.Results.Constants;

namespace Tombatron.Results;

public class Error<T> : Result<T>, IErrorResult where T : notnull
{
    public IErrorResult? ChildError { get; }
    
    private readonly string[] _messages;
    public string Message => string.Join("\n", Messages);
    public string[] Messages => ValidateMessages(_messages);
    
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    public Error(string message, IErrorResult? childError = null, 
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) : this([message], 
        childError, 
        // ReSharper disable twice ExplicitCallerInfoArgument
        callerFilePath,
        callerLineNumber)
    {
    }

    public Error(string[] messages, IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        _messages = messages;
        
        ChildError = childError;
        
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }



    public string ToErrorString()
    {
        var errorStringBuilder = new StringBuilder();
        
        errorStringBuilder.AppendLine($"Error from {this.GetType().Name}");
        errorStringBuilder.AppendLine(ErrorHeaderLineSeparator);
        errorStringBuilder.AppendLine("Message(s):");
        errorStringBuilder.AppendLine(Message);
        errorStringBuilder.AppendLine($"Path: {CallerFilePath}, Line: #{CallerLineNumber}");
        
        if (ChildError is not null)
        {
            errorStringBuilder.AppendLine(ErrorEntrySeparator);
            errorStringBuilder.AppendLine(string.Empty);
            errorStringBuilder.AppendLine(ChildError.ToErrorString());
        }

        return errorStringBuilder.ToString();
    }
    
    private string[] ValidateMessages(string[] messages)
    {
        if (messages is not { Length: > 0 })
        {
            throw new ArgumentException("At least one error message is required", nameof(messages));
        }

        return messages;
    }
}

public class Error(string[] messages) : Result
{
    public Error(string message) : this([message])
    {
    }

    public string[] Messages { get; } = ValidateMessages(messages);

    private static string[] ValidateMessages(string[] messages)
    {
        if (messages is not { Length: > 0 })
        {
            throw new ArgumentException("At least one error message is required", nameof(messages));
        }

        return messages;
    }
}