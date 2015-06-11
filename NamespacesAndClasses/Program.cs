using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var solutionFileName = @"..\..\..\SampleSolution\SampleSolution.sln";
            //var solutionFileName = @"..\..\..\BabyStepsRoslyn.sln";

            var ws = MSBuildWorkspace.Create();
            var solution = ws.OpenSolutionAsync(solutionFileName).Result;

            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    ProcessDocumentUsingSyntaxTrees(document);
                    //ProcessDocumentUsingSemanticModel(document);
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

                var containingNamespace = symbol.ContainingNamespace;
                var containingType = symbol.ContainingType;

                var s = "";
                if (containingType != null)
                    s += containingType;
                else if (containingNamespace != null)
                    s += containingNamespace;
                s += "." + symbol.Name;
                Console.WriteLine(s);
            }
        }

        private static void ProcessDocumentUsingSyntaxTrees(Document document)
        {
            var root = document.GetSyntaxRootAsync().Result;
            foreach (var namespaceDeclaration in root.ChildNodes().OfType<NamespaceDeclarationSyntax>())
            {
                ProcessNamespace(namespaceDeclaration, namespaceDeclaration.Name.ToString());
            }
        }

        private static void ProcessNamespace(NamespaceDeclarationSyntax namespaceDeclaration, string container)
        {
            ProcessNestedNamespaces(namespaceDeclaration, container);
            ProcessTypesInContainer(namespaceDeclaration, "["+container+"]");
        }

        private static void ProcessNestedNamespaces(NamespaceDeclarationSyntax namespaceDeclaration, string container)
        {
            foreach (var innerNamespace in namespaceDeclaration.ChildNodes().OfType<NamespaceDeclarationSyntax>())
            {
                var containerName = string.Format("{0}.{1}", container, innerNamespace.Name);
                ProcessNamespace(innerNamespace, containerName);
            }
        }

        private static void ProcessTypesInContainer(MemberDeclarationSyntax containerNode, string container)
        {
            foreach (var typeDeclaration in containerNode.ChildNodes().OfType<TypeDeclarationSyntax>())
            {
                ProcessType(typeDeclaration, container);
                var containingClassName = string.Format("{0}.{1}", container, typeDeclaration.Identifier);
                ProcessTypesInContainer(typeDeclaration, containingClassName);
            }

        }

        private static void ProcessType(TypeDeclarationSyntax typeDeclaration, string container)
        {
            Console.WriteLine("{0}.{1}", container, typeDeclaration.Identifier);
        }
    }
}
