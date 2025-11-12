<#
.DESCRIPTION
  Recursively searches for files in directory matching provided prompt.

.ROLE
  MCP tool

.NOTES
  IdempotentHint: true
  ReadOnlyHint: true
  DestructiveHint: false
  OpenWorldHint: false
#>
param(
    [Parameter(
        Mandatory = $true,
        HelpMessage = "Path to the directory to search recursively"
    )]
    [string]$Directory,
    
    [Parameter(
        Mandatory = $true,
        HelpMessage = "Glob pattern to filter files (e.g., '*.cs', '**/*.txt', 'src/**/*.json')"
    )]
    [string]$Filter,
    
    [Parameter(
        Mandatory = $true,
        HelpMessage = "Prompt describing what content to extract from each file"
    )]
    [string]$Prompt
)

# Validate directory exists
if (-not (Test-Path -Path $Directory -PathType Container)) {
    throw "Directory not found: $Directory"
}

# Resolve to absolute path
$Directory = Resolve-Path -Path $Directory

# Get all files matching the filter recursively
Write-Progress -Activity "File Search" -Status "Searching for files matching filter: $Filter"
$files = Get-ChildItem -Path $Directory -Filter $Filter -File -Recurse -ErrorAction Stop

if ($files.Count -eq 0) {
    throw "No files found matching filter '$Filter' in directory '$Directory'"
}

Write-Progress -Activity "File Search" -Status "Found $($files.Count) files. Processing..."

$results = @()
$i = 0

foreach ($file in $files) {
    $i++
    Write-Progress -Activity "File Search" -Status "Processing file $i of $($files.Count): $($file.Name)"
    
    try {
        # Read file content
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
        
        # Create the full prompt for sampling
        $fullPrompt = @"
You are a code analysis assistant. Your task is to carefully read the provided file content and extract only the specific information requested by the user.

Guidelines:
- Return ONLY the relevant snippets or information that matches the extraction criteria
- If multiple snippets match, return all of them
- Preserve the original formatting and structure of extracted content
- If nothing matches the criteria, respond with exactly: "NO_MATCH"
- Do not include explanations, commentary, or any text outside the extracted snippets
- Be precise and accurate in your extraction

USER REQUEST:
$Prompt

FILE CONTENT:
$content
"@
        
        # Invoke AI sampling to extract relevant content
        $samplingResult = Invoke-McpSampling -Text $fullPrompt -Role User
        
        if ($null -eq $samplingResult -or $null -eq $samplingResult.Content) {
            throw "Sampling failed for file: $($file.FullName)"
        }
        
        # Extract the text response
        $extractedContent = $samplingResult.Content.Text
        
        # Skip if no match found
        if ($extractedContent -eq "NO_MATCH") {
            continue
        }
        
        # Create result object
        $result = "$($file.FullName)`n$extractedContent"
        
        $results += $result
    }
    catch {
        Write-Progress -Activity "File Search" -Completed
        throw "Error processing file '$($file.FullName)': $_"
    }
}

Write-Progress -Activity "File Search" -Completed

# Return all results
return $results
