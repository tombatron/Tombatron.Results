using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Tombatron.Results.Analyzers.Tests;

public static class VerifyCS
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return new DiagnosticResult(diagnosticId, DiagnosticSeverity.Error);
    }

    public static async Task VerifyAnalyzerAsync<TAnalyzer>(string source, params DiagnosticResult[] expectedDiagnostics)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location));

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        
        await test.RunAsync();
    }
}