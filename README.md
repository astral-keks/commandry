# Commandry

A handy .NET tool that bridges PowerShell scripts with Model Context Protocol (MCP) servers, enabling seamless integration between AI assistants and PowerShell-based tools and automation.

## Summary

### Motivation

Modern AI agents need access to powerful tools and automation capabilities to be truly useful. PowerShell provides extensive system management and automation capabilities, but there hasn't been an easy way to make these capabilities discoverable by AI assistants in a standardized manner. Commandry solves this problem by providing a bridge between PowerShell scripts and the Model Context Protocol (MCP), allowing AI assistants to discover, understand, and execute PowerShell-based tools.

### Description

Commandry is a .NET-based tool that:

- **Discovers PowerShell Scripts**: Automatically scans directories for `.ps1` files and converts them into various MCP capabilities.
- **Discovers PowerShell Functions**: Automatically scans directories for `.psm1` and `.psd1` files and converts functions exported by them into various MCP capabilities.
- **Extracts Metadata**: Parses PowerShell comment-based help and parameter attributes to generate rich descriptions of MCP capabilities.
- **Provides MCP Server**: Exposes discovered capabilities via MCP server that AI assistants can call.
- **Supports Multiple Transports**: Offers both STDIO and HTTP transport mechanisms for MCP communication.
- **Handles Type Conversion**: Automatically converts between MCP JSON parameters and PowerShell types.
- **Real-time Updates**: Monitors script and module directories for changes and updates tool availability dynamically.
- **Progress reporting**: Provides progress updates from long-running PowerShell scripts back to the MCP client via `Write-Progress` command.
- **Standardized Logging**: Sends logs written by `Write-{Level}` commands to the MCP client.

## MCP Support

### MCP Feature Support

| Feature      | Status | Description |
|--------------|--------|-------------|
| **Tools**    | ✅     | **Fully Supported** - Define MCP tools using PowerShell |
| **Resources**| ✅     | **Fully Supported** - Discover and provide MCP resources using PowerShell |
| **Prompts**  | ✅     | **Fully Supported** - Expose prompt and prompt templates using PowerShell |
| **Sampling** | ✅     | **Fully Supported** - Support LLM sampling requests in PowerShell tools |
| **Roots**    | ✅     | **Fully Supported** - Access workspace roots inside PowerShell tools |
| **Elicitation** | ✅  | **Fully Supported** - Allow PowerShell tools to interact with user |
| **Instructions** | ✅ | **Fully Supported** - Server instructions and agent guidance |

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

Usage:
  commandry-mcp-stdio [options]

Options:
  --scan-directory <scan-directory>                                           Directory to scan for PowerShell scripts and modules.
  --scan-module <scan-module>                                                 PowerShell module to scan for PowerShell functions.
  --log-verbosity <Alert|Critical|Debug|Emergency|Error|Info|Notice|Warning>  Log verbosity. [default: Info]
  --version                                                                   Show version information
  -?, -h, --help                                                              Show help and usage information
```

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
  Starts MCP server on `http://localhost:<port>`.

Usage:
  commandry-mcp-http [options]

Options:
  --scan-directory <scan-directory>                                           Directory to scan for PowerShell scripts and modules.
  --scan-module <scan-module>                                                 PowerShell module to scan for PowerShell functions.
  --port <port>                                                               Port to listen on. [default: 3001]
  --log-verbosity <Alert|Critical|Debug|Emergency|Error|Info|Notice|Warning>  Log verbosity. [default: Info]
  --version                                                                   Show version information
  -?, -h, --help                                                              Show help and usage information
```

### Writing MCP-Compatible PowerShell Scripts and Functions

When creating PowerShell scripts and functions use prompts:
- [mcptool.prompt.md](.github/prompts/mcptool.prompt.md) - for creating MCP tools