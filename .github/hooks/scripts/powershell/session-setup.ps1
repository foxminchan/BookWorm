# BookWorm — Session start environment validation hook
# Validates that the development environment is properly configured.
$ErrorActionPreference = 'Stop'

$RawInput = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($RawInput)) { exit 0 }
try { $Data = $RawInput | ConvertFrom-Json } catch { exit 0 }
$Source = $Data.source
$Cwd = $Data.cwd
$Timestamp = $Data.timestamp

$LogDir = Join-Path $Cwd '.github/hooks/logs'
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }

$SessionLog = Join-Path $LogDir 'session.log'

$Now = Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ'

Add-Content -Path $SessionLog -Value "=== Session started ==="
Add-Content -Path $SessionLog -Value "  Source: $Source"
Add-Content -Path $SessionLog -Value "  Time:   $Now"
Add-Content -Path $SessionLog -Value "  CWD:    $Cwd"

# Validate .NET SDK
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $SdkVersion = & dotnet --version 2>$null
    if (-not $SdkVersion) { $SdkVersion = 'unknown' }
    Add-Content -Path $SessionLog -Value "  .NET SDK: $SdkVersion"
}
else {
    Add-Content -Path $SessionLog -Value "  WARNING: dotnet SDK not found"
}

# Validate dotnet tools
$ToolList = & dotnet tool list 2>$null
if ($ToolList -match 'csharpier') {
    Add-Content -Path $SessionLog -Value "  CSharpier: installed"
}
else {
    Add-Content -Path $SessionLog -Value "  WARNING: CSharpier not found - run 'dotnet tool restore'"
}

# Check if solution builds are cached
$ArtifactsDir = Join-Path $Cwd 'artifacts'
if (Test-Path $ArtifactsDir) {
    Add-Content -Path $SessionLog -Value "  Build artifacts: present"
}
else {
    Add-Content -Path $SessionLog -Value "  Build artifacts: not found (initial build may be required)"
}

Add-Content -Path $SessionLog -Value ''
exit 0
