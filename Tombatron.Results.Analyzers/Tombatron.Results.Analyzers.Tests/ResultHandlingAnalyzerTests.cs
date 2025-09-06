using System.Threading.Tasks;
using Xunit;

namespace Tombatron.Results.Analyzers.Tests;

public class ResultHandlingAnalyzerTests
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
                Result<int> result = SomeMethod();
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);
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
                Result<int> result = SomeMethod();
            }

            Result<int> WrapSomeWork() 
            { 
                var workResult = SomeMethod(); 

                return workResult;
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);
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
                Result<int> result = SomeMethod();
            }

            Result<int> WrapSomeWork() 
            { 
                return SomeMethod(); 
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);
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
                Result<int> result = SomeMethod();

                if (result is Ok<int> ok)
                {
                    Console.WriteLine(ok.Value);
                }
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";

        var expectedDiagnostic = VerifyCS.Diagnostic("TBTRA001")
            .WithSpan(9, 29, 9, 35)
            .WithMessage("You must handle all possible cases of the result of type `Result<T>`. 'Error' is unhandled.");
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode, expectedDiagnostic);
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
                Result<int> result = SomeMethod();

                if (result is Error<int> error)
                {
                    Console.WriteLine(error.Messages[0]);
                }
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";

        var expectedDiagnostic = VerifyCS.Diagnostic("TBTRA001")
            .WithSpan(9, 29, 9, 35)
            .WithMessage("You must handle all possible cases of the result of type `Result<T>`. 'Ok' is unhandled.");
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode, expectedDiagnostic);
    }

    [Fact]
    public async Task ErrorIsSuppressedIfUnwrapIsCalledOnResult()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                var result = SomeMethod();
                  
                Console.WriteLine(result.Unwrap());
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);        
    }

    [Fact]
    public async Task ErrorIsSupposedIfUnwrapOrDefaultIsCalledOnResult()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                var result = SomeMethod();
                  
                Console.WriteLine(result.UnwrapOr(-1));
            }

            Result<int> SomeMethod() => new Error<int>(""Something went wrong"");
        }
        ";
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);           
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
                Result<int> result = SomeMethod();

                var handledValue = result switch
                {
                    Error<int> err => ""err""
                };

                Console.WriteLine(handledValue);
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";

        var expectedDiagnostic = VerifyCS.Diagnostic("TBTRA001")
            .WithSpan(9, 29, 9, 35)
            .WithMessage("You must handle all possible cases of the result of type `Result<T>`. 'Ok' is unhandled.");
        
        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode, expectedDiagnostic);        
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
                Result<int> workResult = SomeMethod();

                var whatever = workResult switch 
                {
                    Ok<int> => "":)"",
                    Error<int> => "":(""
                };

                return whatever;
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";

        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);        
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
                Result<int> result = SomeMethod();

                var handledValue = result switch
                {
                    Ok<int> ok => ""ok"",
                    Error<int> err => ""err""
                };

                Console.WriteLine(handledValue);
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";

        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);        
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
                Result<int> result = SomeMethod();

                var handledValue = result switch
                {
                    Ok<int> ok => ""ok"",
                    _ => ""whatever""
                };

                Console.WriteLine(handledValue);
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";

        await VerifyCS.VerifyAnalyzerAsync<ResultHandlingAnalyzer>(testCode);        
    }        
    
    [Fact]
    public async Task SuppressCompilerWarningForExhaustiveResultSwitch()
    {
        const string testCode = @"
        using System;
        using Tombatron.Results;

        class Program
        {
            void Main()
            {
                Result<int> result = SomeMethod();

                var handledValue = result switch
                {
                    Ok<int> ok => ""ok"",
                    Error<int> err => ""err""
                };

                Console.WriteLine(handledValue);
            }

            Result<int> SomeMethod() => new Ok<int>(42);
        }
        ";

        // Test that CS8509 is suppressed by our suppressor
        await VerifyCS.VerifySuppressionAsync<ResultTypeSwitchSuppressor>(testCode, "CS8509");        
    }    
}