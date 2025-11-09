using System.Reflection;

namespace ModelContextProtocol.Pwsh
{
    public static class McpCmdletsModule
    {
        public static string Location => Path.Combine(Directory, "ModelContextProtocol.Pwsh.psd1");

        private static string Directory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
    }
}
