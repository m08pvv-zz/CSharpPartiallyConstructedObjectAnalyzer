using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SimpleAnalyzer.Walkers;

namespace SimpleAnalyzer
{
    public class Analyzer
    {
        class Options
        {
            [Option('d', "directory", Required = true, HelpText = "Directory for search with pattern *.")]
            public string Directory { get; set; }
        }

        private static void Main(string[] args)
        {
            var parsedOptions = (Parser.Default.ParseArguments<Options>(args) as Parsed<Options>)?.Value;
            if (parsedOptions == null)
                return;

            foreach (var fileName in Directory.EnumerateFiles(parsedOptions.Directory, "*.cs"))
            {
                var result = AnalyzeFile(File.ReadAllText(fileName)).ToList();

                if (result.Any())
                {
                    foreach (var symbol in result)
                    {
                        Console.WriteLine($"In file {fileName} found potential issue for symbol {symbol}");
                    }
                    Console.WriteLine();
                }
            }
        }

        private static IEnumerable<ISymbol> AnalyzeFile(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("CoreFX", new[] {syntaxTree});

            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var root = syntaxTree.GetRoot();

            var locksAndObjectCreationsWalker = new LocksAndObjectCreationsWalker(semanticModel);
            locksAndObjectCreationsWalker.Visit(root);
            var newObjectsAndLocks = locksAndObjectCreationsWalker.NewObjectsAndLocks;

            if (newObjectsAndLocks.Count > 0)
            {

                var lockWalker = new NonLockedObjectsWalker(newObjectsAndLocks, semanticModel);

                lockWalker.Visit(root);
                return lockWalker.PotentialIssuesFound;
            }

            return Enumerable.Empty<ISymbol>();
        }
    }
}