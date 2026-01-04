using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BackupSystem;

public class BackupManager
{
    private List<BackupJob> _activeJobs = new List<BackupJob>();

    // Object used for thread synchronization
    private readonly object _lock = new object();
    public void AddBackupJob(string source, string target)
    {
        
        if (!Directory.Exists(source)) {
            Console.WriteLine($"Directory not found: {source}");
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
                Console.WriteLine($"Backup job failed ({source} --> {target})");
                Console.WriteLine($"Reason: {exception?.Message}");

                lock (_lock)
                {
                    _activeJobs.Remove(job);
                }
            }
            else if(t.IsCompletedSuccessfully) {
                Console.WriteLine($"Initial copy finished: {source} --> {target}. Monitoring is active");
            }
        });




        Console.WriteLine($"Backup job started in background");
    }
}



