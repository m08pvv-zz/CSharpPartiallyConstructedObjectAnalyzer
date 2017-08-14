using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using SimpleAnalyzer;
using SimpleAnalyzer.Walkers;

namespace WalkerTests
{
    [TestFixture]
    public class LocksWalkerDebug
    {
        [TestCase(CodeSamples.ActivityDemo)]
        [TestCase(CodeSamples.DictionaryCode)]
        public void InvokeWithSomeCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("CoreFX", new[] { syntaxTree });
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var locksWalker = new LocksWalker(semanticModel);
            locksWalker.Visit(syntaxTree.GetRoot());
        }
    }
}