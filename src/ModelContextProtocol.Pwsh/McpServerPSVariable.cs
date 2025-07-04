using ModelContextProtocol.Server;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace ModelContextProtocol.Pwsh
{
    public static class McpServerPSVariable
    {
        public static readonly string Name = "__McpServer__";

        public static bool TryGetMcpServer(this PSCmdlet cmdlet, [NotNullWhen(true)] out IMcpServer? mcp)
        {
            mcp = cmdlet.GetVariableValue(Name) as IMcpServer;
            if (mcp is null)
            {
                cmdlet.WriteError(new(new Exception("$Mcp is null or not defined"), "mcpmissing", ErrorCategory.ObjectNotFound, cmdlet));
                return false;
            }

            return true;
        }

        public static void SetMcpServer(this IDictionary parameters, IMcpServer mcpServer)
        {
            parameters[Name] = mcpServer;
        }
    }
}
