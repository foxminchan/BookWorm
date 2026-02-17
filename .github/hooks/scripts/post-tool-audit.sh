#!/bin/bash
# BookWorm — Post-tool audit trail hook
# Logs all tool executions for traceability and debugging.
set -e

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.toolName')
TOOL_ARGS=$(echo "$INPUT" | jq -r '.toolArgs')
RESULT_TYPE=$(echo "$INPUT" | jq -r '.toolResult.resultType')
TIMESTAMP=$(echo "$INPUT" | jq -r '.timestamp')
CWD=$(echo "$INPUT" | jq -r '.cwd')

LOG_DIR="${CWD}/.github/hooks/logs"
mkdir -p "$LOG_DIR"

AUDIT_LOG="${LOG_DIR}/audit.jsonl"

# Write structured JSONL entry
jq -n -c \
  --arg ts "$TIMESTAMP" \
  --arg tool "$TOOL_NAME" \
  --arg result "$RESULT_TYPE" \
  --arg date "$(date -u +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date)" \
  '{timestamp: $ts, date: $date, tool: $tool, result: $result}' >> "$AUDIT_LOG"

# Track failure counts for the session
if [[ "$RESULT_TYPE" = "failure" ]]; then
  FAILURE_LOG="${LOG_DIR}/failures.log"
  RESULT_TEXT=$(echo "$INPUT" | jq -r '.toolResult.textResultForLlm // "no details"' | head -c 500)
  echo "$(date): FAILURE [$TOOL_NAME] $RESULT_TEXT" >> "$FAILURE_LOG"
fi

exit 0
