using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Tombatron.Results.Analyzers.Tests;

public class ImplementIErrorDetailsRefactoringTests
{
    [Fact]
    public async Task AddsBoilerplate()
    {
        var input = @"using System.Runtime.CompilerServices;
using Tombatron.Results;

namespace Tombatron.Results;

public interface IErrorDetails
{
    IErrorResult? ChildError { get; }
    string[] Messages { get; }
    string CallerFilePath { get; }
    int CallerLineNumber { get; }
}

public interface IErrorResult { }

public class Result<T> where T : notnull
{
    public static Result<T> Error(IErrorDetails error) => new Result<T>();
}

public class [|SocketTimeoutError|] : IErrorDetails
{
}
";

        var expected = @"using System.Runtime.CompilerServices;
using Tombatron.Results;

namespace Tombatron.Results;

public interface IErrorDetails
{
    IErrorResult? ChildError { get; }
    string[] Messages { get; }
    string CallerFilePath { get; }
    int CallerLineNumber { get; }
}

public interface IErrorResult { }

public class Result<T> where T : notnull
{
    public static Result<T> Error(IErrorDetails error) => new Result<T>();
}

public class SocketTimeoutError : IErrorDetails
{
    public IErrorResult? ChildError { get; }
    public string[] Messages { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    public SocketTimeoutError(IErrorResult? childError, string[] messages, string callerFilePath, int callerLineNumber)
    {
        ChildError = childError;
        Messages = messages;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public static Result<T> Create<T>(
        string message,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = """",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        Create<T>(new string[] { message }, childError, callerFilePath, callerLineNumber);

    public static Result<T> Create<T>(
        string[] messages,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = """",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        Result<T>.Error(new SocketTimeoutError(childError, messages, callerFilePath, callerLineNumber));
}
";
                
        var test = new CSharpCodeRefactoringTest<ImplementIErrorDetailsRefactoring, DefaultVerifier>
        {
            TestCode = input,
            FixedCode = expected,
            CompilerDiagnostics = CompilerDiagnostics.None  // Don't validate compiler diagnostics
        };
    
        await test.RunAsync();
    }
    
    [Fact]
    public async Task AddsBoilerplateAndUsingDirective_WhenMissing()
    {
        var input = @"
namespace Tombatron.Results;

public interface IErrorDetails
{
    IErrorResult? ChildError { get; }
    string[] Messages { get; }
    string CallerFilePath { get; }
    int CallerLineNumber { get; }
}

public interface IErrorResult { }

public class Result<T> where T : notnull
{
    public static Result<T> Error(IErrorDetails error) => new Result<T>();
}

public class [|SocketTimeoutError|] : IErrorDetails
{
}
";

        var expected = @"using System.Runtime.CompilerServices;

namespace Tombatron.Results;

public interface IErrorDetails
{
    IErrorResult? ChildError { get; }
    string[] Messages { get; }
    string CallerFilePath { get; }
    int CallerLineNumber { get; }
}

public interface IErrorResult { }

public class Result<T> where T : notnull
{
    public static Result<T> Error(IErrorDetails error) => new Result<T>();
}

public class SocketTimeoutError : IErrorDetails
{
    public IErrorResult? ChildError { get; }
    public string[] Messages { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }

    public SocketTimeoutError(IErrorResult? childError, string[] messages, string callerFilePath, int callerLineNumber)
    {
        ChildError = childError;
        Messages = messages;
        CallerFilePath = callerFilePath;
        CallerLineNumber = callerLineNumber;
    }

    public static Result<T> Create<T>(
        string message,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = """",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        Create<T>(new string[] { message }, childError, callerFilePath, callerLineNumber);

    public static Result<T> Create<T>(
        string[] messages,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = """",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        Result<T>.Error(new SocketTimeoutError(childError, messages, callerFilePath, callerLineNumber));
}
";
                
        var test = new CSharpCodeRefactoringTest<ImplementIErrorDetailsRefactoring, DefaultVerifier>
        {
            TestCode = input,
            FixedCode = expected,
            CompilerDiagnostics = CompilerDiagnostics.None  // Don't validate compiler diagnostics
        };
    
        await test.RunAsync();
    }
}
