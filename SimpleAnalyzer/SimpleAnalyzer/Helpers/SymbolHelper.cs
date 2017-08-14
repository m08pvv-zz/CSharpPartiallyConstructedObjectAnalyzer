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
            ExpressionSyntax expressionSyntax = null;

            switch (node.Kind())
            {
                case SyntaxKind.LockStatement:
                    expressionSyntax = ((LockStatementSyntax) node).Expression;
                    break;

                case SyntaxKind.InvocationExpression:
                    var invocationExpressionSyntax = (InvocationExpressionSyntax) node;
                    if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax member
                        && member.Expression is IdentifierNameSyntax className
                        && className.Identifier.Text == nameof(Monitor))
                        expressionSyntax = invocationExpressionSyntax.ArgumentList.Arguments[0].Expression;
                    else
                        expressionSyntax = invocationExpressionSyntax.Expression;
                    break;

                case SyntaxKind.ElementAccessExpression:
                    var elementAccessExpression = (ElementAccessExpressionSyntax) node;
                    expressionSyntax = elementAccessExpression.Expression;
                    break;

                case SyntaxKind.SimpleMemberAccessExpression:
                    var memberAccessExpressionSyntax = (MemberAccessExpressionSyntax) node;
                    expressionSyntax = memberAccessExpressionSyntax.Expression;
                    break;

                case SyntaxKind.IdentifierName:
                    var identifierNameSyntax = (IdentifierNameSyntax) node;
                    expressionSyntax = identifierNameSyntax;
                    break;

                case SyntaxKind.PredefinedType:
                    expressionSyntax = (PredefinedTypeSyntax)node;
                    break;

                case SyntaxKind.ParenthesizedExpression:
                    expressionSyntax = (ParenthesizedExpressionSyntax) node;
                    break;

                case SyntaxKind.ThisExpression:
                    expressionSyntax = (ThisExpressionSyntax) node;
                    break;
                case SyntaxKind.GenericName:
                    expressionSyntax = (GenericNameSyntax) node;
                    break;
                default:
                    throw new InvalidOperationException();
            }


            var elementAccessExpressionSyntax = expressionSyntax as ElementAccessExpressionSyntax;

            expressionSyntax = elementAccessExpressionSyntax != null
                ? elementAccessExpressionSyntax.Expression
                : expressionSyntax;

            var symbol = semanticModel.GetSymbolInfo(expressionSyntax).Symbol;

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
            if (elementAccessExpressionSyntax == null || symbol.Kind != SymbolKind.Local)
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