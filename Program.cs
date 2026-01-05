using System;
using System.Reflection.Metadata.Ecma335;

namespace BackupSystem;

public class Program
{
    public static void Main(string[] args)
    {
        BackupManager manager = new BackupManager();
        Console.WriteLine("==== Backup Manager ====");
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
                            continue;
                        }
                        for(int i=2; i < arguments.Count; i++)
                        {
                            manager.AddBackupJob(arguments[1], arguments[i]);
                        }
                        break;
                    case "list":
                        var list = manager.GetBackupInfo();
                        if (list.Count == 0)
                        {
                            Logger.Write($"There are no current backups");
                        }
                        foreach(var item in list)
                        {
                            Logger.Write($"[{item.Id}] {item.Source} --> {item.Target}");
                        }
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