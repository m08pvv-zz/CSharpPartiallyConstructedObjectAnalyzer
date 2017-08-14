using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleAnalyzer.Helpers;

namespace SimpleAnalyzer.Walkers
{
    public class LocksWalker : CSharpSyntaxWalker
    {
        protected bool InMethod;
        protected SemanticModel SemanticModel;
        protected readonly Stack<ISymbol> CurrentLock = new Stack<ISymbol>();

        public LocksWalker(SemanticModel semanticModel)
        {
            SemanticModel = semanticModel;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            InMethod = true;
            CurrentLock.Clear();
            try
            {
                base.VisitMethodDeclaration(node);
            }
            catch (InvalidOperationException e)
            {
                // We can just swallow it here
            }
            InMethod = false;
            CurrentLock.Clear();
        }

        public override void VisitLockStatement(LockStatementSyntax node)
        {
            var symbol = SymbolHelper.GetSymbol(node, SemanticModel);

            CurrentLock.Push(symbol);
            base.VisitLockStatement(node);
            CurrentLock.Pop();
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax member
                && member.Expression is IdentifierNameSyntax className
                && className.Identifier.Text == nameof(Monitor)
                && node.ArgumentList.Arguments.Count > 0)
            {
                var symbol = SymbolHelper.GetSymbol(node.ArgumentList.Arguments[0].Expression, SemanticModel);
                if (member.Name.Identifier.Text == nameof(Monitor.Enter))
                {
                    CurrentLock.Push(symbol);
                }
                else if (member.Name.Identifier.Text == nameof(Monitor.Exit))
                {
                    if (CurrentLock.Count > 0)
                    {
                        var value = CurrentLock.Pop();
                        if (!value.Equals(symbol))
                            throw new InvalidOperationException("Wrong locking");
                    }
                }
            }
            base.VisitInvocationExpression(node);
        }
    }
}