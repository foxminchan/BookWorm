# BookWorm — Secret scanner pre-tool hook
# Detects potential secrets, API keys, and tokens in commands and file content.
$ErrorActionPreference = 'Stop'

$RawInput = [Console]::In.ReadToEnd()
$Data = $RawInput | ConvertFrom-Json
$ToolName = $Data.toolName
$ToolArgs = $Data.toolArgs

# Patterns that indicate potential secrets
$SecretPatterns = @(
    'sk-[a-zA-Z0-9]{20,}'
    'ghp_[a-zA-Z0-9]{36}'
    'gho_[a-zA-Z0-9]{36}'
    'github_pat_[a-zA-Z0-9_]{82}'
    'AKIA[0-9A-Z]{16}'
    'Password=[^;]{8,}'
    'pwd=[^;]{8,}'
    'Bearer\s+[a-zA-Z0-9\-._~+/]+=*'
    '-----BEGIN (RSA |EC |DSA )?PRIVATE KEY-----'
    '-----BEGIN CERTIFICATE-----'
)

function Test-ForSecrets {
    param(
        [string]$Content,
        [string]$Context
    )

    foreach ($pattern in $SecretPatterns) {
        if ($Content -match $pattern) {
            @{
                permissionDecision       = 'deny'
                permissionDecisionReason = "Potential secret detected in $Context. Hardcoded credentials, API keys, and tokens must not be committed. Use User Secrets or environment variables instead."
            } | ConvertTo-Json -Compress
            exit 0
        }
    }
}

# Check bash commands for secrets
if ($ToolName -eq 'bash') {
    $Command = if ($ToolArgs.command) { $ToolArgs.command } else { '' }
    Test-ForSecrets -Content $Command -Context 'bash command'
}

# Check file edits/creates for embedded secrets
if ($ToolName -eq 'edit' -or $ToolName -eq 'create') {
    $Content = if ($ToolArgs.content) { $ToolArgs.content }
               elseif ($ToolArgs.newText) { $ToolArgs.newText }
               elseif ($ToolArgs.new_string) { $ToolArgs.new_string }
               else { '' }
    Test-ForSecrets -Content $Content -Context 'file content'
}

# Allow if no secrets found
exit 0
