using System;
using System.Diagnostics;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        var currentDirectory = args.FirstOrDefault() ?? Environment.CurrentDirectory;

        using var watcher = new FileSystemWatcher(currentDirectory);
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.IncludeSubdirectories = true;

        watcher.Changed += (sender, eventArgs) =>
        {
            var changedFilePath = eventArgs.FullPath;

            // Ignore changes in the .git folder
            if (!changedFilePath.Contains(Path.Combine(currentDirectory, ".git"), StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"File {changedFilePath} has been changed. Committing changes...");

                CommitChanges(currentDirectory);
            }
        };

        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Monitoring directory: {currentDirectory}. Press Enter to exit.");
        Console.ReadLine();
    }

    private static void CommitChanges(string repositoryPath)
    {
        using var process = new Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.WorkingDirectory = repositoryPath;
        process.StartInfo.Arguments = "add -A";
        process.Start();
        process.WaitForExit();

        process.StartInfo.Arguments = "commit -m \"Changes auto-committed by FileSystemWatcher\"";
        process.Start();
        process.WaitForExit();
    }
}
