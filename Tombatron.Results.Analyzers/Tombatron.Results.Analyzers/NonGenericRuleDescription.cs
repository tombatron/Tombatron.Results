using Microsoft.CodeAnalysis;

namespace Tombatron.Results.Analyzers;

internal static class NonGenericRuleDescription
{
    private const string Category = "Usage";
    
    private const string FullErrorDiagnosticId = "TBTRA002";
    private static readonly LocalizableString FullErrorTitle = GetResourceString(nameof(Resources.TBTRA002Title));
    private static readonly LocalizableString FullErrorMessageFormat = GetResourceString(nameof(Resources.TBTRA002MessageFormat));

    public static readonly DiagnosticDescriptor FullErrorRule = 
        new(FullErrorDiagnosticId, FullErrorTitle, FullErrorMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    private static LocalizableString GetResourceString(string resourceName) => 
        new LocalizableResourceString(resourceName, Resources.ResourceManager, typeof(Resources));
}