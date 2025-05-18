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
    
    public static async Task VerifySuppressionAsync<TAnalyzer>(string source, string suppressedDiagnosticId)
        where TAnalyzer : DiagnosticSuppressor, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState =
            {
                AnalyzerConfigFiles =
                {
                    // This enables the compiler warning we want to suppress
                    ("/.editorconfig", @"
                        [*.cs]
                        dotnet_diagnostic.CS8509.severity = warning
                    ")
                }
            }
        };
        
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location));
        
        // Expect the warning to be suppressed (no diagnostics)
        test.ExpectedDiagnostics.Clear();
        
        // Specify which warning should be suppressed
        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var compilationOptions = solution.GetProject(projectId).CompilationOptions;
            compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItem(suppressedDiagnosticId, ReportDiagnostic.Suppress));
            
            return solution.WithProjectCompilationOptions(projectId, compilationOptions);
        });
        
        await test.RunAsync();
    }    
}