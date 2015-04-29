using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HelloWorld
{
    internal class Class
    {
        private readonly ClassDeclarationSyntax syntax;
        private readonly SemanticModel model;

        public List<Method> Methods = new List<Method>();

        public string Name => syntax.Identifier.ToString();
        public int Loc => Methods.Sum(m => m.Loc);

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
}