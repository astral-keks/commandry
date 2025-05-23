# CommandR

A handy .NET tool that bridges PowerShell scripts with Model Context Protocol (MCP) servers, enabling seamless integration between AI assistants and PowerShell-based tools and automation.

## Summary

### Motivation

Modern AI assistants need access to powerful tools and automation capabilities to be truly useful. PowerShell provides extensive system management and automation capabilities, but there hasn't been an easy way to expose these capabilities to AI assistants in a standardized manner. CommandR solves this problem by providing a bridge between PowerShell scripts and the Model Context Protocol (MCP), allowing AI assistants to discover, understand, and execute PowerShell-based tools.

### Description

CommandR is a .NET-based tool that:

- **Discovers PowerShell Scripts**: Automatically scans directories for `.ps1` files and converts them into MCP tools
- **Extracts Metadata**: Parses PowerShell comment-based help and parameter attributes to generate rich tool descriptions
- **Provides MCP Server**: Exposes PowerShell scripts as MCP tools that AI assistants can discover and call
- **Supports Multiple Transports**: Offers both STDIO and HTTP transport mechanisms for MCP communication
- **Handles Type Conversion**: Automatically converts between MCP JSON parameters and PowerShell types
- **Real-time Updates**: Monitors script directories for changes and updates tool availability dynamically

## MCP Support

### MCP Feature Support

| Feature | Status | Description |
|---------|--------|-------------|
| **Tools** | ✅ | **Fully Supported** - Automatic enumeration of PowerShell scripts as MCP tools with dynamic schema generation, tool execution, result formatting, and comprehensive tool annotations |
| **Resources** | ❌ | Not yet implemented - File and data resource management |
| **Prompts** | ❌ | Not yet implemented - Template and prompt management |
| **Sampling** | ❌ | Not yet implemented - LLM sampling requests |
| **Roots** | ❌ | Not yet implemented - Workspace root management |

#### Tools Capability Details

CommandR's Tools implementation includes:

- **Tool Discovery**: Automatic enumeration of available PowerShell scripts as MCP tools
- **Dynamic Schema Generation**: Converts PowerShell parameter definitions to JSON Schema
- **Tool Execution**: Executes PowerShell scripts with parameters provided by MCP clients
- **Result Formatting**: Converts PowerShell output to MCP-compatible content types
- **Tool Annotations**: Supports MCP tool annotations for better AI assistant understanding:
  - `IdempotentHint`: Indicates if a tool can be safely called multiple times
  - `DestructiveHint`: Warns that a tool may make irreversible changes
  - `ReadOnlyHint`: Indicates a tool only reads data without making changes
  - `OpenWorldHint`: Suggests the tool works with open-world data
- **Change Monitoring**: Real-time updates when PowerShell scripts are added, removed, or modified
- **Dynamic Tool Registration**: Updates the available tool list without requiring server restart

### Supported MCP Transports

#### 1. STDIO Transport (`CommandR.Mcp.StdIO`)

Standard input/output transport for direct communication with MCP clients.

**Command Line Usage:**
```bash
# Basic usage
commandr-mcp-stdio --script-directory <path-to-scripts>

# Full help
commandr-mcp-stdio --help
```

**Help Output:**
```
Description:
  Starts MCP server with STDIO transport.

Options:
  --script-directory <script-directory> (REQUIRED)  Directory containing PowerShell scripts to be included.
  --version                                         Show version information
  -?, -h, --help                                    Show help and usage information
```

**Features:**
- Direct stdin/stdout communication
- Logging redirected to stderr to avoid interference with MCP protocol
- Suitable for integration with MCP clients that support process-based servers

#### 2. HTTP Transport (`CommandR.Mcp.Http`)

HTTP-based transport using Server-Sent Events (SSE) for web-based MCP communication.

**Command Line Usage:**
```bash
# Basic usage (default port 3001)
commandr-mcp-http --script-directory <path-to-scripts>

# Custom port
commandr-mcp-http --script-directory <path-to-scripts> --port 8080

# Full help
commandr-mcp-http --help
```

**Help Output:**
```
Description:
  Starts MCP server on `http://localhost:<port>/sse`.

Options:
  --script-directory <script-directory> (REQUIRED)  Directory containing PowerShell scripts to be included.
  --port <port>                                     Port to listen on. [default: 3001]
  --version                                         Show version information
  -?, -h, --help                                    Show help and usage information
```

**Features:**
- HTTP endpoint at `http://localhost:<port>/sse`
- Server-Sent Events for real-time communication

## PowerShell Integration

### Script Discovery and Execution

CommandR automatically discovers PowerShell scripts (`.ps1` files) in the specified directory and converts them into MCP tools. Each script becomes a tool with:

- **Tool Name**: Derived from the script filename (without `.ps1` extension)
- **Parameters**: Extracted from PowerShell parameter definitions
- **Metadata**: Parsed from comment-based help blocks

### Writing MCP-Compatible PowerShell Scripts

To create PowerShell scripts that work optimally with CommandR, follow these guidelines:

#### 1. Use Comment-Based Help

Include a comment-based help block at the beginning of your script to provide rich metadata:

```powershell
<#
.SYNOPSIS
Brief description of what the script does

.DESCRIPTION
Detailed description of the script's functionality and purpose

.PARAMETER ParamName
Description of the parameter

.NOTES
Additional metadata in key:value format:
IdempotentHint: true
ReadOnlyHint: false
DestructiveHint: false
#>
```

#### 2. Define Parameters Properly

Use proper PowerShell parameter syntax with attributes:

```powershell
param(
    [Parameter(Mandatory=$true, HelpMessage="Description of required parameter")]
    [string]$RequiredParam,
    
    [Parameter(Mandatory=$false, HelpMessage="Description of optional parameter")]
    [int]$OptionalParam = 42,
    
    [Parameter(HelpMessage="Boolean switch parameter")]
    [switch]$EnableFeature
)
```

#### 3. Parameter Type Support

CommandR supports automatic conversion between MCP JSON types and PowerShell types:

- **String**: `[string]` parameters
- **Integer**: `[int]`, `[long]`, `[short]` parameters  
- **Number**: `[float]`, `[double]`, `[decimal]` parameters
- **Boolean**: `[bool]` parameters and `[switch]` parameters
- **Array**: Array types and `List<T>` types
- **Object**: `[object]` and complex types

#### 4. Return Values

Scripts can return various types of data that will be converted to MCP content:

```powershell
# Return simple text
"Operation completed successfully"

# Return structured data
@{
    Status = "Success"
    Message = "File processed"
    Count = 42
}

# Return arrays
@("item1", "item2", "item3")
```

#### 5. Tool Annotations in Notes

Use the `.NOTES` section to specify MCP tool annotations:

```powershell
<#
.NOTES
IdempotentHint: true
ReadOnlyHint: true
DestructiveHint: false
OpenWorldHint: false
#>
```

These annotations help AI assistants understand how to use your tools safely and effectively.

#### 6. Example Script

Here's a complete example of a well-structured PowerShell script for CommandR:

```powershell
<#
.SYNOPSIS
Gets information about files in a directory

.DESCRIPTION
Retrieves detailed information about files in the specified directory,
including size, creation time, and other attributes.

.PARAMETER Path
The directory path to scan for files

.PARAMETER Filter
Optional file filter pattern (e.g., "*.txt")

.PARAMETER Recurse
Include subdirectories in the search

.NOTES
IdempotentHint: true
ReadOnlyHint: true
DestructiveHint: false
#>

param(
    [Parameter(Mandatory=$true, HelpMessage="Directory path to scan")]
    [string]$Path,
    
    [Parameter(Mandatory=$false, HelpMessage="File filter pattern")]
    [string]$Filter = "*",
    
    [Parameter(HelpMessage="Include subdirectories")]
    [switch]$Recurse
)

try {
    $files = Get-ChildItem -Path $Path -Filter $Filter -File -Recurse:$Recurse
    
    $result = @{
        Path = $Path
        Filter = $Filter
        FileCount = $files.Count
        Files = @($files | ForEach-Object {
            @{
                Name = $_.Name
                Size = $_.Length
                Created = $_.CreationTime
                Modified = $_.LastWriteTime
            }
        })
    }
    
    return $result
}
catch {
    throw "Error scanning directory: $($_.Exception.Message)"
}
```
