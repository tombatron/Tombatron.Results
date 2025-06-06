﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tombatron.Results.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class ResultTypeSwitchSuppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor SwitchExpressionExhaustiveRule = new(
        id: SuppressorId,
        suppressedDiagnosticId: SuppressedId,
        justification: Justification);
    
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions =>
        ImmutableArray.Create(SwitchExpressionExhaustiveRule);
    
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            if (diagnostic.Id != SwitchExpressionExhaustiveRule.SuppressedDiagnosticId)
            {
                continue;
            }
            
            var syntaxTree = diagnostic.Location.SourceTree;

            if (syntaxTree is null)
            {
                continue;
            }
            
            var root = syntaxTree.GetRoot(context.CancellationToken);
            
            var switchExpress = root.FindNode(diagnostic.Location.SourceSpan) as SwitchExpressionSyntax;

            if (switchExpress is null)
            {
                continue;
            }
            
            var semanticModel = context.GetSemanticModel(syntaxTree);
            var switchExpressionType = semanticModel.GetTypeInfo(switchExpress.GoverningExpression).Type;

            if (switchExpressionType is not null && IsResultType(switchExpressionType))
            {
                context.ReportSuppression(Suppression.Create(SwitchExpressionExhaustiveRule, diagnostic));
            }
        }
    }

    private static bool IsResultType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedTypeSymbol)
        {
            var fullName = namedTypeSymbol.ToDisplayString();

            return fullName.StartsWith("Tombatron.Results.Result");
        }

        return false;
    }
}