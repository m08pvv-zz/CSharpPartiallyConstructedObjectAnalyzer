using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleAnalyzer.Helpers;

namespace SimpleAnalyzer.Walkers
{
    public class AnalyzeBlockWalker : CSharpSyntaxWalker
    {
        private readonly List<ISymbol> _assignedTo = new List<ISymbol>();
        private readonly SemanticModel _semanticModel;

        public AnalyzeBlockWalker(SemanticModel semanticModel, ISymbol assignedTo)
        {
            _semanticModel = semanticModel;
            _assignedTo.Add(assignedTo);
        }

        // TODO: check backing field for IPropertySymbol for volatile
        public IEnumerable<ISymbol> NonVolatileFieldsAndProperties => _assignedTo
            .Where(x => x is IFieldSymbol field && !field.IsVolatile
                        || x is IPropertySymbol);

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var right = _semanticModel.GetSymbolInfo(node.Right).Symbol;

            if (_assignedTo.Contains(right))
            {
                var symbol = SymbolHelper.GetSymbol(node.Left, _semanticModel);
                _assignedTo.Add(symbol);
            }

            base.VisitAssignmentExpression(node);
        }
    }
}