# BookWorm — Session end cleanup hook
# Cleans up temporary resources and logs session summary.
$ErrorActionPreference = 'Stop'

$RawInput = [Console]::In.ReadToEnd()
$Data = $RawInput | ConvertFrom-Json
$Reason = $Data.reason
$Timestamp = $Data.timestamp
$Cwd = $Data.cwd

$LogDir = Join-Path $Cwd '.github/hooks/logs'
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }

$SessionLog = Join-Path $LogDir 'session.log'
$AuditLog = Join-Path $LogDir 'audit.jsonl'

$Now = Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ'

Add-Content -Path $SessionLog -Value "=== Session ended ==="
Add-Content -Path $SessionLog -Value "  Reason: $Reason"
Add-Content -Path $SessionLog -Value "  Time:   $Now"

# Run formatting before session ends
try {
    if (Get-Command just -ErrorAction SilentlyContinue) {
        Add-Content -Path $SessionLog -Value "  Running 'just format'..."
        $FormatOutput = & just format 2>&1 | Out-String
        Add-Content -Path $SessionLog -Value $FormatOutput
    } else {
        Add-Content -Path $SessionLog -Value "  WARNING: 'just' not found - skipping format"
    }
} catch {
    Add-Content -Path $SessionLog -Value "  WARNING: 'just format' failed: $_"
}

# Summarize tool usage from audit log
if (Test-Path $AuditLog) {
    $Lines = Get-Content $AuditLog
    $Total = $Lines.Count
    $Failures = ($Lines | Where-Object { $_ -match '"result":"failure"' }).Count
    Add-Content -Path $SessionLog -Value "  Tools invoked: $Total (failures: $Failures)"
}

Add-Content -Path $SessionLog -Value ''
exit 0
