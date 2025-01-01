using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tombatron.Results.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ResultHandlingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(RuleDescription.FullErrorRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

        foreach (var variable in localDeclaration.Declaration.Variables)
        {
            var typeInfo = context.SemanticModel.GetTypeInfo(localDeclaration.Declaration.Type);
            var type = typeInfo.Type;

            if (type is INamedTypeSymbol { Name: "Result", Arity: 1 } namedType &&
                namedType.ContainingAssembly.Name == "Tombatron.Results")
            {
                // We've found an instance of `Result<T>` in the method. Let's check to see if it's been handled at all...

                // Get the method of block containing the local declaration.
                var parentBlock = variable.Ancestors().OfType<BlockSyntax>().FirstOrDefault();

                if (parentBlock == null)
                    // I guess we're not in a code block...
                    continue;

                // Look for switches or if-statements handling the variable that we detected up above. 
                var references = parentBlock.DescendantNodes()
                    .OfType<ExpressionSyntax>()
                    .Where(node => context.SemanticModel.GetSymbolInfo(node).Symbol?.Name == variable.Identifier.Text)
                    .ToList();

                // Look for Unwrap or UnwrapOr calls.
                references.AddRange(parentBlock.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(node =>
                    {
                        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                        {
                            var identifier = memberAccess.Expression as IdentifierNameSyntax;

                            if (identifier == null)
                            {
                                return false;
                            }
                            
                            var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;

                            if (methodSymbol == null)
                            {
                                return false;
                            }

                            if (!methodSymbol.ContainingAssembly.Name.StartsWith("Tombatron.Results"))
                            {
                                return false;
                            }
                            
                            return identifier.Identifier.Text == variable.Identifier.Text && methodSymbol.Name.StartsWith("Unwrap");
                        }

                        return false;
                    }));

                var hasOkCase = false;
                var hasErrorCase = false;

                foreach (var reference in references)
                {
                    var switchStatement = reference.Ancestors().OfType<SwitchExpressionSyntax>().FirstOrDefault();

                    foreach (var arm in switchStatement?.Arms ?? Enumerable.Empty<SwitchExpressionArmSyntax>())
                    {
                        INamedTypeSymbol? typeSymbol = null;
                        
                        // Check the pattern of the arm
                        switch (arm.Pattern)
                        {
                            case DeclarationPatternSyntax declarationPattern:
                                // Get the type of the pattern
                                typeSymbol = context.SemanticModel.GetTypeInfo(declarationPattern.Type).Type as INamedTypeSymbol;

                                if (typeSymbol?.Name == "Ok" && typeSymbol.ContainingAssembly.Name == "Tombatron.Results")
                                {
                                    hasOkCase = true;
                                }
                                else if (typeSymbol?.Name == "Error" && typeSymbol.ContainingAssembly.Name == "Tombatron.Results")
                                {
                                    hasErrorCase = true;
                                }
                                break;
                            
                            case TypePatternSyntax typePattern:
                                typeSymbol = context.SemanticModel.GetTypeInfo(typePattern.Type).Type as INamedTypeSymbol;

                                if (typeSymbol?.Name == "Ok" && typeSymbol.ContainingAssembly.Name == "Tombatron.Results")
                                {
                                    hasOkCase = true;
                                }
                                else if (typeSymbol?.Name == "Error" && typeSymbol.ContainingAssembly.Name == "Tombatron.Results")
                                {
                                    hasErrorCase = true;
                                }
                                break;

                            case DiscardPatternSyntax:
                                hasOkCase = hasErrorCase = true;
                                break;
                        }
                    }

                    var ifStatement = reference.Ancestors().OfType<IfStatementSyntax>().FirstOrDefault();

                    if (ifStatement != null)
                    {
                        var condition = ifStatement.Condition;

                        if (condition is IsPatternExpressionSyntax isPatternExpression)
                        {
                            var pattern = isPatternExpression.Pattern;

                            if (pattern is DeclarationPatternSyntax declarationPattern)
                            {
                                var typeSymbol =
                                    context.SemanticModel.GetTypeInfo(declarationPattern.Type).Type as INamedTypeSymbol;

                                if (typeSymbol?.Name == "Ok" && typeSymbol.ContainingAssembly.Name == "Tombatron.Results")
                                {
                                    hasOkCase = true;
                                }
                                else if (typeSymbol?.Name == "Error")
                                {
                                    hasErrorCase = true;
                                }
                            }
                        }
                    }

                    var unwrapCalled = reference as InvocationExpressionSyntax; // Assuming that is this is here, then Unwrap or UnwrapOr was called.
                    
                    if (unwrapCalled != null)
                    {
                        // We're going to set these as true because we think that an Unwrap method has been called.
                        hasOkCase = true;
                        hasErrorCase = true;
                    }
                }

                var missingCase = default(string);
                var linkingVerb = "is";

                if (hasOkCase && !hasErrorCase)
                {
                    missingCase = "'Error'";
                }

                if (!hasOkCase && hasErrorCase)
                {
                    missingCase = "'Ok'";
                }

                if (!hasOkCase && !hasErrorCase)
                {
                    missingCase = "'Ok' and 'Error'";
                    linkingVerb = "are";
                }

                if (missingCase != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(RuleDescription.FullErrorRule, variable.Identifier.GetLocation(), missingCase, linkingVerb));
                }
            }
        }
    }
}