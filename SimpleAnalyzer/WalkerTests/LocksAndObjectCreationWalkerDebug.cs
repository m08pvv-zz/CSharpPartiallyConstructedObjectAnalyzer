using NUnit.Framework;
using SimpleAnalyzer;
using SimpleAnalyzer.Walkers;

namespace WalkerTests
{
    [TestFixture]
    public class LocksAndObjectCreationWalkerDebug : SyntaxWalkerDebugBase
    {
        [TestCase(CodeSamples.ActivityDemo, 1)]
        [TestCase(CodeSamples.DictionaryCode, 1)]
        public void InvokeWithSomeCode(string code, int objectsCount)
        {
            var syntaxTree = GetSyntaxTree(code);
            var semanticModel = GetSemanticModel(syntaxTree);

            var locksAndObjectCreationWalkerDebug = new LocksAndObjectCreationsWalker(semanticModel);
            locksAndObjectCreationWalkerDebug.Visit(syntaxTree.GetRoot());
            Assert.AreEqual(objectsCount, locksAndObjectCreationWalkerDebug.NewObjectsAndLocks.Count);
        }
    }
}