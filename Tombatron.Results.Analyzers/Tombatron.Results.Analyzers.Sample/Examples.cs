// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Tombatron.Results.Analyzers.Sample;

// If you don't see warnings, build the Analyzers Project.

public class Examples
{
    public Result<string> DontHaveToHandleResultBecauseWeAreReturningEntireResult()
    {
        // No compiler error would be thrown here because the Result<string> typed result
        // from the DoWork method is passed as the result. Since we're not handling the
        // result in this method, we're not going to expect the user to handle all three
        // possible states of the result object. 
        var workResult = DoWork();

        return workResult;
    }

    public string HaveToHandleError()
    {
        // Here we would expect a compiler error because the Result<string> typed result
        // from the DoWork method is only handled for the Ok case where as the Warn and Error
        // case are unhandled. 
        var workResult = DoWork();

        if (workResult is Ok<string> okResult)
        {
            return okResult.Value;
        }

        return string.Empty;
    }

    public string NoErrorsEverythingHandled()
    {
        // No compiler error here as every possible result type is handled.
        var workResult = DoWork();

        if (workResult is Ok<string> ok)
        {
            return "ok";
        }

        if (workResult is Error<string> error)
        {
            return "error";
        }

        return string.Empty;
    }

    public string NoErrorsPatternMatching()
    {
        // No error here as the pattern matching covers all of the possibilities.
        var workResult = DoWork();

        return workResult switch
        {
            Ok<string> => "ok",
            Error<string> => "error",
            _ => "whatever"
        };
    }

    public string NoErrorsPatternMatchingAgain()
    {
        // No error here as the case state effectively handles all cases because of the default case. 
        var workResult = DoWork();

        var whatever =  workResult switch
        {
            Ok<string> => "ok",
            Error<string> => "error"
        };

        return whatever;
    }

    public string NoErrorBecauseImUsingUnwrap()
    {
        // I would expect a compiler warning here because we're only partially handling the Result<T>
        // type, and calling `Unwrap` should be considered a last resort because we're not handling
        // all of the possibilities, and if the result isn't Ok<T> then an exception will result. 
        var workResult = DoWork();

        return workResult.Unwrap();
    }

    public string NoErrorWarningBecauseDefaultValueProvided()
    {
        // No warning here because by providing a default value we're essentially handling the Warn<T>
        // and Error<T> cases. 
        var workResult = DoWork();

        return workResult.UnwrapOr("hello");
    }

    public Result<string> DoWork()
    {
        // No compiler error because I'm immediately returning the result. 
        return Result<string>.Ok("hello world");
    }
}