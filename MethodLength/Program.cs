using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace MethodLength
{
    class Program
    {
        static void Main(string[] args)
        {
            //string solutionFilePath = @"H:\Battleship\Battleship.sln";
            string solutionFilePath = @"C:\Users\csaba.trucza\Documents\Hacks\BabyStepsRoslyn\BabyStepsRoslyn.sln";
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionFilePath).Result;
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var root = document.GetSyntaxRootAsync().Result;
                    foreach (var namespaceDeclaration in root.DescendantNodes().OfType<NamespaceDeclarationSyntax>())
                    {
                        foreach (var classDeclaration in namespaceDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>())
                        {
                            CalculateMethodLengthsInClass(classDeclaration, namespaceDeclaration.Name.ToString());
                            CalculateMethodLengthsInInnerClasses(classDeclaration, namespaceDeclaration.Name.ToString());
                        }
                    }
                    

                    //foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                    //{
                    //    var c = GetClass(method);                        
                    //    var totalLines = method.GetText().Lines.Count;
                    //    var bodyLines = method.Body.GetText().Lines.Count;

                    //    var statementsInBody = method.Body.ChildNodes().OfType<StatementSyntax>().Count();
                    //    var totalStatementCount = method.Body.DescendantNodes().OfType<StatementSyntax>().Count();

                    //    var statementLines = method.Body.Statements.Sum(s=>s.GetText().Lines.Count);
                    //    Console.WriteLine("{0}\t{1}\t{2}\t{3}", totalStatementCount, statementsInBody, c.Identifier, method.Identifier);
                    //    //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", c.Identifier, method.Identifier, totalLines, bodyLines, statementsInBody, directStatementCount, totalStatementCount, statementLines);
                    //}
                }
            }
        }

        private static void CalculateMethodLengthsInInnerClasses(ClassDeclarationSyntax classDeclaration, string containerName)
        {
            foreach (var innerClassDeclaration in classDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var s = string.Format("{0}.{1}", containerName, innerClassDeclaration.Identifier);
                CalculateMethodLengthsInClass(innerClassDeclaration, s);
                CalculateMethodLengthsInInnerClasses(innerClassDeclaration, s);
            }
        }

        private static void CalculateMethodLengthsInClass(ClassDeclarationSyntax classDeclaration, string containerName)
        {
            foreach (var methodDeclaration in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var methodContainerName = string.Format("{0}.{1}", containerName, classDeclaration.Identifier); 
                Console.WriteLine("{0}.{1}", methodContainerName, methodDeclaration.Identifier);
            }
        }

        private static ClassDeclarationSyntax GetClass(MethodDeclarationSyntax method)
        {
            while (method.Parent != null)
            {
                if (method.Parent is ClassDeclarationSyntax)
                    return method.Parent as ClassDeclarationSyntax;
                method = method.Parent as MethodDeclarationSyntax;
            }
            return null;
        }
    }
}
