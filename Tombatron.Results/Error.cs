namespace Tombatron.Results;

public class Error<T>(string[] messages) : Result<T> where T : notnull
{
    public Error(string message) : this([message]) { }

    public string[] Messages => messages;
}

public class Error(string[] messages) : Result
{
    public Error(string message) : this([message]) { }

    public string[] Messages => messages;
}