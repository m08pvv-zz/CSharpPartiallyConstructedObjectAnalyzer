using NUnit.Framework;
using SimpleAnalyzer;
using SimpleAnalyzer.Walkers;

namespace WalkerTests
{
    [TestFixture]
    public class LocksWalkerDebug : SyntaxWalkerDebugBase
    {
        [TestCase(CodeSamples.ActivityDemo)]
        [TestCase(CodeSamples.DictionaryCode)]
        public void InvokeWithSomeCode(string code)
        {
            var syntaxTree = GetSyntaxTree(code);
            var semanticModel = GetSemanticModel(syntaxTree);
            var locksWalker = new LocksWalker(semanticModel);
            locksWalker.Visit(syntaxTree.GetRoot());
        }
    }
}