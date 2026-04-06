# BookWorm — Post-tool audit trail hook
# Logs all tool executions for traceability and debugging.
$ErrorActionPreference = 'Stop'

$RawInput = [Console]::In.ReadToEnd()
$Data = $RawInput | ConvertFrom-Json
$ToolName = $Data.toolName
$ResultType = $Data.toolResult.resultType
$Timestamp = $Data.timestamp
$Cwd = $Data.cwd

$LogDir = Join-Path $Cwd '.github/hooks/logs'
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }

$AuditLog = Join-Path $LogDir 'audit.jsonl'

$Now = Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ'

function Write-LogLine {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Line
    )
    $maxAttempts = 3
    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        try {
            [System.IO.File]::AppendAllText(
                $Path,
                $Line + [Environment]::NewLine,
                [System.Text.Encoding]::UTF8
            )
            return
        } catch {
            if ($attempt -eq $maxAttempts) { throw }
            Start-Sleep -Milliseconds (25 * $attempt)
        }
    }
}

# Write structured JSONL entry
$Entry = @{
    timestamp = $Timestamp
    date      = $Now
    tool      = $ToolName
    result    = $ResultType
} | ConvertTo-Json -Compress

Write-LogLine -Path $AuditLog -Line $Entry

# Track failure counts for the session
if ($ResultType -eq 'failure') {
    $FailureLog = Join-Path $LogDir 'failures.log'
    $ResultText = if ($Data.toolResult.textResultForLlm) {
        $Data.toolResult.textResultForLlm.Substring(0, [Math]::Min(500, $Data.toolResult.textResultForLlm.Length))
    } else { 'no details' }
    Write-LogLine -Path $FailureLog -Line "$($Now): FAILURE [$ToolName] $ResultText"
}

exit 0
