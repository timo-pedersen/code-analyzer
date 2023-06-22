using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Utils;
using Const = Constants.Const;

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
        try
        {
            //Log.Add ($"Entering {this.GetType().Name} with node: {node}");

            // Search for 8634 - InvocationExpression

            // Quick & dirty guard
            if (!(node.Expression.GetText().ToString().Contains(Text1)
               && node.Expression.GetText().ToString().Contains(Text2)))
                return;

            Log.Add($"Entering {this.GetType().Name} with node: {node}");

            // Get expressions we need: Identifier, GenerateStub<T> and specifically T
            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = (MemberAccessExpressionSyntax?)node
                .ChildNodesAndTokens()
                .FirstOrDefault(x => x.RawKind == (int)SyntaxKind.SimpleMemberAccessExpression);

            if (memberAccessExpressionSyntax is null)
                return;

            IdentifierNameSyntax? identifierNameSyntax = (IdentifierNameSyntax?)memberAccessExpressionSyntax
                .ChildNodes()
                ?.Where(y => y.RawKind == (int)SyntaxKind.IdentifierName)
                ?.FirstOrDefault(x => ((IdentifierNameSyntax)x).Identifier.Text == Text1);

            if (identifierNameSyntax == null)
                return;

            // Get second part + <T>

            GenericNameSyntax? genericNameSyntax = (GenericNameSyntax?)memberAccessExpressionSyntax
                .ChildNodes()
                .Where(y => y.RawKind == (int)SyntaxKind.GenericName)
                ?.First(x => ((GenericNameSyntax)x).Identifier.Text == Text2);

            if (genericNameSyntax == null) return;

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
            // ToDo: Can be GenericNameSyntax, please note and handle
            // Both Identifiername and GeneriName nodes inherit from SimpleNameSyntax
            var typeParamIdentifierNameSyntax = (NameSyntax?)typeArgumentList?.ChildNodes().FirstOrDefault();

            if (typeParamIdentifierNameSyntax is null)
            {
                Log.Add($"ERROR: Type param was null, exiting.");
                return;
            }

            Log.Add($"Found type param: {typeParamIdentifierNameSyntax}");

            SyntaxNodes.Add(node);

            //WpfUtils.MessageBox.ShowList(Log);
        }
        catch(Exception ex)
        {
            MessageBox.Show($"Exception thrown when entering {node} in parent {node.Parent}{Const.NL}{Const.NL}Ex: {ex.Message}{Const.NL}StackTrace{Const.NL}{ex.StackTrace}", "ERROR");
        }
    }
}

