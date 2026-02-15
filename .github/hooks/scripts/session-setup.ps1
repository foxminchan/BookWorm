# BookWorm — Session start environment validation hook
# Validates that the development environment is properly configured.
$ErrorActionPreference = "Stop"

try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $source = $hookInput.source
    $cwd = $hookInput.cwd
    $timestamp = $hookInput.timestamp

    $logDir = Join-Path $cwd ".github/hooks/logs"
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }

    $sessionLog = Join-Path $logDir "session.log"
    $now = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    $logLines = @(
        "=== Session started ==="
        "  Source: $source"
        "  Time:   $now"
        "  CWD:    $cwd"
    )

    # Validate .NET SDK
    try {
        $sdkVersion = & dotnet --version 2>$null
        $logLines += "  .NET SDK: $sdkVersion"
    }
    catch {
        $logLines += "  WARNING: dotnet SDK not found"
    }

    # Validate dotnet tools
    try {
        $tools = & dotnet tool list 2>$null
        if ($tools -match "csharpier") {
            $logLines += "  CSharpier: installed"
        }
        else {
            $logLines += "  WARNING: CSharpier not found - run 'dotnet tool restore'"
        }
    }
    catch {
        $logLines += "  WARNING: Could not check dotnet tools"
    }

    # Check if solution builds are cached
    $artifactsPath = Join-Path $cwd "artifacts"
    if (Test-Path $artifactsPath) {
        $logLines += "  Build artifacts: present"
    }
    else {
        $logLines += "  Build artifacts: not found (initial build may be required)"
    }

    $logLines += ""
    $logLines | Add-Content -Path $sessionLog -Encoding UTF8
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
