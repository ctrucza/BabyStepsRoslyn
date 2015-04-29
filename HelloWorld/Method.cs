using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HelloWorld
{
    internal class Method
    {
        private readonly MethodDeclarationSyntax syntax;
        private readonly SemanticModel model;

        public List<string> Calls = new List<string>();
        public string Name => syntax.Identifier.ToString();
        public int Loc => syntax.Body.Statements.Sum(s => s.GetText().Lines.Count - 1);

        public Method(MethodDeclarationSyntax syntax, SemanticModel model)
        {
            this.syntax = syntax;
            this.model = model;

            int length = syntax.Span.Length;

            //CollectCalledMethods();
        }

        private void CollectCalledMethods()
        {
            var descendantNodes = syntax.DescendantNodes();
            var invocations = descendantNodes.OfType<InvocationExpressionSyntax>();
            invocations.ToList().ForEach(
                invocation =>
                {
                    var calledMethod = model.GetSymbolInfo(invocation).Symbol;
                    var containingType = calledMethod.ContainingType;
                    var name = calledMethod.Name;
                    Calls.Add(string.Format("{0} in {1}", name, containingType));
                });
        }
    }
}