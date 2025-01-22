namespace Tombatron.Results;

public class Error<T>(string message) : Result<T> where T : notnull
{
    public string Message => message;
}

public class Error(string message) : Result
{
    public string Message => message;
}