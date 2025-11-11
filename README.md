# Commandry

A handy .NET tool that bridges PowerShell scripts with Model Context Protocol (MCP) servers, enabling seamless integration between AI assistants and PowerShell-based tools and automation.

## Summary

### Motivation

Modern AI agents need access to powerful tools and automation capabilities to be truly useful. PowerShell provides extensive system management and automation capabilities, but there hasn't been an easy way to expose these capabilities to AI assistants in a standardized manner. Commandry solves this problem by providing a bridge between PowerShell scripts and the Model Context Protocol (MCP), allowing AI assistants to discover, understand, and execute PowerShell-based tools.

### Description

Commandry is a .NET-based tool that:

- **Discovers PowerShell Scripts**: Automatically scans directories for `.ps1` files and converts them into MCP tools
- **Supports PowerShell Functions**: Automatically scans directories for `.psm1` and `.psd1` files and converts functions exported by them into MCP tools
- **Extracts Metadata**: Parses PowerShell comment-based help and parameter attributes to generate rich tool descriptions
- **Provides MCP Server**: Exposes PowerShell scripts as MCP tools that AI assistants can discover and call
- **Supports Multiple Transports**: Offers both STDIO and HTTP transport mechanisms for MCP communication
- **Handles Type Conversion**: Automatically converts between MCP JSON parameters and PowerShell types
- **Real-time Updates**: Monitors script and module directories for changes and updates tool availability dynamically

## MCP Support

### MCP Feature Support

| Feature | Status | Description |
|---------|--------|-------------|
| **Tools** | ✅ | **Fully Supported** - Automatic enumeration of PowerShell scripts and functions as MCP tools with dynamic schema generation, tool execution, result formatting, and comprehensive tool annotations |
| **Resources** | ❌ | Not yet implemented - File and data resource management |
| **Prompts** | ❌ | Not yet implemented - Template and prompt management |
| **Sampling** | ❌ | Not yet implemented - LLM sampling requests |
| **Roots** | ❌ | Not yet implemented - Workspace root management |

#### Tools Capability Details

Commandry's Tools implementation includes:

- **Tool Discovery**: Automatic enumeration of available PowerShell scripts and functions as MCP tools
- **Dynamic Schema Generation**: Converts PowerShell parameter definitions to JSON Schema
- **Tool Execution**: Executes PowerShell scripts and functions with parameters provided by MCP clients
- **Result Formatting**: Converts PowerShell output to MCP-compatible content types
- **Tool Annotations**: Supports MCP tool annotations for better AI assistant understanding:
  - `IdempotentHint`: Indicates if a tool can be safely called multiple times
  - `DestructiveHint`: Warns that a tool may make irreversible changes
  - `ReadOnlyHint`: Indicates a tool only reads data without making changes
  - `OpenWorldHint`: Suggests the tool works with open-world data
- **Change Monitoring**: Real-time updates when PowerShell scripts are added, removed, or modified
- **Dynamic Tool Registration**: Updates the available tool list without requiring server restart

### Supported MCP Transports

#### 1. STDIO Transport (`Commandry.Mcp.StdIO`)

Standard input/output transport for direct communication with MCP clients.

**Command Line Usage:**
```bash
# Basic usage with directory scanning
commandry-mcp-stdio --scan-directory <path-to-scripts>

# Scan multiple directories
commandry-mcp-stdio --scan-directory <path1> --scan-directory <path2>

# Scan PowerShell modules
commandry-mcp-stdio --scan-module <module-name-or-path>

# Combine directory and module scanning
commandry-mcp-stdio --scan-directory <path-to-scripts> --scan-module <module-name-or-path>

# Full help
commandry-mcp-stdio --help
```

**Help Output:**
```
Description:
  Starts MCP server with STDIO transport.

Options:
  --scan-directory <scan-directory>  Directory to scan for PowerShell scripts with non-empty .DESCRIPTION and .ROLE set to 'MCP tool'.
  --scan-module <scan-module>        PowerShell module to scan for PowerShell functions with non-empty .DESCRIPTION and .ROLE set to 'MCP tool'.
  --version                          Show version information
  -?, -h, --help                     Show help and usage information
```

**Features:**
- Direct stdin/stdout communication
- Logging redirected to stderr to avoid interference with MCP protocol
- Suitable for integration with MCP clients that support process-based servers
- Supports scanning multiple directories and PowerShell modules
- Automatic discovery of scripts and functions with proper MCP tool metadata

#### 2. HTTP Transport (`Commandry.Mcp.Http`)

HTTP-based transport using Server-Sent Events (SSE) for web-based MCP communication.

**Command Line Usage:**
```bash
# Basic usage with directory scanning (default port 3001)
commandry-mcp-http --scan-directory <path-to-scripts>

# Scan multiple directories
commandry-mcp-http --scan-directory <path1> --scan-directory <path2>

# Custom port
commandry-mcp-http --scan-directory <path-to-scripts> --port 8080

# Scan PowerShell modules
commandry-mcp-http --scan-module <module-name-or-path>

# Combine directory and module scanning with custom port
commandry-mcp-http --scan-directory <path-to-scripts> --scan-module <module-name-or-path> --port 8080

# Full help
commandry-mcp-http --help
```

**Help Output:**
```
Description:
  Starts MCP server on `http://localhost:<port>/sse`.

Options:
  --scan-directory <scan-directory>  Directory to scan for PowerShell scripts with non-empty .DESCRIPTION and .ROLE set to 'MCP tool'.
  --scan-module <scan-module>        PowerShell module to scan for PowerShell functions with non-empty .DESCRIPTION and .ROLE set to 'MCP tool'.
  --port <port>                      Port to listen on. [default: 3001]
  --version                          Show version information
  -?, -h, --help                     Show help and usage information
```

**Features:**
- HTTP endpoint at `http://localhost:<port>/sse`
- Server-Sent Events for real-time communication
- Supports scanning multiple directories and PowerShell modules
- Automatic discovery of scripts and functions with proper MCP tool metadata

### Writing MCP-Compatible PowerShell Scripts and Functions

When creating PowerShell scripts and functions to be used as MCP tools, follow [these guidelines](GUIDE.md)