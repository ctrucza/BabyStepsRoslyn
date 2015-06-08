using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace NamespacesAndClasses
{
    class Program
    {
        static void Main(string[] args)
        {
            //var solutionFileName = @"..\..\..\SampleSolution\SampleSolution.sln";
            var solutionFileName = args[0];
            var ws = MSBuildWorkspace.Create();
            var solution = ws.OpenSolutionAsync(solutionFileName).Result;
        }
    }
}
