using System.Runtime.CompilerServices;
using System.Text;
using static Tombatron.Results.Constants;

namespace Tombatron.Results;

public class Error<T> : Result<T>, IErrorResult where T : notnull
{
    public IErrorDetails Details { get; }
    public IErrorResult? ChildError => Details.ChildError;
    public string Message => string.Join("\n", Details.Messages);
    public string[] Messages => Details.Messages;
    public string CallerFilePath => Details.CallerFilePath;
    public int CallerLineNumber => Details.CallerLineNumber;
    
    public Error(IErrorDetails details) => Details = details;

    public Error(string message, IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) : 
            this(DefaultErrorDetails.Create(childError, ErrorUtilities.ValidateMessage(message),  callerFilePath, callerLineNumber)) { }

    public Error(string[] messages, IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) :
            this(DefaultErrorDetails.Create(childError, ErrorUtilities.ValidateMessages(messages), callerFilePath, callerLineNumber)) { }

    public string ToErrorString() => this.FormatErrorToString();
}

public class Error : Result, IErrorResult
{
    public IErrorDetails Details { get; }
    public IErrorResult? ChildError => Details.ChildError;
    public string Message => string.Join("\n", Details.Messages);
    public string[] Messages => Details.Messages;
    public string CallerFilePath => Details.CallerFilePath;
    public int CallerLineNumber => Details.CallerLineNumber;
    
    public Error(IErrorDetails details) => Details = details;

    public Error(string[] messages, IErrorResult? childError = null, 
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) : 
            this(DefaultErrorDetails.Create(childError, ErrorUtilities.ValidateMessages(messages), callerFilePath, callerLineNumber)) { }

    public Error(string message, IErrorResult? childError = null, 
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) : 
            this(DefaultErrorDetails.Create(childError, ErrorUtilities.ValidateMessage(message),  callerFilePath, callerLineNumber)) { }

    public string ToErrorString() => this.FormatErrorToString();
}

internal static class ErrorUtilities
{
    public static string[] ValidateMessages(string[] messages) =>
        messages is not { Length: > 0 }
            ? throw new ArgumentException("At least one error message is required", nameof(messages))
            : messages;
    
    public static string[] ValidateMessage(string message) =>
        ValidateMessages([message]);

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
        var baseName = type.Name.Substring(0, type.Name.IndexOf('`'));

        return $"{baseName}<{string.Join(", ", genericArgs)}>";
    }
}