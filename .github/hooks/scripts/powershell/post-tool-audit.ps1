# BookWorm — Post-tool audit trail hook
# Logs all tool executions for traceability and debugging.
$ErrorActionPreference = 'Stop'

$Input = $input | Out-String
$Data = $Input | ConvertFrom-Json
$ToolName = $Data.toolName
$ToolArgs = $Data.toolArgs
$ResultType = $Data.toolResult.resultType
$Timestamp = $Data.timestamp
$Cwd = $Data.cwd

$LogDir = Join-Path $Cwd '.github/hooks/logs'
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }

$AuditLog = Join-Path $LogDir 'audit.jsonl'

$Now = Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ'

# Write structured JSONL entry
$Entry = @{
    timestamp = $Timestamp
    date      = $Now
    tool      = $ToolName
    result    = $ResultType
} | ConvertTo-Json -Compress

Add-Content -Path $AuditLog -Value $Entry

# Track failure counts for the session
if ($ResultType -eq 'failure') {
    $FailureLog = Join-Path $LogDir 'failures.log'
    $ResultText = if ($Data.toolResult.textResultForLlm) {
        $Data.toolResult.textResultForLlm.Substring(0, [Math]::Min(500, $Data.toolResult.textResultForLlm.Length))
    } else { 'no details' }
    Add-Content -Path $FailureLog -Value "$($Now): FAILURE [$ToolName] $ResultText"
}

exit 0
