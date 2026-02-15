#!/bin/bash
# BookWorm — Pre-tool use guard hook
# Blocks modifications to protected files and dangerous commands.
set -e

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.toolName')
TOOL_ARGS=$(echo "$INPUT" | jq -r '.toolArgs')

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

if [ "$TOOL_NAME" = "edit" ] || [ "$TOOL_NAME" = "create" ]; then
  FILE_PATH=$(echo "$TOOL_ARGS" | jq -r '.path // .filePath // empty')

  for protected in "${PROTECTED_FILES[@]}"; do
    if echo "$FILE_PATH" | grep -qF "$protected"; then
      jq -n --arg reason "Modifying '$protected' is not allowed without explicit user approval. This file controls foundational build/SDK configuration." \
        '{permissionDecision: "deny", permissionDecisionReason: $reason}'
      exit 0
    fi
  done
fi

# --- Dangerous bash commands ---
if [ "$TOOL_NAME" = "bash" ]; then
  COMMAND=$(echo "$TOOL_ARGS" | jq -r '.command // empty')

  # Block destructive system-level commands
  if echo "$COMMAND" | grep -qE "rm\s+-rf\s+/|mkfs|format\s+[A-Z]:|DROP\s+(TABLE|DATABASE)|TRUNCATE\s+TABLE"; then
    jq -n '{permissionDecision: "deny", permissionDecisionReason: "Destructive system command detected. This operation is blocked by project policy."}'
    exit 0
  fi

  # Block attempts to modify global tool config
  if echo "$COMMAND" | grep -qE "dotnet\s+workload\s+install\s+aspire"; then
    jq -n '{permissionDecision: "deny", permissionDecisionReason: "The Aspire workload is obsolete and must not be installed. Use Aspire NuGet packages instead."}'
    exit 0
  fi

  # Block modifications to global.json via shell
  if echo "$COMMAND" | grep -qE "(sed|awk|echo|cat|tee|>).*global\.json"; then
    jq -n '{permissionDecision: "deny", permissionDecisionReason: "Modifying global.json via shell is not permitted."}'
    exit 0
  fi
fi

# Allow everything else
exit 0
