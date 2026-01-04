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

    //private void CopyDirectory(string sourceDir, string targetDir)
    //{
    //    Directory.CreateDirectory(targetDir);

    //    foreach (string file in Directory.EnumerateFiles(sourceDir)) { 
    //        string fileName = Path.GetFileName(file);
    //        string destFile = Path.Combine(targetDir, fileName);

    //        try
    //        {
                
    //        }
    //    }

    //}
    public void Start()
    {

    }
}