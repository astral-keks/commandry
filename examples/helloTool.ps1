<#
.DESCRIPTION
  A tool that returns greeting.
.ROLE
  MCP tool
#>
param(
)

$letter = 'e'
$result = Invoke-McpSampling -Text "Count letters '$letter' in the current message. Respond with only the number."

return "There were $($result.Text) letters '$letter'."
