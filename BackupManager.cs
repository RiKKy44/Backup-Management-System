using System;
using System.Threading.Tasks;

namespace BackupSystem;

public class BackupManager
{
    private List<BackupJob> _activeJobs = new List<BackupJob>();
    public void AddBackupJob(string source, string target)
    {
        if (!Directory.Exists(source)) {
            Console.WriteLine($"Directory not found: {source}");
        }

        try
        {
            var job = new BackupJob(source, target);


            _activeJobs.Add(job);

            Task backup = Task.Run(() =>
            {
                try
                {
                    job.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Backup job failed ({source} --> {target}). Error: {e.Message}");
                }
            });

            Console.WriteLine($"Backup job started in background");
        }
        catch(Exception e)
        {
            Console.WriteLine($"Failed to initialize job. Error: {e.Message}");
        }
    }

   

}

