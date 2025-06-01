using System.Collections.Generic;
using System.Linq;

namespace CommandR
{
    internal static class PwshHelp
    {
        public static Dictionary<string, string> ParseDictionary(string text) => text
            .Split("\n").Select(line => line.Split(":")).Where(line => line.Length >= 2).GroupBy(line => line[0].Trim(), line => string.Join(":", line[1..]))
            .ToDictionary(lines => lines.Key, lines => lines.First().Trim());
    }
}
