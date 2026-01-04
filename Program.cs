using System;
using System.Reflection.Metadata.Ecma335;

namespace BackupSystem;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("==== Backup Manager ====");
        PrintUsage();
        while (true)
        {
            Console.Write(">");
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
                Console.WriteLine($"Error: {e.ToString()}");
                continue;
            }

            if(arguments.Count > 0)
            {
                string command = arguments[0].ToLower();

                switch (command)
                {
                    case "exit":
                        return;

                    case "help":
                        PrintUsage();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        PrintUsage();
                        break;
                }
            }

        }
    }

    public static void PrintUsage()
    {
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  add <source> <target> <target2> ...   Start backup monitoring");
        Console.WriteLine("  end <source> <target> <target2> ...   Stop specific backup");
        Console.WriteLine("  restore <source> <target>             Restore files from backup");
        Console.WriteLine("  list                                  Show active backups");
        Console.WriteLine("  exit                                  Quit the application");
        Console.WriteLine();
    }
}