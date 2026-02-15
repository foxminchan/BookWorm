# BookWorm — Pre-tool use guard hook
# Blocks modifications to protected files and dangerous commands.
$ErrorActionPreference = "Stop"

try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $toolName = $hookInput.toolName
    $toolArgs = $hookInput.toolArgs | ConvertFrom-Json -ErrorAction SilentlyContinue

    # --- Protected files: deny edits to foundational config ---
    $protectedFiles = @(
        "global.json"
        "NuGet.config"
        "nuget.config"
        "LICENSE"
        ".editorconfig"
        "Directory.Build.props"
        "Directory.Packages.props"
        "Versions.props"
        "BookWorm.sln.DotSettings"
        ".github/workflows/copilot-setup-steps.yml"
    )

    if ($toolName -in @("edit", "create")) {
        $filePath = if ($toolArgs.path) { $toolArgs.path } elseif ($toolArgs.filePath) { $toolArgs.filePath } else { "" }

        foreach ($protected in $protectedFiles) {
            if ($filePath -like "*$protected*") {
                @{
                    permissionDecision       = "deny"
                    permissionDecisionReason = "Modifying '$protected' is not allowed without explicit user approval. This file controls foundational build/SDK configuration."
                } | ConvertTo-Json -Compress
                exit 0
            }
        }
    }

    # --- Dangerous bash/shell commands ---
    if ($toolName -eq "bash") {
        $command = $toolArgs.command

        # Block destructive system-level commands
        if ($command -match "rm\s+-rf\s+/|mkfs|format\s+[A-Z]:|DROP\s+(TABLE|DATABASE)|TRUNCATE\s+TABLE") {
            @{
                permissionDecision       = "deny"
                permissionDecisionReason = "Destructive system command detected. This operation is blocked by project policy."
            } | ConvertTo-Json -Compress
            exit 0
        }

        # Block attempts to install obsolete Aspire workload
        if ($command -match "dotnet\s+workload\s+install\s+aspire") {
            @{
                permissionDecision       = "deny"
                permissionDecisionReason = "The Aspire workload is obsolete and must not be installed. Use Aspire NuGet packages instead."
            } | ConvertTo-Json -Compress
            exit 0
        }

        # Block modifications to global.json via shell
        if ($command -match "(Set-Content|Out-File|Add-Content|sed|awk|>).*global\.json") {
            @{
                permissionDecision       = "deny"
                permissionDecisionReason = "Modifying global.json via shell is not permitted."
            } | ConvertTo-Json -Compress
            exit 0
        }
    }

    # Allow everything else
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
