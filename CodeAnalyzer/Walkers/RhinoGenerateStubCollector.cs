using Microsoft.CodeAnalysis;
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
    public ICollection<CSharpSyntaxNode> ParameterNodes { get; } = new List<CSharpSyntaxNode>();// This is where we store the T in GenerateStub<T>.
    public List<string> Log { get; } = new ();

    private const string Text1 = "MockRepository";
    private const string Text2 = "GenerateStub";

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        try
        {
            // Search for 8634 - InvocationExpression

            // Quick & dirty guard
            string text = node.Expression.GetText().ToString();
            if (!(text.Contains(Text1) && text.Contains(Text2)))
                return;

            Log.Add($"Entering {this.GetType().Name} with node: {node}");

            // Get expressions we need: Identifier, GenerateStub<T> and specifically T
            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = (MemberAccessExpressionSyntax?)node
                .ChildNodesAndTokens()
                .FirstOrDefault(x => x.RawKind == (int)SyntaxKind.SimpleMemberAccessExpression);

            if (memberAccessExpressionSyntax is null)
            {
                Log.Add("Unknow expression");
                return;
            }

            // Here we are expecting either a Identifier node or an invocationExpression.
            // In case of invocationExpression, just go down the tree until we find an identifierExpression
            CSharpSyntaxNode? tmpNode = memberAccessExpressionSyntax;
            while (tmpNode?.ChildNodes()?.FirstOrDefault()?.RawKind != (int)SyntaxKind.IdentifierName)
                tmpNode = (CSharpSyntaxNode?)tmpNode?.ChildNodes().FirstOrDefault();

            if(tmpNode is null)
            {
                Log.Add($"Something went wrong - tmpNode is null. Parent expression is '{memberAccessExpressionSyntax}'.");
                return;
            }

            IdentifierNameSyntax? identifierNameSyntax = (IdentifierNameSyntax?)tmpNode
                .ChildNodes()
                ?.Where(y => y.RawKind == (int)SyntaxKind.IdentifierName)
                ?.FirstOrDefault(x => ((IdentifierNameSyntax)x).Identifier.Text == Text1);

            if (identifierNameSyntax == null)
            {
                Log.Add($"Found expressionNode, but no matching Identifier for {Text1}");
                return;
            }

            // Get second part + <T>
            GenericNameSyntax? genericNameSyntax = (GenericNameSyntax?)tmpNode
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
            var typeParamSyntax = typeArgumentList?.ChildNodes().FirstOrDefault();


            if (typeParamSyntax is null)
            {
                Log.Add($"ERROR: Type param was null, exiting.");
                return;
            }

            Log.Add($"Found type param: {typeParamSyntax}");

            // Parent of tmpNode can not be null here
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
            SyntaxNodes.Add((CSharpSyntaxNode)tmpNode.Parent); // Node including the '()'
#pragma warning restore CS8604
#pragma warning restore CS8600

            ParameterNodes.Add((CSharpSyntaxNode)typeParamSyntax); // T in <T>
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Exception thrown when entering {node} in parent {node.Parent}{Const.NL}{Const.NL}Ex: {ex.Message}{Const.NL}StackTrace{Const.NL}{ex.StackTrace}", "ERROR");
        }
    }
}

