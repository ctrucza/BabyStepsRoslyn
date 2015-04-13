using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            //OpenSolution();
            //OpenProject();
            CountClasses();
            CountMethodsInClasses();
        }

        private static void OpenSolution()
        {
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(@"BabyStepsRoslyn.sln").Result;
            Console.WriteLine(solution.FilePath);
            Console.WriteLine("{0} Projects:", solution.Projects.Count());
            solution.Projects.ToList().ForEach(project => Console.WriteLine(project.Name));
        }

        private static void OpenProject()
        {
            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(@"HelloWorld\HelloWorld.csproj").Result;
            Console.WriteLine("Project file: " + project.FilePath);
            Console.WriteLine("{0} Documents:", project.Documents.Count());
            project.Documents.ToList().ForEach(document => Console.WriteLine("{0} ({1})", document.Name, document.FilePath));
            project.Documents.ToList().ForEach(document =>
            {
                var syntaxTree = document.GetSyntaxTreeAsync().Result;
                var semanticModel = document.GetSemanticModelAsync().Result;
                var root = document.GetSyntaxRootAsync().Result;
            });
            var compilation = project.GetCompilationAsync().Result;
            compilation.SyntaxTrees.ToList().ForEach(syntaxTree =>
            {
                var root = syntaxTree.GetRoot();
            });
        }

        private static void CountClasses()
        {
            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(@"Sample\Sample.csproj").Result;

            List<ClassDeclarationSyntax> classes = new List<ClassDeclarationSyntax>();

            project.Documents.ToList().ForEach(document =>
            {
                var syntaxTree = document.GetSyntaxTreeAsync().Result;
                var semanticModel = document.GetSemanticModelAsync().Result;
                var root = document.GetSyntaxRootAsync().Result;
                var classesInDocument = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                classes.AddRange(classesInDocument);
            });

            var compilation = project.GetCompilationAsync().Result;
            compilation.SyntaxTrees.ToList().ForEach(syntaxTree =>
            {
                var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
                var classesInSyntaxTree = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
            });

            Console.WriteLine("{0} classes found: ", classes.Count);
            classes.ForEach(c => Console.WriteLine(c.Identifier));
        }

        private static void CountMethodsInClasses()
        {
            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(@"Sample\Sample.csproj").Result;
            var compilation = project.GetCompilationAsync().Result;

            Console.WriteLine("Methods in classes:");
            List<IMethodSymbol> allMethods = new List<IMethodSymbol>();
            Dictionary<IMethodSymbol, List<ISymbol>> calls = new Dictionary<IMethodSymbol, List<ISymbol>>();
            var syntaxTrees = compilation.SyntaxTrees;
            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
                var classesInSyntaxTree = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDeclarationSyntax in classesInSyntaxTree)
                {
                    var methods = classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
                    foreach (var methodDeclarationSyntax in methods)
                    {
                        IMethodSymbol methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
                        allMethods.Add(methodSymbol);

                        calls.Add(methodSymbol, new List<ISymbol>());

                        var descendantNodes = methodDeclarationSyntax.DescendantNodes();
                        var invocations = descendantNodes.OfType<InvocationExpressionSyntax>();
                        invocations.ToList().ForEach(
                            i =>
                            {
                                ISymbol symbol = semanticModel.GetSymbolInfo(i).Symbol;
                                calls[methodSymbol].Add(symbol);
                            });
                    }
                }
            }

            var methodsByClass = allMethods.GroupBy(m => m.ContainingType).ToList();
            methodsByClass.ForEach(m =>
            {
                Console.WriteLine(m.Key);
                foreach (Accessibility accessibility in Enum.GetValues(typeof(Accessibility)))
                {
                    var methodsInClass = m.Where(x=>x.DeclaredAccessibility == accessibility).ToList();
                    if (!methodsInClass.Any())
                        continue;
                    Console.WriteLine(accessibility);
                    methodsInClass.ForEach(x =>
                    {
                        Console.WriteLine("\t{0} {1}", x.ReturnType, x.Name);
                        if (calls[x].Any())
                        {
                            Console.WriteLine("\tcalls to:");
                            calls[x].ForEach(d =>
                            {
                                Console.WriteLine("\t\t" + d.ContainingType+"."+d.Name);
                            });
                        }

                    });
                }
            });
        }
    }
}
