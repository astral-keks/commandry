---
description: Defining PowerShell 7.0 MCP resource content
mode: agent
---

# User input
You must consider the following inputs which could be provided by user or agent:
- `resource name`: The name of the MCP resource content provider to create or edit. If not provided suggest suitable name.
- `resource form` (`script` or `function`): The form of the MCP resource content to create, either as a standalone script or as a function within a module. Use `script` by default if not provided.
- `uri pattern`: The URI pattern that this resource content provider handles (e.g., `file:///`, `db://`, `config://`)

# Instructions
You are creating a **PowerShell MCP resource content provider** in a form of script (`.ps1` files) or in a form of function (in `.psm1` modules) depending on the `resource form`. Follow these guidelines:


## Scripts Guidelines

To create PowerShell scripts that work optimally with Commandry as MCP resource content providers, follow these guidelines:

#### 1. Create `.ps1` Script File Following Naming Conventions

Since Commandry derives the MCP resource content provider name directly from the PowerShell script filename (without the `.ps1` extension), follow these naming conventions to ensure compatibility:

**Naming Requirements:**
- **Start with a letter or underscore**: Resource content provider names cannot begin with numbers
- **Use only alphanumeric characters, hyphens, and underscores**: Avoid spaces and special characters
- **Keep names descriptive but concise**: 2-4 words that clearly indicate what content is being provided
- **Use action verbs**: Consider names like `readFile.ps1`, `getDbContent.ps1`, `fetchConfig.ps1`

**Recommended Naming Patterns:**
- Use **camelCase** (preferred): `readFile.ps1`, `getDbContent.ps1`, `fetchConfig.ps1`
- Use **snake_case**: `read_file.ps1`, `get_db_content.ps1`, `fetch_config.ps1`

#### 2. Use Comment-Based Help

Include a comment-based help block at the beginning of your script to provide rich metadata:

```powershell
<#
.DESCRIPTION (required)
This description enables AI models to comprehend the resource content provider's functionality and what content it retrieves. It must be clear, brief, and provide an accurate explanation of what kind of content is returned for the given URI pattern.

.ROLE (required)
MCP resource content

.LINK (required)
The URI pattern prefix that this resource content provider handles (e.g., "file:///", "db://", "config://")
This is used by the MCP server to route resource read requests to the appropriate content provider.

.SYNOPSIS (optional)
The title offers a more explanatory and human-readable label for the resource content provider compared to its programmatic identifier.

.NOTES (optional)
ReadOnlyHint: true
#>
```

#### 3. Define Required Uri Parameter

Resource content providers **must** have a `Uri` parameter that receives the resource URI to read:

```powershell
param(
    [Parameter(Mandatory=$true, HelpMessage="The URI of the resource to read")]
    [string]$Uri
)
```

#### 4. Output Type Requirements

Scripts must define `OutputType` attributes to specify the structure of content they return. Resource content providers must return either:

1. **ResourceContents objects** (from ModelContextProtocol.Protocol namespace)
2. **String content** (which will be automatically converted to TextResourceContents)

```powershell
[OutputType([ModelContextProtocol.Protocol.ResourceContents])]
# OR
[OutputType([string])]
```

Avoid using `PSCustomObject` in `OutputType` as it does not provide sufficient type information for MCP clients.

#### 5. Return Values

Resource content scripts must return the content of the requested resource. The content can be returned in two ways:

**Simple approach - return content as string:**
```powershell
# Return the content as a string (will be converted to TextResourceContents)
$content = Get-Content -Path $filePath -Raw
return $content
```

**Advanced approach - return ResourceContents object:**
```powershell
# Import the MCP protocol types
using namespace ModelContextProtocol.Protocol

# Return a TextResourceContents object
$content = Get-Content -Path $filePath -Raw
[TextResourceContents]@{
    Text = $content
    MimeType = "text/plain"
}

# Or return a BlobResourceContents object for binary data
[BlobResourceContents]@{
    Blob = [Convert]::ToBase64String($binaryData)
    MimeType = "application/octet-stream"
}
```

#### 6. URI Matching with .LINK

The `.LINK` metadata in the comment-based help specifies the URI prefix that this content provider handles. The MCP server uses this to route resource read requests:

```powershell
<#
.LINK
file:///
#>
```

When a client requests to read a resource with URI `file:///project/README.md`, this script will be invoked if its `.LINK` is `file:///`.

**Important:** 
- The URI parameter will receive the **full URI** (e.g., `file:///project/README.md`)
- Your script must parse the URI to extract the relevant path or identifier
- Multiple content providers can exist for different URI patterns

Resource content providers are typically read-only operations that retrieve but don't modify content.

#### 7. Additional cmdlets

Here is the list of cmdlets that are provided to the MCP resource by the MCP server to support additional MCP features:
- `Get-McpRoot` - Returns the collection of available MCP roots from the server.
- `Invoke-McpElicitation` - Sends an elicitation request and yields the server's response payload.
- `Invoke-McpSampling` - Requests message sampling to generate assistant or user replies based on input text or messages.

Refer to following materials for more details on these cmdlets:
- [GetMcpRootCmdlet.cs](src/ModelContextProtocol.Pwsh/GetRootCmdlet.cs)
- [InvokeMcpElicitationCmdlet.cs](src/ModelContextProtocol.Pwsh/InvokeElicitationCmdlet.cs)
- [InvokeMcpSamplingCmdlet.cs](src/ModelContextProtocol.Pwsh/InvokeSamplingCmdlet.cs)
- [modelcontextprotocol/csharp-sdk](https://github.com/modelcontextprotocol/csharp-sdk)

#### 8. Example Script - Simple File Content Provider

Here's a complete example of a well-structured PowerShell script for reading file contents:

```powershell
<#
.SYNOPSIS
  Reads file content from the project

.DESCRIPTION
  Returns the content of a file specified by a file:/// URI. Supports text and binary files.

.ROLE
  MCP resource content

.LINK
  file:///
#>

[OutputType([string])]

param(
    [Parameter(Mandatory=$true, HelpMessage="The file URI to read (e.g., file:///path/to/file.txt)")]
    [string]$Uri
)

try {
    # Parse the URI to extract the file path
    if ($Uri -notmatch '^file:///(.+)$') {
        throw "Invalid file URI format: $Uri"
    }
    
    $relativePath = $matches[1] -replace '/', '\'
    $fullPath = Join-Path $PSScriptRoot $relativePath
    
    if (-not (Test-Path $fullPath -PathType Leaf)) {
        throw "File not found: $fullPath"
    }
    
    # Read and return the file content
    $content = Get-Content -Path $fullPath -Raw -ErrorAction Stop
    
    return $content
}
catch {
    Write-Error "Error reading file from URI '$Uri': $($_.Exception.Message)"
    throw
}
```

#### 19. Example Script - Advanced Database Content Provider

Here's an example with full ResourceContents object for database content:

```powershell
<#
.SYNOPSIS
  Retrieves database schema information

.DESCRIPTION
  Returns schema information for a database specified by a db:// URI. The content is returned as JSON.

.ROLE
  MCP resource content

.LINK
  db://
#>

using namespace ModelContextProtocol.Protocol

[OutputType([ResourceContents])]

param(
    [Parameter(Mandatory=$true, HelpMessage="The database URI to read (e.g., db://database-name)")]
    [string]$Uri
)

try {
    # Parse the URI to extract the database name
    if ($Uri -notmatch '^db://([^/]+)/?(.*)$') {
        throw "Invalid database URI format: $Uri"
    }
    
    $databaseName = $matches[1]
    $objectPath = $matches[2]
    
    # Simulate fetching database schema (replace with actual database query)
    $schemaInfo = @{
        Database = $databaseName
        ObjectPath = $objectPath
        Schema = @{
            Tables = @("users", "orders", "products")
            Views = @("active_users", "order_summary")
        }
        Timestamp = (Get-Date).ToString("o")
    }
    
    # Convert to JSON
    $jsonContent = $schemaInfo | ConvertTo-Json -Depth 10
    
    # Return as TextResourceContents with JSON mime type
    return [TextResourceContents]@{
        Text = $jsonContent
        MimeType = "application/json"
    }
}
catch {
    Write-Error "Error reading database content from URI '$Uri': $($_.Exception.Message)"
    throw
}
```

#### 10. Example Script - Binary Content Provider

Here's an example that returns binary content:

```powershell
<#
.SYNOPSIS
  Retrieves image resources

.DESCRIPTION
  Returns image content for resources specified by an image:// URI as base64-encoded blob.

.ROLE
  MCP resource content

.LINK
  image://
#>

using namespace ModelContextProtocol.Protocol

[OutputType([ResourceContents])]

param(
    [Parameter(Mandatory=$true, HelpMessage="The image URI to read (e.g., image://logo.png)")]
    [string]$Uri
)

try {
    # Parse the URI
    if ($Uri -notmatch '^image://(.+)$') {
        throw "Invalid image URI format: $Uri"
    }
    
    $imageName = $matches[1]
    $imagePath = Join-Path $PSScriptRoot "assets" $imageName
    
    if (-not (Test-Path $imagePath -PathType Leaf)) {
        throw "Image not found: $imagePath"
    }
    
    # Read binary content
    $binaryData = [System.IO.File]::ReadAllBytes($imagePath)
    $base64Data = [Convert]::ToBase64String($binaryData)
    
    # Determine MIME type from extension
    $mimeType = switch ([System.IO.Path]::GetExtension($imagePath).ToLower()) {
        ".png" { "image/png" }
        ".jpg" { "image/jpeg" }
        ".jpeg" { "image/jpeg" }
        ".gif" { "image/gif" }
        default { "application/octet-stream" }
    }
    
    # Return as BlobResourceContents
    return [BlobResourceContents]@{
        Blob = $base64Data
        MimeType = $mimeType
    }
}
catch {
    Write-Error "Error reading image from URI '$Uri': $($_.Exception.Message)"
    throw
}
```

## Functions guidelines

The guidelines for writing MCP-compatible PowerShell functions for resource content are the same as for scripts. Ensure the function is exported from the module and includes the `.ROLE MCP resource content` and `.LINK <uri-pattern>` in its comment-based help.

## Important Notes

- Resource content scripts are executed when an MCP client requests to read a specific resource URI
- The `.LINK` metadata determines which URIs this script handles - it must match the prefix of the requested URI
- The `Uri` parameter is mandatory and receives the complete URI
- Content providers should validate URIs and handle errors gracefully
- Consider security implications when reading files or accessing resources - validate paths and implement access controls
- For large files, consider streaming or chunking the content
- The URI parameter will always start with the pattern specified in `.LINK`
- Multiple content providers can coexist for different URI patterns (e.g., `file:///`, `db://`, `config://`)
