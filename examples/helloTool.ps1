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
$message = [ModelContextProtocol.Protocol.SamplingMessage]::new()

return "There were $($result.Content.Text) letters '$letter'."
