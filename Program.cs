using System;

namespace BackupSystem;

public class Program
{
    public static void Main(string[] args)
    {
        BackupManager manager = new BackupManager();
        Console.WriteLine("\t==== Backup Manager ====");
        Logger.Write(Usage());
        while (true)
        {
            var arguments = new List<string>();

            try
            {
                var commandLine = Console.ReadLine();
                if(commandLine == null)
                {
                    break;
                }
                if (string.IsNullOrEmpty(commandLine)) continue;

                arguments = CommandParser.Parse(commandLine);

            }
            catch (Exception e) { 
                Logger.Write($"Error: {e.ToString()}");
                continue;
            }

            if(arguments.Count > 0)
            {
                int count = arguments.Count;
                string command = arguments[0].ToLower();

                switch (command)
                {
                    case "exit":
                        return;

                    case "help":
                        Logger.Write(Usage());
                        break;

                    case "add":
                        if(count < 3)
                        {
                            Logger.Write(Usage());
                            break;
                        }
                        for(int i=2; i < arguments.Count; i++)
                        {
                            string sourcePath = arguments[1];
                            manager.AddBackupJob(sourcePath, arguments[i]);
                        }
                        break;

                    case "list":
                        var list = manager.GetBackupInfo();
                        if (list.Count == 0)
                        {
                            Logger.Write($"There are no current backups");
                        }
                        else
                        {
                            foreach (var item in list)
                            {
                                Logger.Write($"[{item.Id}] {item.Source} --> {item.Target}");
                            }
                        }
                        break;
                    case "restore":
                        if (count < 3)
                        {
                            Logger.Write($"Error: Missing arguments for restore function\n" +
                                $"Usage: restore <backup_path> <restore_path>");
                            break;
                        }

                        string source = arguments[1];
                        string target = arguments[2];
                        manager.RestoreBackup(target, source);
                        break;
                    case "end":
                        if (count<3)
                        {
                            Logger.Write("Usage: end <source> <target>");
                            continue;
                        }
                        manager.RemoveBackupJob(arguments[1], arguments[2]);
                        break;

                    default:
                        Logger.Write($"Unknown command: {command}\n{Usage()}");
                        break;
                }
            }

        }
    }

    public static string Usage()
    {
        string message = @"
Usage:
add <source> <target> ...   Start backup monitoring
end <source> <target> ...   Stop specific backup
restore <source> <target>   Restore files from backup
list                        Show active backups
exit                        Quit the application
";
        return message;
    }
}