# BookWorm — Pre-tool use guard hook
# Blocks modifications to protected files and dangerous commands.
$ErrorActionPreference = 'Stop'

$RawInput = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($RawInput)) { exit 0 }
try { $Data = $RawInput | ConvertFrom-Json } catch { exit 0 }
$ToolName = $Data.toolName
$ToolArgs = $Data.toolArgs

# --- Protected files: deny edits to foundational config ---
$ProtectedFiles = @(
    'global.json'
    'NuGet.config'
    'nuget.config'
    'LICENSE'
    '.editorconfig'
    'Directory.Build.props'
    'Directory.Packages.props'
    'Versions.props'
    'BookWorm.sln.DotSettings'
    '.github/workflows/copilot-setup-steps.yml'
)

if ($ToolName -eq 'edit' -or $ToolName -eq 'create') {
    $FilePath = if ($ToolArgs.path) { $ToolArgs.path } elseif ($ToolArgs.filePath) { $ToolArgs.filePath } else { '' }

    foreach ($protected in $ProtectedFiles) {
        if ($FilePath -like "*$protected*") {
            @{
                permissionDecision       = 'deny'
                permissionDecisionReason = "Modifying '$protected' is not allowed without explicit user approval. This file controls foundational build/SDK configuration."
            } | ConvertTo-Json -Compress
            exit 0
        }
    }
}

# --- Dangerous commands ---
if ($ToolName -eq 'bash') {
    $Command = if ($ToolArgs.command) { $ToolArgs.command } else { '' }

    # Block destructive system-level commands
    if ($Command -match 'rm\s+-rf\s+/|mkfs|format\s+[A-Z]:|DROP\s+(TABLE|DATABASE)|TRUNCATE\s+TABLE') {
        @{
            permissionDecision       = 'deny'
            permissionDecisionReason = 'Destructive system command detected. This operation is blocked by project policy.'
        } | ConvertTo-Json -Compress
        exit 0
    }

    # Block attempts to install obsolete Aspire workload
    if ($Command -match 'dotnet\s+workload\s+install\s+aspire') {
        @{
            permissionDecision       = 'deny'
            permissionDecisionReason = 'The Aspire workload is obsolete and must not be installed. Use Aspire NuGet packages instead.'
        } | ConvertTo-Json -Compress
        exit 0
    }

    # Block modifications to global.json via shell
    if ($Command -match '(sed|awk|echo|cat|tee|>).*global\.json') {
        @{
            permissionDecision       = 'deny'
            permissionDecisionReason = 'Modifying global.json via shell is not permitted.'
        } | ConvertTo-Json -Compress
        exit 0
    }

    # Block force push and bypassing safety checks
    if ($Command -match 'git\s+push\s+.*--force|git\s+push\s+-f\b|git\s+reset\s+--hard|git\s+.*--no-verify') {
        @{
            permissionDecision       = 'deny'
            permissionDecisionReason = 'Force push, hard reset, and --no-verify are blocked by project policy. These operations are destructive or bypass safety checks.'
        } | ConvertTo-Json -Compress
        exit 0
    }
}

# Allow everything else
exit 0
