using LibGit2Sharp;
using System;
using System.IO;

internal class Program
{
    private static void Main()
    {
        var currentDirectory = Environment.CurrentDirectory;
        var gitRepositoryPath = Path.Combine(currentDirectory, ".git");

        using (var repo = new Repository(gitRepositoryPath))
        using (var watcher = new FileSystemWatcher(currentDirectory))
        {
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = true;

            watcher.Changed += (sender, eventArgs) =>
            {
                // Ignore changes in the .git folder
                if (!eventArgs.FullPath.StartsWith(gitRepositoryPath, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"File {eventArgs.FullPath} has been changed. Committing changes...");

                    using (var repo = new Repository(gitRepositoryPath))
                    {
                        Commands.Stage(repo, "*");
                        var author = new Signature("Kaven", "kaven@wuwenkai.com", DateTimeOffset.Now);
                        var committer = author;

                        var commit = repo.Commit("Changes auto-committed by FileSystemWatcher", author, committer);
                        Console.WriteLine($"Changes committed with commit id: {commit.Id.Sha}");
                    }
                }
            };

            watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Monitoring directory: {currentDirectory}. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
