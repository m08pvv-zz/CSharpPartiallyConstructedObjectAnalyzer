using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleAnalyzer.Helpers;

namespace SimpleAnalyzer.Walkers
{
    public class NonLockedObjectsWalker : LocksWalker
    {
        private readonly Dictionary<ISymbol, List<ISymbol>> _newObjectsAndLocks;
        public HashSet<ISymbol> PotentialIssuesFound { get; } = new HashSet<ISymbol>();


        public NonLockedObjectsWalker(Dictionary<ISymbol, List<ISymbol>> newObjectsAndLocks, SemanticModel semanticModel) : base(semanticModel)
        {
            _newObjectsAndLocks = newObjectsAndLocks;
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var identifierText = (node.Expression as IdentifierNameSyntax)?.Identifier.Text;
            if (identifierText != nameof(Type)
                || identifierText != nameof(TypeCode))
            {
                var symbol = SymbolHelper.GetSymbol(node.Expression, SemanticModel);

                if (symbol != null && _newObjectsAndLocks.Keys.Contains(symbol))
                {
                    var objectLocks = _newObjectsAndLocks[symbol];

                    if (!objectLocks.All(x => CurrentLock.Contains(x)))
                    {
                        if (!PotentialIssuesFound.Contains(symbol))
                        {
                            if (GloablOptions.Verbose)
                            {
                                Console.WriteLine($"For node {node}");
                                Console.WriteLine($"With parent {node.Parent}");
                                Console.WriteLine($"In lock(s) {String.Join(", ", objectLocks)}");
                                Console.WriteLine();
                            }
                            PotentialIssuesFound.Add(symbol);
                        }
                    }
                }
            }
            base.VisitMemberAccessExpression(node);
        }
    }
}