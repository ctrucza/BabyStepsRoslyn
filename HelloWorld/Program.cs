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
            //CountClasses();
            //CountMethodsInClasses();
            CountMethodEvents();
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

        private static void CountMethodEvents()
        {
            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(@"Sample\Sample.csproj").Result;
            foreach (var document in project.Documents)
            {
                var root = document.GetSyntaxRootAsync().Result;
                foreach (var c in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    foreach (var m in c.DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        string className = c.Identifier.ToString();
                        string methodName = m.Identifier.ToString();
                        string methodSize = m.Body.Statements.Sum(s => s.GetText().Lines.Count - 1).ToString();
                        Console.WriteLine("{0}.{1} ({2})", className, methodName, methodSize);
                    }
                }
            }
        }

        private static void CountMethodsInClasses()
        {
            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(@"Sample\Sample.csproj").Result;
            var compilation = project.GetCompilationAsync().Result;

            Console.WriteLine("Classes:");

            List<Class> classes = new List<Class>();

            var syntaxTrees = compilation.SyntaxTrees;
            foreach (var syntaxTree in syntaxTrees)
            {
                CollectClassesFromSyntaxTree(compilation, syntaxTree, classes);
            }

            foreach (var c in classes)
            {
                ShowClass(c);
            }
        }

        private static void CollectClassesFromSyntaxTree(Compilation compilation, SyntaxTree syntaxTree, List<Class> classes)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = (CompilationUnitSyntax) syntaxTree.GetRoot();
            var classesInSyntaxTree = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classDeclarationSyntax in classesInSyntaxTree)
            {
                classes.Add(new Class(classDeclarationSyntax, semanticModel));
            }
        }

        private static void ShowClass(Class c)
        {
            Console.WriteLine(c.Name + "(" + c.Loc + ")");
            if (!c.Methods.Any())
                return;

            ShowMethods(c);
        }

        private static void ShowMethods(Class c)
        {
            foreach (var method in c.Methods)
            {
                Console.WriteLine("\t" + method.Name + "(" + method.Loc + ")");
                if (!method.Calls.Any())
                    continue;

                ShowCalledMethods(method);
            }
        }

        private static void ShowCalledMethods(Method method)
        {
            Console.WriteLine("\t" + "called methods:");
            foreach (var calledMethod in method.Calls)
            {
                Console.WriteLine("\t\t" + calledMethod);
            }
        }
    }
}
