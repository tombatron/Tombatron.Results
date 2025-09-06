namespace Tombatron.Results;

public class Error<T>(string[] messages) : Result<T> where T : notnull
{
    public Error(string message) : this([message]) { }

    public string[] Messages { get; } = ValidateMessages(messages);

    private static string[] ValidateMessages(string[] messages)
    {
        if (messages is not {Length: > 0})
        {
            throw new ArgumentException("At least one error message is required", nameof(messages));
        }

        return messages;
    }
}

public class Error(string[] messages) : Result
{
    public Error(string message) : this([message]) { }

    public string[] Messages { get; } = ValidateMessages(messages);

    private static string[] ValidateMessages(string[] messages)
    {
        if (messages is not {Length: > 0})
        {
            throw new ArgumentException("At least one error message is required", nameof(messages));
        }

        return messages;
    }
}