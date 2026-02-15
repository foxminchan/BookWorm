# BookWorm — Session end cleanup hook
# Cleans up temporary resources and logs session summary.
$ErrorActionPreference = "Stop"

try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $reason = $hookInput.reason
    $cwd = $hookInput.cwd

    $logDir = Join-Path $cwd ".github/hooks/logs"
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }

    $sessionLog = Join-Path $logDir "session.log"
    $auditLog = Join-Path $logDir "audit.jsonl"
    $now = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    $logLines = @(
        "=== Session ended ==="
        "  Reason: $reason"
        "  Time:   $now"
    )

    # Summarize tool usage from audit log
    if (Test-Path $auditLog) {
        $total = (Get-Content $auditLog | Measure-Object -Line).Lines
        $failures = (Select-String -Path $auditLog -Pattern '"result":"failure"' -SimpleMatch | Measure-Object).Count
        $logLines += "  Tools invoked: $total (failures: $failures)"
    }

    $logLines += ""
    $logLines | Add-Content -Path $sessionLog -Encoding UTF8
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
