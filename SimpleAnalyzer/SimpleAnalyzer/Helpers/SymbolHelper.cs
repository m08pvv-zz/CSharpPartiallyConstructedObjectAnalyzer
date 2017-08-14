using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleAnalyzer.Helpers
{
    public static class SymbolHelper
    {
        public static ISymbol GetSymbol(CSharpSyntaxNode node, SemanticModel semanticModel)
        {
            CSharpSyntaxNode syntaxNode;

            switch (node.Kind())
            {
                case SyntaxKind.LockStatement:
                    syntaxNode = ((LockStatementSyntax) node).Expression;
                    break;

                case SyntaxKind.InvocationExpression:
                    var invocationExpressionSyntax = (InvocationExpressionSyntax) node;
                    if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax member
                        && member.Expression is IdentifierNameSyntax className
                        && className.Identifier.Text == nameof(Monitor))
                        syntaxNode = invocationExpressionSyntax.ArgumentList.Arguments[0].Expression;
                    else
                        syntaxNode = invocationExpressionSyntax.Expression;
                    break;

                case SyntaxKind.ElementAccessExpression:
                    var elementAccessExpression = (ElementAccessExpressionSyntax) node;
                    // TODO: we need somehow tell to other code that it's ElementAccessExpression
                    syntaxNode = elementAccessExpression.Expression;
                    break;

                default:
                    syntaxNode = node;
                    break;
            }


            var elementAccessExpressionSyntax = syntaxNode as ElementAccessExpressionSyntax;

            syntaxNode = elementAccessExpressionSyntax != null
                ? elementAccessExpressionSyntax.Expression
                : syntaxNode;

            var symbol = semanticModel.GetSymbolInfo(syntaxNode).Symbol;

            symbol = FixSymbolIfLocalVariable(node, elementAccessExpressionSyntax, symbol, semanticModel);

            if (symbol == null)
            {
                //throw new InvalidOperationException();

            }

            return symbol;
        }

        private static ISymbol FixSymbolIfLocalVariable(CSharpSyntaxNode node,
            ElementAccessExpressionSyntax elementAccessExpressionSyntax, ISymbol symbol, SemanticModel semanticModel)
        {
            if (elementAccessExpressionSyntax == null || symbol == null || symbol.Kind != SymbolKind.Local)
                return symbol;

            var variableDeclaratorSyntaxs = node
                .Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .DescendantNodes()
                .OfType<VariableDeclaratorSyntax>();
            var identifier = ((IdentifierNameSyntax) elementAccessExpressionSyntax.Expression).Identifier.Text;

            var initializerValue = variableDeclaratorSyntaxs
                .Single(x => x.Identifier.Text == identifier).Initializer.Value;

            return semanticModel.GetSymbolInfo(initializerValue).Symbol;
        }
    }
}