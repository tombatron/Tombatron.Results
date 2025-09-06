namespace Tombatron.Results;

public class ResultUnwrapAggregateException(string message, IEnumerable<Exception> innerExceptions) : AggregateException(message, innerExceptions)
{
}
