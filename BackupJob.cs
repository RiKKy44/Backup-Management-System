using System;
using System.Threading;
namespace BackupSystem;


public class BackupJob
{
    public string SourcePath { get;}
    public string TargetPath { get;}

    public BackupJob(string sourcePath, string targetPath)
    {
        SourcePath = Path.GetFullPath(sourcePath).Trim();
        TargetPath = Path.GetFullPath(targetPath).Trim();
    }

    private FileSystemWatcher _watcher;
    private void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.EnumerateFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(targetDir, fileName);

            var fileInfo = new FileInfo(file);

            try
            {
                CopyFile(file, destFile);
            }
            catch(IOException e) {
                Console.Error.WriteLine($"Could not copy file {fileName} to {destFile}. Error: {e.Message}");
            }
        }
        foreach(string directory in Directory.EnumerateDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(directory);
            string destDir = Path.Combine(targetDir, dirName);

            CopyDirectory(directory, destDir);
        }
    }
    public void Start()
    {
        if (TargetPath.StartsWith(SourcePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Target directory cannot be inside source directory");
        }
        if (!Directory.Exists(SourcePath))
        {
            throw new DirectoryNotFoundException($"Source does not exist: {SourcePath}");
        }
        if (Directory.Exists(TargetPath))
        {
            if (Directory.GetFileSystemEntries(TargetPath).Length > 0) {
                throw new IOException($"Target exists but is not empty: {TargetPath}");
            }
            
        }
        else
        {
            Directory.CreateDirectory(TargetPath);
        }

        Console.WriteLine($"Copying has started: {SourcePath} --> {TargetPath}");

        CopyDirectory(SourcePath, TargetPath);

        Console.WriteLine("Copy complete");

        _watcher = new FileSystemWatcher(SourcePath);

        _watcher.EnableRaisingEvents = true;

        _watcher.IncludeSubdirectories = true;

        _watcher.EnableRaisingEvents = true;

        _watcher.NotifyFilter = NotifyFilters.LastWrite
            | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.CreationTime;

        _watcher.Created += OnCreated;

        _watcher.Deleted += OnDeleted;

        _watcher.Renamed += OnRenamed;

        _watcher.Changed += OnChanged;

        _watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Monitoring active for: {SourcePath}");

    }


    private void OnCreated(object sender, FileSystemEventArgs fileEvent)
    {
        Console.WriteLine($"Creation reported: {fileEvent.Name}");

        string destPath = Path.Combine(TargetPath, fileEvent.Name);

        try
        {

            if(!File.Exists(destPath) && !Directory.Exists(fileEvent.FullPath))
            {
                return;
            }

            FileAttributes atribute = File.GetAttributes(fileEvent.FullPath);

            if (atribute.HasFlag(FileAttributes.Directory))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                CopyFile(fileEvent.FullPath, destPath);
            }
        }
        catch(IOException exception)
        {
            Console.WriteLine($" OnCreated error: {exception.Message}");
        }
    }
    private void OnDeleted(object sender, FileSystemEventArgs fileEvent)
    {
        string destPath = Path.Combine(TargetPath, fileEvent.Name);

        try
        {
            FileAttributes attribute = File.GetAttributes(destPath);
            if (attribute.HasFlag(FileAttributes.Directory))
            {
                Directory.Delete(destPath, true);
            }
            else
            {
                File.Delete(destPath);
            }
        }
        catch (IOException exception) {
            Console.WriteLine($"OnDeleted error: {exception.Message}");
        }
    }
    private void OnRenamed(object sender, RenamedEventArgs fileEvent)
    {
        string oldPath = Path.Combine(TargetPath, fileEvent.OldName);

        string newPath = Path.Combine(TargetPath, fileEvent.Name);

        try
        {
            FileAttributes attribute = File.GetAttributes(oldPath);
            if (attribute.HasFlag(FileAttributes.Directory)){
                Directory.Move(oldPath, newPath);
            }
            else if(File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
            }
            else
            {
                CopyFile(fileEvent.FullPath, newPath); 
            }
        }
        catch(IOException exception)
        {
            Console.WriteLine($"OnRenamed error: {exception.Message}");
        }
    }
    private void OnChanged(object sender, FileSystemEventArgs fileEvent)
    {
        string destPath = Path.Combine(TargetPath, fileEvent.Name);

        if (Directory.Exists(fileEvent.FullPath)) {
            return;
        }

        int attempts = 0;

        bool success = false;

        while(attempts < 6 && !success)
        {
            try
            {
                CopyFile(fileEvent.FullPath, destPath);
                success = true;
                attempts++;
            }
            catch (IOException exception)
            {
                attempts++;
                Console.WriteLine($"File is busy, attempt nr {attempts}. Error: {exception.Message}");

                Thread.Sleep(100);
            }
            catch (Exception exception2) { 
                Console.WriteLine($"OnChanged error: {exception2.Message}");
                break;
            }
        }

        if (!success)
        {
            Console.WriteLine($"Unable to update file after {attempts} attemps");
        }


    }

    private void CopyFile(string sourceFile, string destFile)
    {
        var fileInfo = new FileInfo(sourceFile);

        if (File.Exists(destFile))
        {
            File.Delete(destFile);
        }

        if(fileInfo.LinkTarget != null)
        {
            string target = fileInfo.LinkTarget.ToString();

            if(Path.IsPathFullyQualified(target) && target.StartsWith(SourcePath))
            {
                target = target.Replace(SourcePath, TargetPath);
            }
            File.CreateSymbolicLink(destFile, target);
        }
        else
        {
            File.Copy(sourceFile, destFile, true);
        }
    }
}