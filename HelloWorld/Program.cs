using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
			OpenSolution();
			OpenProject();
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

            classes.ForEach(c => Console.WriteLine(c.Identifier));
        }

		private static void CountMethodsInClasses()
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

			classes.ForEach(c =>
			{
				Console.WriteLine(c.Identifier);
				c.Members.OfType<MethodDeclarationSyntax>().ToList().ForEach(m=>Console.WriteLine(m.Identifier));
			});
		}
	}
}
