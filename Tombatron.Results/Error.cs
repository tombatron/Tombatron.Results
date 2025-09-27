using System.Runtime.CompilerServices;
using System.Text;
using static Tombatron.Results.Constants;

namespace Tombatron.Results;

public class Error<T> : Result<T>, IErrorResult where T : notnull
{
    public IErrorResult? ChildError { get; }
    public string Message { get; }
    public string[] Messages { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    public Error(string message, IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) :
        this(
            [message],
            childError,
            // ReSharper disable twice ExplicitCallerInfoArgument
            callerFilePath,
            callerLineNumber
        )
    {
    }

    public Error(string[] messages, IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        Messages = ErrorUtilities.ValidateMessages(messages);
        Message = string.Join("\n", Messages);

        ChildError = childError;

        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public string ToErrorString() => this.FormatErrorToString();
}

public class Error : Result, IErrorResult
{
    public IErrorResult? ChildError { get; }
    public string Message { get; }
    public string[] Messages { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }


    public Error(string[] messages, IErrorResult? childError = null, 
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        Messages = ErrorUtilities.ValidateMessages(messages);
        Message = string.Join("\n", Messages);
        ChildError = childError;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public Error(string message, IErrorResult? childError = null, 
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) : 
        this(
            [message], 
            childError, 
            callerFilePath, 
            callerLineNumber
        )
    {
    }

    public string ToErrorString() => this.FormatErrorToString();
}

internal static class ErrorUtilities
{
    public static string[] ValidateMessages(string[] messages) =>
        messages is not { Length: > 0 }
            ? throw new ArgumentException("At least one error message is required", nameof(messages))
            : messages;

    public static string FormatErrorToString(this IErrorResult @this)
    {
        var errorStringBuilder = new StringBuilder();

        errorStringBuilder.AppendLine($"Error from {GetFriendlyTypeName(@this)}");
        errorStringBuilder.AppendLine(ErrorHeaderLineSeparator);
        errorStringBuilder.AppendLine("Message(s):");
        errorStringBuilder.AppendLine(@this.Message);
        errorStringBuilder.AppendLine($"Path: {@this.CallerFilePath}, Line: #{@this.CallerLineNumber}");

        if (@this.ChildError is not null)
        {
            errorStringBuilder.AppendLine(ErrorEntrySeparator);
            errorStringBuilder.AppendLine(string.Empty);
            errorStringBuilder.AppendLine(@this.ChildError.ToErrorString());
        }

        return errorStringBuilder.ToString();
    }

    private static string GetFriendlyTypeName(IErrorResult errorResult)
    {
        var type = errorResult.GetType();

        if (!type.IsGenericType)
        {
            return type.Name;
        }
        
        var genericArgs = type.GetGenericArguments().Select(t => t.Name).ToArray();
        var baseName = type.Name[..type.Name.IndexOf('`')];

        return $"{baseName}<{string.Join(", ", genericArgs)}>";
    }
}