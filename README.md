# Commandry

A handy .NET tool that bridges PowerShell scripts with Model Context Protocol (MCP) servers, enabling seamless integration between AI assistants and PowerShell-based tools and automation.

## Summary

### Motivation

Modern AI agents need access to powerful tools and automation capabilities to be truly useful. PowerShell provides extensive system management and automation capabilities, but there hasn't been an easy way to expose these capabilities to AI assistants in a standardized manner. Commandry solves this problem by providing a bridge between PowerShell scripts and the Model Context Protocol (MCP), allowing AI assistants to discover, understand, and execute PowerShell-based tools.

### Description

Commandry is a .NET-based tool that:

- **Discovers PowerShell Scripts**: Automatically scans directories for `.ps1` files and converts them into MCP tools
- **Supports PowerShell Functions**: Automatically scans directories for `.ps,1` files and converts functions exported by them into MCP tools
- **Extracts Metadata**: Parses PowerShell comment-based help and parameter attributes to generate rich tool descriptions
- **Provides MCP Server**: Exposes PowerShell scripts as MCP tools that AI assistants can discover and call
- **Supports Multiple Transports**: Offers both STDIO and HTTP transport mechanisms for MCP communication
- **Handles Type Conversion**: Automatically converts between MCP JSON parameters and PowerShell types
- **Real-time Updates**: Monitors script directories for changes and updates tool availability dynamically

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

## PowerShell Integration

### Script and Function Discovery and Execution

Commandry automatically discovers PowerShell scripts (`.ps1` files) in the specified directory and converts them into MCP tools. It also discovers PowerShell functions defined within `.psm1` files and converts them into MCP tools. Each script and function becomes a tool with:

- **Tool Name**: Derived from the script filename (without `.ps1` extension) or function name or from the Name property defined in .NOTES section of the help block
- **Parameters**: Extracted from PowerShell parameter definitions
- **Metadata**: Parsed from comment-based help blocks

### Writing MCP-Compatible PowerShell Scripts

To create PowerShell scripts that work optimally with Commandry, follow these guidelines:

#### 1. Create `.ps1` Script File Following Naming Conventions

Since Commandry derives the MCP tool name directly from the PowerShell script filename (without the `.ps1` extension), follow these naming conventions to ensure compatibility:

**Naming Requirements:**
- **Start with a letter or underscore**: Tool names cannot begin with numbers
- **Use only alphanumeric characters, hyphens, and underscores**: Avoid spaces and special characters
- **Keep names descriptive but concise**: 2-4 words that clearly indicate the tool's purpose

**Recommended Naming Patterns:**
- Use **kebab-case**: `get-file-info.ps1`, `deploy-website.ps1`, `backup-database.ps1`
- Use **snake_case**: `get_file_info.ps1`, `deploy_website.ps1`, `backup_database.ps1`
- Use **camelCase**: `getFileInfo.ps1`, `deployWebsite.ps1`, `backupDatabase.ps1`

#### 2. Use Comment-Based Help

Include a comment-based help block at the beginning of your script to provide rich metadata:

```powershell
<#
.DESCRIPTION (required)
This description enables AI models to comprehend the tool's functionality and appropriate usage scenarios. It must be clear, brief, and provide an accurate explanation of the tool's intended purpose and capabilities. AI models are generally provided with this description to assist them in deciding whether and how to utilize the tool in response to user queries.

.ROLE (required)
MCP tool

.SYNOPSIS (optional)
The title offers a more explanatory and human-readable label for the tool compared to its programmatic identifier. It serves display functions and enables users to quickly grasp the tool's function and purpose.
In contrast to the tool name (which adheres to programming naming standards), the title may contain spaces, special symbols, and can be written in a more conversational, natural language format.

.NOTES (optional)
IdempotentHint: true
ReadOnlyHint: false
DestructiveHint: false
#>
```

#### 3. Define Parameters Properly

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

#### 4. Parameter Type Support

Commandry supports automatic conversion between MCP JSON types and PowerShell types:

- **String**: `[string]` parameters
- **Integer**: `[int]`, `[long]`, `[short]` parameters  
- **Number**: `[float]`, `[double]`, `[decimal]` parameters
- **Boolean**: `[bool]` parameters and `[switch]` parameters
- **Array**: Array types and `List<T>` types
- **Object**: `[object]` and complex types

#### 5. Return Values

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

#### 6. Tool Annotations in Notes

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

#### 7. Example Script

Here's a complete example of a well-structured PowerShell script for Commandry:

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

### Writing MCP-Compatible PowerShell Functions

The guidelines for writing MCP-compatible PowerShell functions are the same as for scripts.
