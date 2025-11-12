---
description: Defining PowerShell 7.0 MCP resource template list
mode: agent
---

# User input
You must consider the following inputs which could be provided by user or agent:
- `resource template name`: The name of the MCP resource template list to create or edit. If not provided suggest suitable name.
- `resource form` (`script` or `function`): The form of the MCP resource template list to create, either as a standalone script or as a function within a module. Use `script` by default if not provided.

# Instructions
You are creating a **PowerShell MCP resource template list** in a form of script (`.ps1` files) or in a form of function (in `.psm1` modules) depending on the `resource form`. Follow these guidelines:


## Scripts Guidelines

To create PowerShell scripts that work optimally with Commandry as MCP resource template lists, follow these guidelines:

#### 1. Create `.ps1` Script File Following Naming Conventions

Since Commandry derives the MCP resource template name directly from the PowerShell script filename (without the `.ps1` extension), follow these naming conventions to ensure compatibility:

**Naming Requirements:**
- **Start with a letter or underscore**: Resource template names cannot begin with numbers
- **Use only alphanumeric characters, hyphens, and underscores**: Avoid spaces and special characters
- **Keep names descriptive but concise**: 2-4 words that clearly indicate the resource template collection's purpose
- **Use plural forms**: Since this is a list, consider names like `listFileTemplates.ps1`, `getDbTemplates.ps1`, `listApiTemplates.ps1`

**Recommended Naming Patterns:**
- Use **camelCase** (preferred): `listFileTemplates.ps1`, `getDbTemplates.ps1`, `listApiTemplates.ps1`
- Use **snake_case**: `list_file_templates.ps1`, `get_db_templates.ps1`, `list_api_templates.ps1`

#### 2. Use Comment-Based Help

Include a comment-based help block at the beginning of your script to provide rich metadata:

```powershell
<#
.DESCRIPTION (required)
This description enables AI models to comprehend the resource template list's functionality and what resource templates it provides. It must be clear, brief, and provide an accurate explanation of the resource template collection's purpose and what kind of parameterized resources are available.

.ROLE (required)
MCP resource template list

.SYNOPSIS (optional)
The title offers a more explanatory and human-readable label for the resource template list compared to its programmatic identifier. It serves display functions and enables users to quickly grasp the resource template collection's purpose.
In contrast to the resource template name (which adheres to programming naming standards), the title may contain spaces, special symbols, and can be written in a more conversational, natural language format.
#>
```

#### 3. Define Parameters Properly

Resource template lists typically don't require parameters:

```powershell
param()
```

#### 4. Output Type Requirements

Scripts must define `OutputType` attributes to specify the structure of resource templates they return. Resource template lists must return either:

1. **ResourceTemplate objects** (from ModelContextProtocol.Protocol namespace)
2. **String URI templates** (which will be automatically converted to ResourceTemplate objects)

```powershell
[OutputType([ModelContextProtocol.Protocol.ResourceTemplate])]
# OR
[OutputType([string])]
```

Avoid using `PSCustomObject` in `OutputType` as it does not provide sufficient type information for MCP clients.

#### 5. Understanding Resource Templates

Resource templates are URI patterns that contain placeholders for parameters. They allow clients to dynamically construct resource URIs by filling in the template parameters.

**Template URI Format:**
- Use RFC 6570 URI template syntax
- Variables are enclosed in curly braces: `{variable}`
- Example: `file:///{path}`, `db://{database}/{table}`, `api://users/{userId}/posts/{postId}`

**When to use Resource Templates vs Resources:**
- **Resources** (from resource lists): Static, enumerable resources with fixed URIs
- **Resource Templates**: Dynamic resources where URIs are constructed from parameters (e.g., user profiles, database tables, API endpoints with IDs)

#### 6. Return Values

Resource template list scripts must return a collection of resource templates. Each template must have:
- **UriTemplate**: A URI template string following RFC 6570 format (required)
- **Name**: A human-readable name (optional, defaults to UriTemplate if not provided)
- **Description**: A description of what resources this template provides (optional)
- **MimeType**: The MIME type of the resource content (optional)

You can return resource templates in two ways:

**Simple approach - return URI templates as strings:**
```powershell
# Return array of URI template strings
@(
    "file:///{path}",
    "db://{database}/{table}",
    "api://users/{userId}"
)
```

**Advanced approach - return ResourceTemplate objects:**
```powershell
# Import the MCP protocol types
using namespace ModelContextProtocol.Protocol

# Return array of ResourceTemplate objects
@(
    [ResourceTemplate]@{
        UriTemplate = "file:///{path}"
        Name = "File Access"
        Description = "Access any file by path"
        MimeType = "text/plain"
    },
    [ResourceTemplate]@{
        UriTemplate = "db://{database}/{table}"
        Name = "Database Table"
        Description = "Access database table schema and data"
        MimeType = "application/json"
    },
    [ResourceTemplate]@{
        UriTemplate = "api://users/{userId}"
        Name = "User Profile"
        Description = "Access user profile information"
        MimeType = "application/json"
    }
)
```

Resource template lists are typically read-only operations that don't modify the environment.

#### 7. Example Script - Simple URI Template List

Here's a complete example of a well-structured PowerShell script for a simple resource template list:

```powershell
<#
.SYNOPSIS
  Lists available file path templates

.DESCRIPTION
  Returns URI templates for accessing files and directories in the project. Clients can use these templates to construct URIs for specific paths.

.ROLE
  MCP resource template list
#>

[OutputType([string])]

param()

try {
    # Return URI templates for file access
    $templates = @(
        "file:///{path}",
        "file:///{directory}/{filename}",
        "file:///docs/{section}/{page}.md"
    )
    
    return $templates
}
catch {
    Write-Error "Error listing file templates: $($_.Exception.Message)"
    return @()
}
```

#### 8. Example Script - Advanced ResourceTemplate Objects

Here's an example with full ResourceTemplate objects:

```powershell
<#
.SYNOPSIS
  Lists available API endpoint templates

.DESCRIPTION
  Returns URI templates for accessing various API endpoints. Each template includes parameter placeholders that clients can fill in to construct specific resource URIs.

.ROLE
  MCP resource template list
#>

using namespace ModelContextProtocol.Protocol

[OutputType([ResourceTemplate])]

param()

try {
    # Define available API endpoint templates
    $templates = @(
        [ResourceTemplate]@{
            UriTemplate = "api://users/{userId}"
            Name = "User Profile"
            Description = "Access user profile information by user ID"
            MimeType = "application/json"
        },
        [ResourceTemplate]@{
            UriTemplate = "api://users/{userId}/posts/{postId}"
            Name = "User Post"
            Description = "Access a specific post by a user"
            MimeType = "application/json"
        },
        [ResourceTemplate]@{
            UriTemplate = "api://projects/{projectId}/issues"
            Name = "Project Issues"
            Description = "Access all issues for a specific project"
            MimeType = "application/json"
        },
        [ResourceTemplate]@{
            UriTemplate = "api://search?q={query}&type={resourceType}"
            Name = "Search API"
            Description = "Search for resources by query and type"
            MimeType = "application/json"
        }
    )
    
    return $templates
}
catch {
    Write-Error "Error listing API templates: $($_.Exception.Message)"
    return @()
}
```

#### 9. Example Script - Database Templates

Here's an example for database resource templates:

```powershell
<#
.SYNOPSIS
  Lists available database resource templates

.DESCRIPTION
  Returns URI templates for accessing database resources including schemas, tables, and queries.

.ROLE
  MCP resource template list
#>

using namespace ModelContextProtocol.Protocol

[OutputType([ResourceTemplate])]

param()

try {
    # Get list of available databases (could be from configuration)
    $databases = @("primary", "analytics", "cache")
    
    # Create templates
    $templates = @(
        [ResourceTemplate]@{
            UriTemplate = "db://{database}/schema"
            Name = "Database Schema"
            Description = "Access complete schema for a database. Available databases: $($databases -join ', ')"
            MimeType = "application/json"
        },
        [ResourceTemplate]@{
            UriTemplate = "db://{database}/tables/{table}"
            Name = "Database Table"
            Description = "Access table schema and sample data"
            MimeType = "application/json"
        },
        [ResourceTemplate]@{
            UriTemplate = "db://{database}/query/{queryName}"
            Name = "Saved Query"
            Description = "Execute a saved query by name"
            MimeType = "application/json"
        }
    )
    
    return $templates
}
catch {
    Write-Error "Error listing database templates: $($_.Exception.Message)"
    return @()
}
```

#### 10. Example Script - Configuration Templates

Here's an example for configuration resource templates with dynamic discovery:

```powershell
<#
.SYNOPSIS
  Lists available configuration templates

.DESCRIPTION
  Returns URI templates for accessing application configuration values. Templates are discovered dynamically from configuration sources.

.ROLE
  MCP resource template list
#>

using namespace ModelContextProtocol.Protocol

[OutputType([ResourceTemplate])]

param()

try {
    # Discover configuration sections
    $configRoot = Join-Path $PSScriptRoot "config"
    $configSections = @()
    
    if (Test-Path $configRoot) {
        $configSections = Get-ChildItem -Path $configRoot -Directory | Select-Object -ExpandProperty Name
    }
    
    # Build templates
    $templates = @(
        [ResourceTemplate]@{
            UriTemplate = "config://{section}/{key}"
            Name = "Configuration Value"
            Description = "Access configuration value by section and key. Available sections: $($configSections -join ', ')"
            MimeType = "application/json"
        },
        [ResourceTemplate]@{
            UriTemplate = "config://{section}"
            Name = "Configuration Section"
            Description = "Access all configuration values in a section"
            MimeType = "application/json"
        },
        [ResourceTemplate]@{
            UriTemplate = "config://env/{environmentName}/{key}"
            Name = "Environment Configuration"
            Description = "Access environment-specific configuration values"
            MimeType = "application/json"
        }
    )
    
    return $templates
}
catch {
    Write-Error "Error listing configuration templates: $($_.Exception.Message)"
    return @()
}
```

## Functions guidelines

The guidelines for writing MCP-compatible PowerShell functions for resource template lists are the same as for scripts. Ensure the function is exported from the module and includes the `.ROLE MCP resource template list` in its comment-based help.

## Important Notes

- Resource template list scripts are executed automatically when an MCP client requests the list of available resource templates
- They should be fast and efficient, as they may be called frequently
- URI templates must follow RFC 6570 format with variables in `{curly braces}`
- Templates are used by clients to construct actual resource URIs by substituting parameter values
- Resource templates work in conjunction with resource content providers - the content provider must handle URIs matching the template pattern
- Consider documenting available parameter values in the Description field (e.g., "Available databases: primary, analytics")
- Resource templates enable dynamic resource discovery without enumerating all possible resources upfront
- Templates are ideal for large or infinite resource spaces (e.g., user IDs, file paths, timestamps)
