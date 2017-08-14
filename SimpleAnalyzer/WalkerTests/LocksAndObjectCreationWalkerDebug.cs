using System.Linq;
using NUnit.Framework;
using SimpleAnalyzer;
using SimpleAnalyzer.Walkers;

namespace WalkerTests
{
    [TestFixture]
    public class LocksAndObjectCreationWalkerDebug : SyntaxWalkerDebugBase
    {
        [TestCase(CodeSamples.ActivityDemo, 1, "person")]
        [TestCase(CodeSamples.DictionaryCode, 1, "_buckets")]
        public void InvokeWithSomeCode(string code, int objectsCount, string expectedName)
        {
            var syntaxTree = GetSyntaxTree(code);
            var semanticModel = GetSemanticModel(syntaxTree);

            var locksAndObjectCreationWalkerDebug = new LocksAndObjectCreationsWalker(semanticModel);
            locksAndObjectCreationWalkerDebug.Visit(syntaxTree.GetRoot());
            Assert.AreEqual(objectsCount, locksAndObjectCreationWalkerDebug.NewObjectsAndLocks.Count);
            Assert.AreEqual(expectedName, locksAndObjectCreationWalkerDebug.NewObjectsAndLocks.First().Key.Name);
        }
    }
}