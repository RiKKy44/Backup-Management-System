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

            try
            {
                File.Copy(file, destFile, true);
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

    }
}