using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BackupSystem;

public class BackupManager
{
    private List<BackupJob> _activeJobs = new List<BackupJob>();

    // Object used for thread synchronization
    private readonly object _lock = new object();


    public struct BackupInfo
    {
        public int Id;
        public string Source;
        public string Target;
    }

    public List<BackupInfo> GetBackupInfo()
    {
        lock (_lock)
        {
            List<BackupInfo> result = new List<BackupInfo>();

            for (int i = 0; i < _activeJobs.Count; i++)
            {
                result.Add(new BackupInfo
                {
                    Id = i + 1,
                    Source = _activeJobs[i].SourcePath,
                    Target = _activeJobs[i].TargetPath,
                });
            }
            return result;
        }
    }
    public void AddBackupJob(string source, string target)
    {
        
        if (!Directory.Exists(source)) {
            Logger.Write($"Directory not found: {source}");
            return;
        }

        var job = new BackupJob(source, target);
        lock(_lock)
        {
            _activeJobs.Add(job);
        }

        Task backupTask = Task.Run(() =>
        {
               
                job.Start();
             
        });
        backupTask.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                var exception = t.Exception?.InnerException;
                // Cleaning up console row, to get rid of ">"


                Logger.Write($"Backup job failed ({source} --> {target})\nReason: {exception?.Message}");
                lock (_lock)
                {
                    _activeJobs.Remove(job);
                }
            }
            else if(t.IsCompletedSuccessfully) {

                Logger.Write($"Initial copy finished: {source} --> {target}. Monitoring is active");
                
            }
        });
        Logger.Write($"Backup job started in background");
    }
}



