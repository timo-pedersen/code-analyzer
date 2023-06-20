using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Utils;

namespace CodeAnalyzer.Walkers;

/// <summary>
/// Catches "MockRepository.GenerateStub<T>()"
/// </summary>
public class RhinoGenerateStubCollector : CSharpSyntaxWalker, ISyntaxWalker
{
    public ICollection<CSharpSyntaxNode> SyntaxNodes { get; } = new List<CSharpSyntaxNode>();
    public List<string> Log { get; } = new ();

    private const string Text1 = "MockRepository";
    private const string Text2 = "GenerateStub";

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        //Log.Add ($"Entering {this.GetType().Name} with node: {node}");

        // Search for 8634 - InvocationExpression

        // Quick & dirty guard
        if(!(node.Expression.GetText().ToString().Contains(Text1)
           && node.Expression.GetText().ToString().Contains(Text2))) 
           return;

        Log.Add($"Entering {this.GetType().Name} with node: {node}");

        // Get expressions we need: Identifier, GenerateStub<T> and specifically T
        MemberAccessExpressionSyntax? memberAccessExpressionSyntax = (MemberAccessExpressionSyntax?)node
            .ChildNodesAndTokens()
            .FirstOrDefault(x => x.RawKind == 8689);

        if (memberAccessExpressionSyntax is null)
            return;

        // Debug
        Microsoft.CodeAnalysis.ChildSyntaxList xxx = memberAccessExpressionSyntax.ChildNodesAndTokens();

        IdentifierNameSyntax? identifierNameSyntax = (IdentifierNameSyntax?)memberAccessExpressionSyntax
            .ChildNodes()
            .Where(y => y.RawKind == 8616)
            ?.First(x => ((IdentifierNameSyntax)x).Identifier.Text == Text1);

        if (identifierNameSyntax == null)
            return;

        // Get second part + <T>

        GenericNameSyntax? genericNameSyntax = (GenericNameSyntax?)memberAccessExpressionSyntax
            .ChildNodes()
            .Where(y => y.RawKind == 8618) // GenericName
            ?.First(x => ((GenericNameSyntax)x).Identifier.Text == Text2);

        if(genericNameSyntax == null) return;

        // Test that we have GenerateStub
        Microsoft.CodeAnalysis.SyntaxToken generateStubToken = genericNameSyntax.ChildTokens().First();
        
        if (generateStubToken.ValueText != Text2)
            return;

        // Get TypeArgumentList

        TypeArgumentListSyntax? typeArgumentList =
            (TypeArgumentListSyntax?)genericNameSyntax
            .ChildNodes()
            .Where(y => y.RawKind == (int)SyntaxKind.TypeArgumentList).FirstOrDefault();

        // We expect: '<' + IdentifierName node + '>' contained in  typeArgumentList (only center part is a node)
        var typeParamIdentifierNameSyntax = (IdentifierNameSyntax?)typeArgumentList.ChildNodes().FirstOrDefault();


        if (typeParamIdentifierNameSyntax is null)
            return;

        MessageBox.Show(t);

        Log.Add($"Found type param: {typeParamIdentifierNameSyntax.Identifier.Text}");

        SyntaxNodes.Add(node);

        WpfUtils.MessageBox.ShowList(Log);
    }
}

