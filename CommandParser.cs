using System;
using System.Text.RegularExpressions;

namespace BackupSystem;


public static class CommandParser
{
    public static List<string> Parse(string command)
    {
        var args = new List<string>();

        var regex = new Regex(@"[\""](?<arg>[^\""]+)[\""]|(?<arg>\S+)");

        var matches = regex.Matches(command);

        foreach(Match match in matches)
        {
            args.Add(match.Groups["arg"].Value);
        }
        return args;
    }
}