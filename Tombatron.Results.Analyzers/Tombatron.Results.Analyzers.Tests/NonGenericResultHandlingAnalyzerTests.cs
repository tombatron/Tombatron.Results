using System.Threading.Tasks;
using Xunit;

namespace Tombatron.Results.Analyzers.Tests;

public class NonGenericResultHandlingAnalyzerTests
{
    [Fact]
    public async Task NoErrorBecauseNothingIsDoneWithResult()
    {
        const string testCode = @"
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();
            }

            Result SomeMethod() => Result.Ok;
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode);
    }
    
    [Fact]
    public async Task NoErrorBecauseResultIsReturnedWithoutBeingAccessed()
    {
        const string testCode = @"
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();
            }

            Result WrapSomeWork() 
            { 
                var workResult = SomeMethod(); 

                return workResult;
            }

            Result SomeMethod() => Result.Ok;
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode);
    }    
    
    [Fact]
    public async Task NoErrorBecauseResultIsDirectlyReturned()
    {
        const string testCode = @"
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();
            }

            Result WrapSomeWork() 
            { 
                return SomeMethod(); 
            }

            Result SomeMethod() => Result.Ok;
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode);
    }        
    
    [Fact]
    public async Task ErrorBecauseErrorIsNotHandled()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();

                if (result is Ok)
                {
                    Console.WriteLine(""Ok!"");
                }
            }

            Result SomeMethod() => Result.Ok;
        }
        ";

        var expectedDiagnostic = VerifyCS.Diagnostic("TBTRA002")
            .WithSpan(9, 24, 9, 30)
            .WithMessage("You must handle all possible cases of the result of type `Result`. 'Error' is unhandled.");
        
        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode, expectedDiagnostic);
    }    
    
    [Fact]
    public async Task ErrorBecauseOkIsNotHandled()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();

                if (result is Error error)
                {
                    Console.WriteLine(error.Messages[0]);
                }
            }

            Result SomeMethod() => Result.Ok;
        }
        ";

        var expectedDiagnostic = VerifyCS.Diagnostic("TBTRA002")
            .WithSpan(9, 24, 9, 30)
            .WithMessage("You must handle all possible cases of the result of type `Result`. 'Ok' is unhandled.");
        
        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode, expectedDiagnostic);
    }
    
    [Fact]
    public async Task ErrorBecausePatternMatchingDoesntCoverOk()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();

                var handledValue = result switch
                {
                    Error err => ""err""
                };

                Console.WriteLine(handledValue);
            }

            Result SomeMethod() => Result.Ok;
        }
        ";

        var expectedDiagnostic = VerifyCS.Diagnostic("TBTRA002")
            .WithSpan(9, 24, 9, 30)
            .WithMessage("You must handle all possible cases of the result of type `Result`. 'Ok' is unhandled.");
        
        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode, expectedDiagnostic);        
    }
    
    [Fact]
    public async Task SuppressedBecausePatternMatchingCoversOkAndError()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                var example = Testing();

                Console.WriteLine(example);
            }

            string Testing()
            {
                Result workResult = SomeMethod();

                var whatever = workResult switch 
                {
                    Ok => "":)"",
                    Error => "":(""
                };

                return whatever;
            }

            Result SomeMethod() => Result.Ok;
        }
        ";

        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode);        
    }
    
    [Fact]
    public async Task SuppressedBecausePatternMatchingCoversOkAndErrorPart2()
    {
        // TODO: Figure out what the difference is between the usage here and the usage in the above test case. 
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();

                var handledValue = result switch
                {
                    Ok ok => ""ok"",
                    Error err => ""err""
                };

                Console.WriteLine(handledValue);
            }

            Result SomeMethod() => Result.Ok;
        }
        ";

        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode);        
    }

    [Fact]
    public async Task SuppressedBecausePatternMatchingCoversOkAndDefault()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result result = SomeMethod();

                var handledValue = result switch
                {
                    Ok ok => ""ok"",
                    _ => ""whatever""
                };

                Console.WriteLine(handledValue);
            }

            Result SomeMethod() => Result.Ok;
        }
        ";

        await VerifyCS.VerifyAnalyzerAsync<NonGenericResultHandlingAnalyzer>(testCode);        
    }        
}