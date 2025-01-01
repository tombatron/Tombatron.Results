using Microsoft.CodeAnalysis;

namespace Tombatron.Results.Analyzers;

internal static class RuleDescription
{
    private const string Category = "Usage";
    
    private const string FullErrorDiagnosticId = "TBTRA001";
    private static readonly LocalizableString FullErrorTitle = GetResourceString(nameof(Resources.TBTRA001Title));
    private static readonly LocalizableString FullErrorMessageFormat = GetResourceString(nameof(Resources.TBTRA001MessageFormat));

    public static readonly DiagnosticDescriptor FullErrorRule = 
        new(FullErrorDiagnosticId, FullErrorTitle, FullErrorMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    private static LocalizableString GetResourceString(string resourceName) => 
        new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources));
}