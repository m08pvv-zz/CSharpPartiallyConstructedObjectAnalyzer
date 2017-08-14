using System.Linq;
using NUnit.Framework;
using SimpleAnalyzer;

namespace WalkerTests
{
    [TestFixture]
    public class AnalyzerTests
    {
        [TestCase(CodeSamples.ActivityDemo, 1)]
        [TestCase(CodeSamples.DictionaryCode, 1)]
        public void InvokeWithSomeCode(string code, int potentialIssuesFound)
        {
            var analyzeResult = Analyzer.AnalyzeCode(code);
            Assert.AreEqual(potentialIssuesFound, analyzeResult.Count());
        }
    }
}