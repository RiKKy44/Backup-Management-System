using System;

namespace BackupSystem;


public class BackupJob
{
    public string SourcePath { get;}
    public string TargetPath { get;}

    public BackupJob(string sourcePath, string targetPath)
    {
        SourcePath = Path.GetFullPath(sourcePath);
        TargetPath = Path.GetFullPath(targetPath);
    }

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
                if (fileInfo.LinkTarget != null)
                {
                    string target = fileInfo.LinkTarget;

                    if (Path.IsPathFullyQualified(target) && target.StartsWith(SourcePath))
                    {
                        target = target.Replace(SourcePath, TargetPath);
                    }

                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }

                    File.CreateSymbolicLink(destFile, target);
                }
                else
                {
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Copy(file, destFile, true);
                }
            }
            catch(IOException e) {
                Console.Error.WriteLine($"Could not copy file {fileName} to {destFile}. Error: {e.ToString()}");

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
    }
}