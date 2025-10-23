// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Tombatron.Results.Analyzers.Sample;

// If you don't see warnings, build the Analyzers Project.

public class NonGenericExamples
{
    public Result DontHaveToHandleResultBecauseWeAreReturningEntireResult()
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

        if (workResult is Ok)
        {
            return "Whatever";
        }

        return string.Empty;
    }

    public string NoErrorsEverythingHandled()
    {
        // No compiler error here as every possible result type is handled.
        var workResult = DoWork();

        if (workResult is Ok ok)
        {
            return "ok";
        }

        if (workResult is Error error)
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
            Ok => "ok",
            Error => "error",
            //_ => "whatever" <-- Don't need this since I've suppressed CS8509.
        };
    }

    public string NoErrorsPatternMatchingAgain()
    {
        // No error here as the case state effectively handles all cases because of the default case. 
        var workResult = DoWork();

        var whatever =  workResult switch
        {
            Ok => "ok",
            Error => "error",
            _ => "whoa"
        };


        return whatever;
    }
    
    public string ErrorBecauseTheSwitchStatementIsntExhaustive()
    {
        // Error here as the switch statement doesn't handles all cases. 
        var workResult = DoWork();
        
        var whatever =  workResult switch // Warning because of non-exhaustive switch-statement.
        {
            Error => "error"
        };

        return whatever;
    }

    public void NoErrorBecauseWeAreCallingVerifyOk()
    {
        var workResult = DoWork();
        
        workResult.VerifyOk();
    }
    
    public Result DoWork()
    {

        var justOn = Result.Ok;

        return justOn;
        // No compiler error because I'm immediately returning the result. 
        //return Result<string>.Ok("hello world");
    }
}