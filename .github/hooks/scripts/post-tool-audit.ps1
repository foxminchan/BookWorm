# BookWorm — Post-tool audit trail hook
# Logs all tool executions for traceability and debugging.
$ErrorActionPreference = "Stop"

try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $toolName = $hookInput.toolName
    $resultType = $hookInput.toolResult.resultType
    $timestamp = $hookInput.timestamp
    $cwd = $hookInput.cwd

    $logDir = Join-Path $cwd ".github/hooks/logs"
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }

    $auditLog = Join-Path $logDir "audit.jsonl"
    $now = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"

    # Write structured JSONL entry
    $entry = @{
        timestamp = $timestamp
        date      = $now
        tool      = $toolName
        result    = $resultType
    } | ConvertTo-Json -Compress

    $entry | Add-Content -Path $auditLog -Encoding UTF8

    # Track failure counts for the session
    if ($resultType -eq "failure") {
        $failureLog = Join-Path $logDir "failures.log"
        $resultText = if ($hookInput.toolResult.textResultForLlm) {
            $hookInput.toolResult.textResultForLlm.Substring(0, [Math]::Min(500, $hookInput.toolResult.textResultForLlm.Length))
        }
        else { "no details" }

        "${now}: FAILURE [$toolName] $resultText" | Add-Content -Path $failureLog -Encoding UTF8
    }

    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
