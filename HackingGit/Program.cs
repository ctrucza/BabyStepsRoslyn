using System;
using System.Linq;
using LibGit2Sharp;

namespace HackingGit
{
    class Program
    {
        static void Main(string[] args)
        {
            string repositoryPath = args[0];
            Repository repository = new Repository(repositoryPath);
            foreach (var indexEntry in repository.Index)
            {
                Console.WriteLine("{0} {1} {2} {3}", indexEntry.Id, indexEntry.Mode, indexEntry.Path, indexEntry.StageLevel);
            }
            foreach (var commit in repository.Commits.Reverse())
            {
                Console.WriteLine(commit.Message);
            }
        }
    }
}
