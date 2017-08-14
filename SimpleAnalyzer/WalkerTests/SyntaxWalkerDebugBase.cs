using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WalkerTests
{
    public class SyntaxWalkerDebugBase
    {
        public static SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
        {
            var compilation = CSharpCompilation.Create("CoreFX", new[] {syntaxTree});
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            return semanticModel;
        }

        public static SyntaxTree GetSyntaxTree(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            return syntaxTree;
        }
    }
}