using System;

namespace BackupSystem;


public static class Logger
{
    private static readonly object _lock = new object();


    public static void Write(string message)
    {
        lock (_lock)
        {
            Console.Write("\r");
            Console.WriteLine(message);
            Console.Write("> ");
        }
    }
}