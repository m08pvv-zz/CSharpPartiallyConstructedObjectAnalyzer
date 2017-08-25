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
    public static class GloablOptions
    {
        public static bool Verbose { get; set; }
        public static bool AssemblyResolverDebugOutput { get; set; }

        public static void Set(Options options)
        {
            Verbose = options.Verbose;
            AssemblyResolverDebugOutput = options.AssemblyResolverDebugOutput;
        }
    }

    public class Options
    {
        [Option('d', "directory", Required = true, HelpText = "Directory for search with pattern *.cs")]
        public string Directory { get; set; }

        [Option('r', "recursive", Default = false, HelpText = "Option to search recursively in directory")]
        public bool Recursive { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Display debug output")]
        public bool Verbose { get; set; }

        [Option('a', "assmblyresolver", Default = false, HelpText = "Display assembly resolver debug output")]
        public bool AssemblyResolverDebugOutput { get; set; }
    }

    public class Analyzer
    {
        private static void Main(string[] args)
        {
            var parsedOptions = (Parser.Default.ParseArguments<Options>(args) as Parsed<Options>)?.Value;
            if (parsedOptions == null)
                return;

            GloablOptions.Set(parsedOptions);

            var analyzedFilesCount = 0;

            using(AssemblyResolveHelper.CreateHelper())
            {

                foreach (var fileName in Directory.EnumerateFiles(parsedOptions.Directory, "*.cs",
                    parsedOptions.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    var result = AnalyzeCode(File.ReadAllText(fileName)).ToList();
                    ++analyzedFilesCount;

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

            Console.WriteLine($"Analyzed {analyzedFilesCount} file(s)");
        }

        public static IEnumerable<ISymbol> AnalyzeCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("CoreFX", new[] {syntaxTree})
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location));

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