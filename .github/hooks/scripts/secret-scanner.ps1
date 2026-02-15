# BookWorm — Secret scanner pre-tool hook
# Detects potential secrets, API keys, and tokens in commands and file content.
$ErrorActionPreference = "Stop"

try {
    $hookInput = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $toolName = $hookInput.toolName
    $toolArgs = $hookInput.toolArgs | ConvertFrom-Json -ErrorAction SilentlyContinue

    # Patterns that indicate potential secrets
    $secretPatterns = @(
        # API keys and tokens
        "sk-[a-zA-Z0-9]{20,}"
        "ghp_[a-zA-Z0-9]{36}"
        "gho_[a-zA-Z0-9]{36}"
        "github_pat_[a-zA-Z0-9_]{82}"
        "AKIA[0-9A-Z]{16}"
        # Connection strings with passwords
        "Password=[^;]{8,}"
        "pwd=[^;]{8,}"
        # Bearer tokens
        "Bearer\s+[a-zA-Z0-9\-._~+/]+=*"
        # Generic secret patterns
        "-----BEGIN (RSA |EC |DSA )?PRIVATE KEY-----"
        "-----BEGIN CERTIFICATE-----"
    )

    function Test-ForSecrets {
        param(
            [string]$Content,
            [string]$Context
        )

        foreach ($pattern in $secretPatterns) {
            if ($Content -match $pattern) {
                @{
                    permissionDecision       = "deny"
                    permissionDecisionReason = "Potential secret detected in $Context. Hardcoded credentials, API keys, and tokens must not be committed. Use User Secrets or environment variables instead."
                } | ConvertTo-Json -Compress
                exit 0
            }
        }
    }

    # Check bash commands for secrets
    if ($toolName -eq "bash") {
        $command = $toolArgs.command
        if ($command) {
            Test-ForSecrets -Content $command -Context "bash command"
        }
    }

    # Check file edits/creates for embedded secrets
    if ($toolName -in @("edit", "create")) {
        $content = if ($toolArgs.content) { $toolArgs.content }
                   elseif ($toolArgs.newText) { $toolArgs.newText }
                   elseif ($toolArgs.new_string) { $toolArgs.new_string }
                   else { "" }

        if ($content) {
            Test-ForSecrets -Content $content -Context "file content"
        }
    }

    # Allow if no secrets found
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
