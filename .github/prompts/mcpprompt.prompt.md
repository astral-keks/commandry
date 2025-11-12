---
description: Defining PowerShell 7.0 MCP prompt
mode: agent
---

# User input
You must consider the following inputs which could be provided by user or agent:
- `prompt name`: The name of the MCP prompt to create or edit. If not provided suggest suitable name.
- `prompt form` (`script` or `function`): The form of the MCP prompt to create, either as a standalone script or as a function within a module. Use `script` by default if not provided.

# Instructions
You are creating a **PowerShell MCP prompt** in a form of script (`.ps1` files) or in a form of function (in `.psm1` modules) depending on the `prompt form`. Follow these guidelines:


## Scripts Guidelines

To create PowerShell scripts that work optimally with Commandry as MCP prompts, follow these guidelines:

#### 1. Create `.ps1` Script File Following Naming Conventions

Since Commandry derives the MCP prompt name directly from the PowerShell script filename (without the `.ps1` extension), follow these naming conventions to ensure compatibility:

**Naming Requirements:**
- **Start with a letter or underscore**: Prompt names cannot begin with numbers
- **Use only alphanumeric characters, hyphens, and underscores**: Avoid spaces and special characters
- **Keep names descriptive but concise**: 2-4 words that clearly indicate the prompt's purpose
- **Use action or context descriptors**: Consider names like `codeReview.ps1`, `bugAnalysis.ps1`, `generateDocs.ps1`

**Recommended Naming Patterns:**
- Use **camelCase** (preferred): `codeReview.ps1`, `bugAnalysis.ps1`, `generateDocs.ps1`
- Use **snake_case**: `code_review.ps1`, `bug_analysis.ps1`, `generate_docs.ps1`

#### 2. Use Comment-Based Help

Include a comment-based help block at the beginning of your script to provide rich metadata:

```powershell
<#
.DESCRIPTION (required)
This description enables AI models to comprehend the prompt's functionality and when it should be used. It must be clear, brief, and provide an accurate explanation of what kind of assistance or context this prompt provides to the AI model.

.ROLE (required)
MCP prompt

.SYNOPSIS (optional)
The title offers a more explanatory and human-readable label for the prompt compared to its programmatic identifier. It serves display functions and enables users to quickly grasp the prompt's purpose.
In contrast to the prompt name (which adheres to programming naming standards), the title may contain spaces, special symbols, and can be written in a more conversational, natural language format.
#>
```

#### 3. Define Parameters Properly

Prompts can have parameters that customize the generated messages. Use proper PowerShell parameter syntax:

```powershell
param(
    [Parameter(Mandatory=$true, HelpMessage="The code file to review")]
    [string]$FilePath,
    
    [Parameter(Mandatory=$false, HelpMessage="Focus area for the review")]
    [ValidateSet("security", "performance", "style", "all")]
    [string]$Focus = "all"
)
```

Prompts without parameters:

```powershell
param()
```

#### 4. Output Type Requirements

Scripts must define `OutputType` attributes to specify the structure of messages they return. MCP prompts must return either:

1. **PromptMessage objects** (from ModelContextProtocol.Protocol namespace)
2. **String content** (which will be automatically converted to assistant message)
3. **Other types** (which will be converted to content blocks in assistant messages)

```powershell
[OutputType([ModelContextProtocol.Protocol.PromptMessage])]
# OR
[OutputType([string])]
```

#### 5. Understanding MCP Prompts

MCP prompts are reusable templates or workflows that generate messages to help guide AI model interactions. They provide:
- **Context**: Background information about a task or domain
- **Instructions**: Specific guidance on how to approach a problem
- **Examples**: Sample inputs/outputs to demonstrate expected behavior
- **System messages**: Set the AI model's role and behavior
- **User messages**: Represent user input or questions

**When to use MCP Prompts:**
- Creating reusable conversation starters for common tasks
- Providing domain-specific context (e.g., project structure, coding standards)
- Generating multi-message workflows for complex operations
- Encapsulating best practices and patterns

#### 6. Return Values

Prompt scripts must return one or more messages. You can return messages in several ways:

**Simple approach - return string content:**
```powershell
# Returns a single assistant message with text content
"You are a code reviewer. Analyze the following code for security issues..."
```

**Multiple messages as strings:**
```powershell
# Returns multiple assistant messages
@(
    "You are an expert code reviewer.",
    "Focus on security vulnerabilities and best practices.",
    "Provide specific, actionable feedback."
)
```

**Advanced approach - return PromptMessage objects:**
```powershell
using namespace ModelContextProtocol.Protocol

# Return messages with specific roles
@(
    [PromptMessage]@{
        Role = [Role]::User
        Content = [TextContent]@{ Text = "Please review this code for security issues" }
    },
    [PromptMessage]@{
        Role = [Role]::Assistant
        Content = [TextContent]@{ Text = "I'll analyze the code focusing on common security vulnerabilities..." }
    }
)
```

**Message Roles:**
- **User**: Represents input from the user
- **Assistant**: Represents AI model responses or instructions to the model

#### 7. Example Script - Simple Context Prompt

Here's a complete example of a well-structured PowerShell script for a simple prompt:

```powershell
<#
.SYNOPSIS
  Code review assistant

.DESCRIPTION
  Provides context and instructions for conducting thorough code reviews with focus on best practices, security, and maintainability.

.ROLE
  MCP prompt
#>

[OutputType([string])]

param(
    [Parameter(Mandatory=$false, HelpMessage="Programming language")]
    [string]$Language = "any"
)

try {
    $instructions = @"
You are an expert code reviewer with deep knowledge of software engineering best practices.

When reviewing code, consider the following aspects:
1. **Security**: Look for vulnerabilities, injection risks, and insecure patterns
2. **Performance**: Identify inefficiencies and optimization opportunities
3. **Maintainability**: Check code clarity, documentation, and structure
4. **Best Practices**: Ensure adherence to $Language coding standards
5. **Testing**: Evaluate test coverage and quality

Provide specific, actionable feedback with code examples where appropriate.
Be constructive and educational in your comments.
"@

    return $instructions
}
catch {
    Write-Error "Error generating code review prompt: $($_.Exception.Message)"
    throw
}
```

#### 8. Example Script - Multi-Message Workflow

Here's an example that generates multiple messages for a conversation workflow:

```powershell
<#
.SYNOPSIS
  Bug analysis workflow

.DESCRIPTION
  Generates a structured conversation workflow for analyzing and debugging issues. Provides context, gathers information, and guides the debugging process.

.ROLE
  MCP prompt
#>

using namespace ModelContextProtocol.Protocol

[OutputType([PromptMessage])]

param(
    [Parameter(Mandatory=$true, HelpMessage="Description of the bug")]
    [string]$BugDescription,
    
    [Parameter(Mandatory=$false, HelpMessage="Error message if available")]
    [string]$ErrorMessage = ""
)

try {
    $messages = @()
    
    # System context
    $messages += [PromptMessage]@{
        Role = [Role]::Assistant
        Content = [TextContent]@{ 
            Text = @"
I am a debugging assistant specialized in root cause analysis. I will help you systematically investigate and resolve this issue.

My approach:
1. Gather information about the problem
2. Analyze symptoms and error messages
3. Form hypotheses about root causes
4. Suggest diagnostic steps
5. Recommend fixes with explanations
"@
        }
    }
    
    # User's problem statement
    $userMessage = "I'm encountering the following issue: $BugDescription"
    if ($ErrorMessage) {
        $userMessage += "`n`nError message: $ErrorMessage"
    }
    
    $messages += [PromptMessage]@{
        Role = [Role]::User
        Content = [TextContent]@{ Text = $userMessage }
    }
    
    # Initial analysis prompt
    $messages += [PromptMessage]@{
        Role = [Role]::Assistant
        Content = [TextContent]@{ 
            Text = @"
Let me analyze this issue. To provide the best assistance, I need to understand:

1. **When does this occur?** (Always, intermittently, under specific conditions)
2. **What changed recently?** (Code changes, configuration, dependencies)
3. **Environment details?** (OS, runtime version, deployment environment)
4. **Expected vs actual behavior?**

Please share any relevant code snippets, logs, or stack traces.
"@
        }
    }
    
    return $messages
}
catch {
    Write-Error "Error generating bug analysis prompt: $($_.Exception.Message)"
    throw
}
```

#### 9. Example Script - Dynamic Context from Project

Here's an example that generates context from the project structure:

```powershell
<#
.SYNOPSIS
  Project context provider

.DESCRIPTION
  Generates context about the current project structure, coding standards, and architecture to help the AI model provide more relevant assistance.

.ROLE
  MCP prompt
#>

using namespace ModelContextProtocol.Protocol

[OutputType([PromptMessage])]

param()

try {
    # Discover project structure
    $projectRoot = $PSScriptRoot
    $sourceFiles = Get-ChildItem -Path (Join-Path $projectRoot "src") -Recurse -File -ErrorAction SilentlyContinue
    $testFiles = Get-ChildItem -Path (Join-Path $projectRoot "tests") -Recurse -File -ErrorAction SilentlyContinue
    
    # Read coding standards if available
    $codingStandards = ""
    $standardsFile = Join-Path $projectRoot "CODING_STANDARDS.md"
    if (Test-Path $standardsFile) {
        $codingStandards = Get-Content -Path $standardsFile -Raw
    }
    
    # Generate context message
    $context = @"
# Project Context

## Project Structure
- Source files: $($sourceFiles.Count) files in /src
- Test files: $($testFiles.Count) files in /tests
- Primary languages: $(($sourceFiles | Group-Object Extension | Sort-Object Count -Descending | Select-Object -First 3 -ExpandProperty Name) -join ', ')

## Architecture
This is a $(if ($testFiles.Count -gt 0) { "well-tested" } else { "developing" }) project with organized source structure.

$(if ($codingStandards) { "## Coding Standards`n$codingStandards" } else { "" })

## Guidelines
When working with this project:
- Follow the established directory structure
- Maintain consistent naming conventions
- Write tests for new functionality
- Update documentation for significant changes
"@

    return [PromptMessage]@{
        Role = [Role]::Assistant
        Content = [TextContent]@{ Text = $context }
    }
}
catch {
    Write-Error "Error generating project context: $($_.Exception.Message)"
    throw
}
```

#### 10. Example Script - Interactive Prompt with Resources

Here's an example that incorporates resource references:

```powershell
<#
.SYNOPSIS
  Documentation writer assistant

.DESCRIPTION
  Generates a prompt for writing documentation with context from existing docs and code structure.

.ROLE
  MCP prompt
#>

using namespace ModelContextProtocol.Protocol

[OutputType([PromptMessage])]

param(
    [Parameter(Mandatory=$true, HelpMessage="Topic to document")]
    [string]$Topic,
    
    [Parameter(Mandatory=$false, HelpMessage="Documentation type")]
    [ValidateSet("API", "Tutorial", "Guide", "Reference")]
    [string]$Type = "Guide"
)

try {
    $messages = @()
    
    # Context setting
    $messages += [PromptMessage]@{
        Role = [Role]::Assistant
        Content = [TextContent]@{ 
            Text = @"
I am a technical documentation specialist. I will help you create clear, comprehensive $Type documentation for: $Topic

My documentation principles:
- **Clarity**: Use simple, precise language
- **Structure**: Organize with clear headings and sections
- **Examples**: Include practical code examples
- **Completeness**: Cover all important aspects
- **Accessibility**: Write for your target audience
"@
        }
    }
    
    # User request
    $messages += [PromptMessage]@{
        Role = [Role]::User
        Content = [TextContent]@{ 
            Text = "I need to create $Type documentation for $Topic. Please help me structure and write comprehensive documentation."
        }
    }
    
    # Template guidance
    $template = switch ($Type) {
        "API" { "# API Reference for $Topic`n`n## Overview`n`n## Endpoints/Methods`n`n## Parameters`n`n## Response Format`n`n## Examples`n`n## Error Handling" }
        "Tutorial" { "# $Topic Tutorial`n`n## Prerequisites`n`n## Step-by-Step Guide`n`n## Examples`n`n## Troubleshooting`n`n## Next Steps" }
        "Guide" { "# $Topic Guide`n`n## Introduction`n`n## Concepts`n`n## How-To`n`n## Best Practices`n`n## Common Issues" }
        "Reference" { "# $Topic Reference`n`n## Overview`n`n## Syntax`n`n## Options/Parameters`n`n## Examples`n`n## Related Topics" }
    }
    
    $messages += [PromptMessage]@{
        Role = [Role]::Assistant
        Content = [TextContent]@{ 
            Text = @"
I'll help you create this documentation. Here's a recommended structure:

$template

Let me know what specific aspects you'd like to cover, and I'll help you develop each section.
"@
        }
    }
    
    return $messages
}
catch {
    Write-Error "Error generating documentation prompt: $($_.Exception.Message)"
    throw
}
```

## Functions guidelines

The guidelines for writing MCP-compatible PowerShell functions for prompts are the same as for scripts. Ensure the function is exported from the module and includes the `.ROLE MCP prompt` in its comment-based help.

## Important Notes

- MCP prompts are invoked when users or AI models request them by name
- Prompts can accept parameters to customize the generated messages
- Return messages in the order they should appear in the conversation
- Use appropriate roles (User/Assistant) to structure the conversation flow
- Prompts are great for encapsulating domain knowledge and best practices
- Consider making prompts discoverable with clear descriptions
- Prompts can be dynamic - read files, query systems, or generate context based on current state
- Keep prompts focused on a specific task or workflow
- Test prompts to ensure they generate helpful, relevant context
