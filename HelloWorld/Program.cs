using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace HelloWorld
{
    class Class
    {
        private ClassDeclarationSyntax syntax;
        private SemanticModel model;

        public List<Method> Methods = new List<Method>();

        public string Name => syntax.Identifier.ToString();

        public Class(ClassDeclarationSyntax syntax, SemanticModel model)
        {
            this.syntax = syntax;
            this.model = model;

            CollectMethods();
        }

        private void CollectMethods()
        {
            var methods = syntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDeclarationSyntax in methods)
            {
                Methods.Add(new Method(methodDeclarationSyntax, model));
            }
        }
    }

    class Method
    {
        private MethodDeclarationSyntax syntax;
        private SemanticModel model;

        public List<string> Calls = new List<string>();
        public string Name => syntax.Identifier.ToString();

        public Method(MethodDeclarationSyntax syntax, SemanticModel model)
        {
            this.syntax = syntax;
            this.model = model;

            var descendantNodes = syntax.DescendantNodes();
            var invocations = descendantNodes.OfType<InvocationExpressionSyntax>();
            invocations.ToList().ForEach(
                invocation =>
                {
                    var symbol = model.GetSymbolInfo(invocation).Symbol;
                    var containingType = symbol.ContainingType;
                    var name = symbol.Name;
                    Calls.Add(string.Format("{0} in {1}", name, containingType));
                });
        }


    }
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

            List<Class> classes = new List<Class>();

            var syntaxTrees = compilation.SyntaxTrees;
            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
                var classesInSyntaxTree = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDeclarationSyntax in classesInSyntaxTree)
                {
                    classes.Add(new Class(classDeclarationSyntax, semanticModel));
                }
            }

            foreach (var @class in classes)
            {
                Console.WriteLine(@class.Name);
                foreach (var method in @class.Methods)
                {
                    Console.WriteLine("\t" + method.Name);
                    foreach (var calledMethod in method.Calls)
                    {
                        Console.WriteLine("\t\t" + calledMethod);
                    }
                }
            }
        }
    }
}
