using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace MethodLength
{
    class Program
    {
        static void Main(string[] args)
        {
            string solutionFilePath = @"H:\Battleship\Battleship.sln";
            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionFilePath).Result;
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var root = document.GetSyntaxRootAsync().Result;
                    foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        var c = GetClass(method);                        
                        var totalLines = method.GetText().Lines.Count;
                        var bodyLines = method.Body.GetText().Lines.Count;

                        var statementsInBody = method.Body.ChildNodes().OfType<StatementSyntax>().Count();
                        var totalStatementCount = method.Body.DescendantNodes().OfType<StatementSyntax>().Count();

                        var statementLines = method.Body.Statements.Sum(s=>s.GetText().Lines.Count);
                        Console.WriteLine("{0}\t{1}\t{2}\t{3}", totalStatementCount, statementsInBody, c.Identifier, method.Identifier);
                        //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", c.Identifier, method.Identifier, totalLines, bodyLines, statementsInBody, directStatementCount, totalStatementCount, statementLines);
                    }
                }
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
