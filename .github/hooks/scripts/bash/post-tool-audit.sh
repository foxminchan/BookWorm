#!/usr/bin/env bash
# BookWorm — Post-tool audit trail hook
# Logs all tool executions for traceability and debugging.
set -e

INPUT=$(cat)
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
json_get() { printf '%s' "$INPUT" | python3 "$SCRIPT_DIR/hook-json.py" get "$@"; }
TOOL_NAME=$(json_get toolName)
RESULT_TYPE=$(json_get toolResult.resultType)

REPO_DIR="$(cd -- "$SCRIPT_DIR/../../../.." && pwd)"
LOG_DIR="${REPO_DIR}/.github/hooks/audit"
mkdir -p "$LOG_DIR"

AUDIT_LOG="${LOG_DIR}/audit.jsonl"

# Write structured JSONL entry
printf '%s\n' "$INPUT" | python3 "$SCRIPT_DIR/hook-json.py" audit >> "$AUDIT_LOG"

# Track failure counts for the session
if [[ "$RESULT_TYPE" = "failure" ]]; then
  FAILURE_LOG="${LOG_DIR}/failures.log"
  RESULT_TEXT=$(json_get toolResult.textResultForLlm)
  RESULT_TEXT=${RESULT_TEXT:-"no details"}
  RESULT_TEXT=${RESULT_TEXT:0:500}
  echo "$(date): FAILURE [$TOOL_NAME] $RESULT_TEXT" >> "$FAILURE_LOG"
fi

exit 0
