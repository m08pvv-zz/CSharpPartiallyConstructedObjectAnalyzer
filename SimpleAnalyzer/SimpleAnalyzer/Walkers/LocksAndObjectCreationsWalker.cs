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
        public LocksAndObjectCreationsWalker(SemanticModel semanticModel) : base (semanticModel)
        {
        }

        public Dictionary<ISymbol, List<ISymbol>> NewObjectsAndLocks { get;  } = new Dictionary<ISymbol, List<ISymbol>>();

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (InMethod && CurrentLock.Count > 0)
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

                if (symbol != null)
                {
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
                            {
                                if (GloablOptions.Verbose)
                                {
                                    Console.WriteLine($"For node {node}");
                                    Console.WriteLine($"With parent {node.Parent}");
                                    Console.WriteLine($"In lock(s) {String.Join(", ", locks)}");
                                    Console.WriteLine();
                                }
                                NewObjectsAndLocks.Add(nonVolatileFieldsAndProperty, locks);
                            }
                        }
                    }
                }
            }

            base.VisitObjectCreationExpression(node);
        }
    }
}