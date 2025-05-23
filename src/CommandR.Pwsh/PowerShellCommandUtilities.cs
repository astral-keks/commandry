using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommandR
{
    internal static class PowerShellCommandUtilities
    {
        public static Dictionary<string, string> ParseDictionary(this string text) => text
            .Split("\n").Select(line => line.Split(":")).Where(line => line.Length >= 2).GroupBy(line => line[0], line => line[1..])
            .ToDictionary(line => line.Key, line => string.Join(":", line));
    }
}
