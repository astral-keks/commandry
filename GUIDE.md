
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
- Use **camelCase** (preferred): `getFileInfo.ps1`, `deployWebsite.ps1`, `backupDatabase.ps1`
- Use **kebab-case**: `get-file-info.ps1`, `deploy-website.ps1`, `backup-database.ps1`
- Use **snake_case**: `get_file_info.ps1`, `deploy_website.ps1`, `backup_database.ps1`

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

Use proper PowerShell parameter syntax with `Parameter` and type attributes:

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

Scripts must define one or more `OutputType` attributes to specify the structure and format of the data they return. This is crucial for ensuring that the tool's output can be correctly interpreted and utilized by the AI model. E.g.:
```powershell
  [OutputType([string])]
  [OutputType([SomeType])]
```
Avoid using `PSCustomObject` in `OutputType` as it does not provide sufficient type information for MCP clients. Instead, define and use specific classes.
Scripts can return various types of data that will be automatically converted to MCP content:

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
- IdempotentHint - indicates whether calling the tool repeatedly with the same arguments will have no additional effect on its environment.
- DestructiveHint - indicates whether the tool may perform destructive updates to its environment.
- OpenWorldHint - indicates whether the tool may interact with an unpredictable or dynamic set of entities(like web search). If false, the tool's domain of interaction is closed and well-defined (like memory access).
- ReadOnlyHint - indicates whether this tool does not modify its environment.

These annotations help AI assistants understand how to use your tools safely and effectively.

#### 7. Additional cmdlets

Here is the list of cmdlets that are provided to the MCP tool by the MCP server to support additional MCP features:
- `Get-McpRoot` - Returns the collection of available MCP roots from the server.
- `Invoke-McpElicitation` - Sends an elicitation request and yields the server's response payload.
- `Invoke-McpSampling` - Requests message sampling to generate assistant or user replies based on input text or messages.

Refer to following materials for more details on these cmdlets:
- [GetMcpRootCmdlet.cs](src/ModelContextProtocol.Pwsh/GetRootCmdlet.cs)
- [InvokeMcpElicitationCmdlet.cs](src/ModelContextProtocol.Pwsh/InvokeElicitationCmdlet.cs)
- [InvokeMcpSamplingCmdlet.cs](src/ModelContextProtocol.Pwsh/InvokeSamplingCmdlet.cs)
- [modelcontextprotocol/csharp-sdk] (https://github.com/modelcontextprotocol/csharp-sdk)


#### 7. Example Script

Here's a complete example of a well-structured PowerShell script for Commandry:

```powershell
<#
.SYNOPSIS
  Gets information about files in a directory

.DESCRIPTION
  Retrieves detailed information about files in the specified directory, including size, creation time, and other attributes.

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
