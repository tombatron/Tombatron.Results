using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Tombatron.Results.Analyzers;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ImplementIErrorDetailsRefactoring))]
public class ImplementIErrorDetailsRefactoring : CodeRefactoringProvider
{
    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return;
        }
        
        var node = root.FindNode(context.Span);

        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return;
        }
        
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        if (semanticModel is null)
        {
            return;
        }
        
        var diagnostics = semanticModel.GetDiagnostics();
        
        var hasUnimplementedInterfaceMembers = diagnostics.Any(d =>
            d.Id == "CS0535" &&
            d.Location.SourceSpan.IntersectsWith(classDeclarationSyntax.Span));

        if (!hasUnimplementedInterfaceMembers)
        {
            // Interface is fully implemented - we're not going to offer the refactoring. 
            return;
        }
        
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax, context.CancellationToken);
        
        if (classSymbol == null || !classSymbol.AllInterfaces.Any(i => i.Name == "IErrorDetails"))
        {
            return;
        }
        
        var action = CodeAction.Create("Implement IErrorDetails using standard template.", c=> AddBoilerplate(context.Document, classDeclarationSyntax, c));
        
        context.RegisterRefactoring(action);
    }

    private async Task<Document> AddBoilerplate(Document document, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        
        // Make sure compiler services is imported. 
        var root = editor.OriginalRoot;

        if (root is CompilationUnitSyntax compilationUnit && compilationUnit.Usings.All(u => u.Name.ToString() != "System.Runtime.CompilerServices"))
        {
            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Runtime.CompilerServices"));
            
            editor.InsertBefore(compilationUnit.Members.First(), usingDirective);
        }
        
        var className = classDeclarationSyntax.Identifier.Text;
        
        var generator = editor.Generator;

        // Complex types...
        var stringArrayType = generator.ArrayTypeExpression(generator.TypeExpression(SpecialType.System_String));
        var iErrorResultNullableType = SyntaxFactory.NullableType(SyntaxFactory.IdentifierName("IErrorResult"));

        // Define the parameters
        
        var properties = new[]
        {
            generator.PropertyDeclaration("ChildError", iErrorResultNullableType, Accessibility.Public, DeclarationModifiers.ReadOnly),
            generator.PropertyDeclaration("Messages", stringArrayType, Accessibility.Public, DeclarationModifiers.ReadOnly),
            generator.PropertyDeclaration("CallerFilePath", generator.TypeExpression(SpecialType.System_String), Accessibility.Public, DeclarationModifiers.ReadOnly),
            generator.PropertyDeclaration("CallerLineNumber", generator.TypeExpression(SpecialType.System_Int32), Accessibility.Public, DeclarationModifiers.ReadOnly),
        };

        foreach (var prop in properties)
        {
            editor.AddMember(classDeclarationSyntax, prop);
        }
        
        // Define the constructor.
        var constructorArguments = new[]
        {
            generator.ParameterDeclaration("childError", iErrorResultNullableType),
            generator.ParameterDeclaration("messages", stringArrayType),
            generator.ParameterDeclaration("callerFilePath", generator.TypeExpression(SpecialType.System_String)),
            generator.ParameterDeclaration("callerLineNumber", generator.TypeExpression(SpecialType.System_Int32))
        };

        var constructorStatements = new[]
        {
            generator.AssignmentStatement(generator.IdentifierName("ChildError"),
                generator.IdentifierName("childError")),
            generator.AssignmentStatement(generator.IdentifierName("Messages"), generator.IdentifierName("messages")),
            generator.AssignmentStatement(generator.IdentifierName("CallerFilePath"),
                generator.IdentifierName("callerFilePath")),
            generator.AssignmentStatement(generator.IdentifierName("CallerLineNumber"),
                generator.IdentifierName("callerLineNumber"))
        };
        
        var ctor = generator.ConstructorDeclaration(className, constructorArguments, Accessibility.Public, statements: constructorStatements);

        editor.AddMember(classDeclarationSyntax, ctor);
        
        // Static Create<T> methods
        var createMethod1 = SyntaxFactory.ParseMemberDeclaration(@"
        public static Result<T> Create<T>(
            string message,
            IErrorResult? childError = null,
            [CallerFilePath] string callerFilePath = """",
            [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
            Create<T>([message], childError, callerFilePath, callerLineNumber);

        ");

        var createMethod2 = SyntaxFactory.ParseMemberDeclaration($@"
    public static Result<T> Create<T>(
        string[] messages,
        IErrorResult? childError = null,
        [CallerFilePath] string callerFilePath = """",
        [CallerLineNumber] int callerLineNumber = 0) where T : notnull =>
        Result<T>.Error(new {className}(childError, messages, callerFilePath, callerLineNumber));
        ");

        editor.AddMember(classDeclarationSyntax, createMethod1);
        editor.AddMember(classDeclarationSyntax, createMethod2);

        return editor.GetChangedDocument();
    }
}