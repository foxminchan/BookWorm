#!/usr/bin/env bash
# BookWorm — Secret scanner pre-tool hook
# Detects potential secrets, API keys, and tokens in commands and file content.
set -e

INPUT=$(cat)
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
json_get() { printf '%s' "$INPUT" | python3 "$SCRIPT_DIR/hook-json.py" get "$@"; }
json_deny() { local input="$1"; python3 "$SCRIPT_DIR/hook-json.py" deny "$input"; }
TOOL_NAME=$(json_get toolName)
TOOL_ARGS=$(json_get toolArgs)

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
      json_deny "Potential secret detected in $context. Hardcoded credentials, API keys, and tokens must not be committed. Use User Secrets or environment variables instead."
      exit 0
    fi
  done

  return 0
}

# Check bash commands for secrets
if [[ "$TOOL_NAME" = "bash" ]]; then
  COMMAND=$(printf '%s' "$TOOL_ARGS" | python3 "$SCRIPT_DIR/hook-json.py" get command)
  check_for_secrets "$COMMAND" "bash command"
fi

# Check file edits/creates for embedded secrets
if [[ "$TOOL_NAME" = "edit" ]] || [[ "$TOOL_NAME" = "create" ]]; then
  CONTENT=$(printf '%s' "$TOOL_ARGS" | python3 "$SCRIPT_DIR/hook-json.py" get content newText new_string)
  check_for_secrets "$CONTENT" "file content"
fi

# Allow if no secrets found
exit 0
