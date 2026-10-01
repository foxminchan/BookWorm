#!/usr/bin/env bash
# BookWorm — Pre-tool use guard hook
# Blocks modifications to protected files and dangerous commands.
set -e

INPUT=$(cat)
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
json_get() { printf '%s' "$INPUT" | python3 "$SCRIPT_DIR/hook-json.py" get "$@"; }
json_deny() { python3 "$SCRIPT_DIR/hook-json.py" deny "$1"; }
TOOL_NAME=$(json_get toolName)
TOOL_ARGS=$(json_get toolArgs)

# --- Protected files: deny edits to foundational config ---
PROTECTED_FILES=(
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

if [[ "$TOOL_NAME" = "edit" ]] || [[ "$TOOL_NAME" = "create" ]]; then
  FILE_PATH=$(printf '%s' "$TOOL_ARGS" | python3 "$SCRIPT_DIR/hook-json.py" get path filePath)

  for protected in "${PROTECTED_FILES[@]}"; do
    if echo "$FILE_PATH" | grep -qF "$protected"; then
      json_deny "Modifying '$protected' is not allowed without explicit user approval. This file controls foundational build/SDK configuration."
      exit 0
    fi
  done
fi

# --- Dangerous bash commands ---
if [[ "$TOOL_NAME" = "bash" ]]; then
  COMMAND=$(printf '%s' "$TOOL_ARGS" | python3 "$SCRIPT_DIR/hook-json.py" get command)

  # Block destructive system-level commands
  if echo "$COMMAND" | grep -qE "rm\s+-rf\s+/|mkfs|format\s+[A-Z]:|DROP\s+(TABLE|DATABASE)|TRUNCATE\s+TABLE"; then
    json_deny "Destructive system command detected. This operation is blocked by project policy."
    exit 0
  fi

  # Block attempts to modify global tool config
  if echo "$COMMAND" | grep -qE "dotnet\s+workload\s+install\s+aspire"; then
    json_deny "The Aspire workload is obsolete and must not be installed. Use Aspire NuGet packages instead."
    exit 0
  fi

  # Block modifications to global.json via shell
  if echo "$COMMAND" | grep -qE "(sed|awk|echo|cat|tee|>).*global\.json"; then
    json_deny "Modifying global.json via shell is not permitted."
    exit 0
  fi

  # Block force push and bypassing safety checks
  if echo "$COMMAND" | grep -qE "git\s+push\s+.*--force|git\s+push\s+-f\b|git\s+reset\s+--hard|git\s+.*--no-verify"; then
    json_deny "Force push, hard reset, and --no-verify are blocked by project policy. These operations are destructive or bypass safety checks."
    exit 0
  fi
fi

# Allow everything else
exit 0
