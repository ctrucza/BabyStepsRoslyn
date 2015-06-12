using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace NamespacesAndClasses
{
    class Program
    {
        static void Main(string[] args)
        {
            //var solutionFileName = args[0];

            //var solutionFileName = @"..\..\..\SampleSolution\SampleSolution.sln";
            var solutionFileName = @"..\..\..\BabyStepsRoslyn.sln";

            var ws = MSBuildWorkspace.Create();
            var solution = ws.OpenSolutionAsync(solutionFileName).Result;

            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    //ProcessDocumentUsingSyntaxTrees(document);
                    ProcessDocumentUsingSemanticModel(document);
                }
            }
        }

        private static void ProcessDocumentUsingSemanticModel(Document document)
        {
            var semanticModel = document.GetSemanticModelAsync().Result;
            var root = document.GetSyntaxRootAsync().Result;
            foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                ITypeSymbol symbol = (ITypeSymbol) semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (symbol == null)
                    continue;
                
                Console.WriteLine("{0} ({1})", symbol, typeDeclaration.GetText().Lines.Count);

                foreach (var constructor in typeDeclaration.ChildNodes().OfType<ConstructorDeclarationSyntax>())
                {
                    var statementsSize = constructor.Body.Statements.Sum(statement => statement.GetText().Lines.Count);
                    var statementCount = CountStatements(constructor.Body);
                    var bodySize = constructor.Body.GetText().Lines.Count;
                    Console.WriteLine("\t{0} ({1}/{2}/{3})", constructor.Identifier, bodySize, statementsSize, statementCount);
                }
                foreach (var method in typeDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>())
                {
                    var methodName = method.Identifier.ToString();
                    var statementsSize = method.Body.Statements.Sum(statement => statement.GetText().Lines.Count);
                    var statementCount = CountStatements(method.Body);
                    var bodySize = method.Body.GetText().Lines.Count;
                    Console.WriteLine("\t{0} ({1}/{2}/{3})", methodName, bodySize, statementsSize, statementCount);
                }
            }
        }

        private static int CountStatements(BlockSyntax body)
        {
            int result = 0;
            result += body.Statements.Count;
            foreach (var b in body.DescendantNodes().OfType<BlockSyntax>())
            {
                result += b.Statements.Count;
            }
            return result;
        }

        private static void ProcessDocumentUsingSyntaxTrees(Document document)
        {
            var root = document.GetSyntaxRootAsync().Result;
            foreach (var typeDeclarationSyntax in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                ProcessType(typeDeclarationSyntax);
            }
        }

        private static void ProcessType(TypeDeclarationSyntax typeDeclaration)
        {
            string fullName = typeDeclaration.Identifier.ToString();

            while (typeDeclaration.Parent is TypeDeclarationSyntax)
            {
                typeDeclaration = (TypeDeclarationSyntax) typeDeclaration.Parent;
                fullName = typeDeclaration.Identifier + "." + fullName;
            }

            NamespaceDeclarationSyntax ns = typeDeclaration.Parent as NamespaceDeclarationSyntax;
            while (ns != null)
            {
                fullName = ns.Name + "." + fullName;
                ns = ns.Parent as NamespaceDeclarationSyntax;
            }

            Console.WriteLine(fullName);
        }
    }
}
