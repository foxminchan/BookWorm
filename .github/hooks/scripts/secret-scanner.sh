#!/bin/bash
# BookWorm — Secret scanner pre-tool hook
# Detects potential secrets, API keys, and tokens in commands and file content.
set -e

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.toolName')
TOOL_ARGS=$(echo "$INPUT" | jq -r '.toolArgs')

# Patterns that indicate potential secrets
SECRET_PATTERNS=(
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

check_for_secrets() {
  local content="$1"
  local context="$2"

  for pattern in "${SECRET_PATTERNS[@]}"; do
    if echo "$content" | grep -qEi "$pattern"; then
      jq -n --arg reason "Potential secret detected in $context. Hardcoded credentials, API keys, and tokens must not be committed. Use User Secrets or environment variables instead." \
        '{permissionDecision: "deny", permissionDecisionReason: $reason}'
      exit 0
    fi
  done
}

# Check bash commands for secrets
if [ "$TOOL_NAME" = "bash" ]; then
  COMMAND=$(echo "$TOOL_ARGS" | jq -r '.command // empty')
  check_for_secrets "$COMMAND" "bash command"
fi

# Check file edits/creates for embedded secrets
if [ "$TOOL_NAME" = "edit" ] || [ "$TOOL_NAME" = "create" ]; then
  CONTENT=$(echo "$TOOL_ARGS" | jq -r '.content // .newText // .new_string // empty')
  check_for_secrets "$CONTENT" "file content"
fi

# Allow if no secrets found
exit 0
