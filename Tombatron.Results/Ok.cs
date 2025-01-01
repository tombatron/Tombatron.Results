namespace Tombatron.Results;

public sealed class Ok<T>(T value) : Result<T> where T : notnull
{
    public T Value => value;
}