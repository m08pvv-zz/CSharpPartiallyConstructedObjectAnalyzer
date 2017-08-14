using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleAnalyzer.Helpers;

namespace SimpleAnalyzer.Walkers
{
    public class LocksAndObjectCreationsWalker : LocksWalker
    {
        private bool _inMethod = false;

        public LocksAndObjectCreationsWalker(SemanticModel semanticModel) : base (semanticModel)
        {
        }

        public Dictionary<ISymbol, List<ISymbol>> NewObjectsAndLocks { get;  } = new Dictionary<ISymbol, List<ISymbol>>();

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _inMethod = true;
            base.VisitMethodDeclaration(node);
            _inMethod = false;
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (_inMethod && CurrentLock.Count > 0)
            {
                ISymbol symbol = null;
                if (node.Parent is AssignmentExpressionSyntax assignmentExpressionSyntax)
                {
                    symbol = SymbolHelper.GetSymbol(assignmentExpressionSyntax.Left, SemanticModel);
                }
                else if (node.Parent is ThrowStatementSyntax
                         || node.Parent is YieldStatementSyntax
                         || node.Parent is ArgumentSyntax
                         || node.Parent is ReturnStatementSyntax)
                {
                    // do nothing
                }
                else if (node.Parent.Parent is VariableDeclaratorSyntax variableDeclaratorSyntax)
                {
                    symbol = SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax);
                }
                else
                {
                    throw new NotSupportedException();
                }

                var blockSyntax = node.Parent.Ancestors().OfType<BlockSyntax>().First();

                var analyzeBlockWalker = new AnalyzeBlockWalker(SemanticModel, symbol);
                analyzeBlockWalker.Visit(blockSyntax);

                var nonVolatileFieldsAndProperties = analyzeBlockWalker.NonVolatileFieldsAndProperties.ToList();

                if (nonVolatileFieldsAndProperties.Any())
                {
                    var locks = CurrentLock.ToList();
                    foreach (var nonVolatileFieldsAndProperty in nonVolatileFieldsAndProperties)
                    {
                        if (!NewObjectsAndLocks.ContainsKey(nonVolatileFieldsAndProperty))
                            NewObjectsAndLocks.Add(nonVolatileFieldsAndProperty, locks);
                    }
                }
            }

            base.VisitObjectCreationExpression(node);
        }
    }
}