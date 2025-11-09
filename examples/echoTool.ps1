<#
.DESCRIPTION
  A tool that inspects context.
.ROLE
  MCP tool
#>
param(
  [Parameter(
    HelpMessage = "The label from the 2nd task in .vscode/tasks.json in the current workspace.",
    Mandatory = $true
  )]
  [string]$Value
)

return "Value $Value."
