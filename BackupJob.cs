using System;
using System.Linq.Expressions;
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
                Logger.Write($"Could not copy file {fileName} to {destFile}. Error: {e.Message}");
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
        CopyDirectory(SourcePath, TargetPath);
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

        Logger.Write($"Monitoring active for: {SourcePath}");

    }


    private void OnCreated(object sender, FileSystemEventArgs fileEvent)
    {
        Logger.Write($"Creation reported: {fileEvent.Name}");

        string destPath = Path.Combine(TargetPath, fileEvent.Name);

        int attempts = 0;
        bool success = false;

        while(attempts < 10 && !success)
        {
            try
            {
                if (!File.Exists(fileEvent.FullPath) && !Directory.Exists(fileEvent.FullPath))
                {
                    return;
                }
                FileAttributes attributes = File.GetAttributes(fileEvent.FullPath);
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    if (!Directory.Exists(destPath))
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    success = true;
                }
                else
                {
                    CopyFile(fileEvent.FullPath, destPath);
                    success = true;
                }
            }
            catch (IOException)
            {
                attempts++;
                Thread.Sleep(100);
            }
            catch (Exception ex) {
                Logger.Write($"OnCreated failed: {ex.Message}");
                break;
            }
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
            Logger.Write($"OnDeleted error: {exception.Message}");
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
            Logger.Write($"OnRenamed error: {exception.Message}");
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
                Logger.Write($"File is busy, attempt nr {attempts}. Error: {exception.Message}");

                Thread.Sleep(100);
            }
            catch (Exception exception2) {
                Logger.Write($"OnChanged error: {exception2.Message}");
                break;
            }
        }

        if (!success)
        {
            Logger.Write($"Unable to update file after {attempts} attemps");
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


    public void Stop()
    {
        if(_watcher != null)
        {
            _watcher.Dispose();

            Logger.Write($"Stopped monitoring: {SourcePath} --> {TargetPath}");
        }
    }

    public void Pause()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
        }
    }
    public void Resume()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = true;
        }
    }


    public void Restore()
    {

    }
}