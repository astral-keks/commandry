---
description: Defining PowerShell 7.0 MCP resource list
mode: agent
---

# User input
You must consider the following inputs which could be provided by user or agent:
- `resource name`: The name of the MCP resource list to create or edit. If not provided suggest suitable name.
- `resource form` (`script` or `function`): The form of the MCP resource list to create, either as a standalone script or as a function within a module. Use `script` by default if not provided.

# Instructions
You are creating a **PowerShell MCP resource list** in a form of script (`.ps1` files) or in a form of function (in `.psm1` modules) depending on the `resource form`. Follow these guidelines:


## Scripts Guidelines

To create PowerShell scripts that work optimally with Commandry as MCP resource lists, follow these guidelines:

#### 1. Create `.ps1` Script File Following Naming Conventions

Since Commandry derives the MCP resource name directly from the PowerShell script filename (without the `.ps1` extension), follow these naming conventions to ensure compatibility:

**Naming Requirements:**
- **Start with a letter or underscore**: Resource names cannot begin with numbers
- **Use only alphanumeric characters, hyphens, and underscores**: Avoid spaces and special characters
- **Keep names descriptive but concise**: 2-4 words that clearly indicate the resource collection's purpose
- **Use plural forms**: Since this is a list, consider names like `listFiles.ps1`, `getProjects.ps1`, `listDatabases.ps1`

**Recommended Naming Patterns:**
- Use **camelCase** (preferred): `listFiles.ps1`, `getProjects.ps1`, `listDatabases.ps1`
- Use **snake_case**: `list_files.ps1`, `get_projects.ps1`, `list_databases.ps1`

#### 2. Use Comment-Based Help

Include a comment-based help block at the beginning of your script to provide rich metadata:

```powershell
<#
.DESCRIPTION (required)
This description enables AI models to comprehend the resource list's functionality and what resources it provides. It must be clear, brief, and provide an accurate explanation of the resource collection's purpose and what kind of resources are available.

.ROLE (required)
MCP resource list

.SYNOPSIS (optional)
The title offers a more explanatory and human-readable label for the resource list compared to its programmatic identifier. It serves display functions and enables users to quickly grasp the resource collection's purpose.
In contrast to the resource name (which adheres to programming naming standards), the title may contain spaces, special symbols, and can be written in a more conversational, natural language format.
#>
```

#### 3. Define Parameters Properly

Resource lists typically don't require parameters:

```powershell
param()
```

#### 4. Output Type Requirements

Scripts must define `OutputType` attributes to specify the structure of resources they return. Resource lists must return either:

1. **Resource objects** (from ModelContextProtocol.Protocol namespace)
2. **String URIs** (which will be automatically converted to Resource objects)

```powershell
[OutputType([ModelContextProtocol.Protocol.Resource])]
# OR
[OutputType([string])]
```

Avoid using `PSCustomObject` in `OutputType` as it does not provide sufficient type information for MCP clients.

#### 5. Return Values

Resource list scripts must return a collection of resources. Each resource must have:
- **Uri**: A unique identifier for the resource (required)
- **Name**: A human-readable name (optional, defaults to Uri if not provided)
- **Description**: A description of the resource (optional)
- **MimeType**: The MIME type of the resource content (optional)

You can return resources in two ways:

**Simple approach - return URIs as strings:**
```powershell
# Return array of URI strings
@(
    "file:///project/README.md",
    "file:///project/src/main.ps1",
    "file:///project/docs/guide.md"
)
```

**Advanced approach - return Resource objects:**
```powershell
# Import the MCP protocol types
using namespace ModelContextProtocol.Protocol

# Return array of Resource objects
@(
    [Resource]@{
        Uri = "file:///project/README.md"
        Name = "Project README"
        Description = "Main project documentation"
        MimeType = "text/markdown"
    },
    [Resource]@{
        Uri = "file:///project/src/main.ps1"
        Name = "Main Script"
        Description = "Primary application script"
        MimeType = "text/plain"
    }
)
```

#### 6. Example Script - Simple URI List

Here's a complete example of a well-structured PowerShell script for a simple resource list:

```powershell
<#
.SYNOPSIS
  Lists available project documentation files

.DESCRIPTION
  Returns a list of documentation files available in the project, including README, guides, and API documentation.

.ROLE
  MCP resource list
#>

[OutputType([string])]

param()

try {
    $docsPath = Join-Path $PSScriptRoot "docs"
    
    if (-not (Test-Path $docsPath)) {
        Write-Warning "Documentation directory not found: $docsPath"
        return @()
    }
    
    $files = Get-ChildItem -Path $docsPath -Filter $Filter -File -Recurse
    
    $resources = $files | ForEach-Object {
        $relativePath = $_.FullName.Substring((Resolve-Path $PSScriptRoot).Path.Length + 1)
        "file:///$($relativePath -replace '\\', '/')"
    }
    
    return $resources
}
catch {
    Write-Error "Error listing documentation files: $($_.Exception.Message)"
    return @()
}
```

#### 7. Example Script - Advanced Resource Objects

Here's an example with full Resource objects:

```powershell
<#
.SYNOPSIS
  Lists available database connections

.DESCRIPTION
  Returns a list of configured database connections with metadata about each database.

.ROLE
  MCP resource list
#>

using namespace ModelContextProtocol.Protocol

[OutputType([Resource])]

param()

try {
    # Simulate fetching database configurations
    $databases = @(
        @{ Name = "primary"; Server = "db1.example.com"; Type = "PostgreSQL" },
        @{ Name = "analytics"; Server = "db2.example.com"; Type = "PostgreSQL" },
        @{ Name = "cache"; Server = "redis.example.com"; Type = "Redis" }
    )
    
    $resources = $databases | ForEach-Object {
        [Resource]@{
            Uri = "db://$($_.Name)"
            Name = "$($_.Type) - $($_.Name)"
            Description = "Database connection to $($_.Server)"
            MimeType = "application/json"
        }
    }
    
    return $resources
}
catch {
    Write-Error "Error listing databases: $($_.Exception.Message)"
    return @()
}
```

## Functions guidelines

The guidelines for writing MCP-compatible PowerShell functions for resource lists are the same as for scripts. Ensure the function is exported from the module and includes the `.ROLE MCP resource list` in its comment-based help.

## Important Notes

- Resource list scripts are executed automatically when an MCP client requests the list of available resources
- They should be fast and efficient, as they may be called frequently
- Resource URIs should be stable and consistent across invocations
- Consider implementing caching if resource discovery is expensive
- Resource URIs are used by resource content scripts to retrieve the actual content (see MCP resource content documentation)
