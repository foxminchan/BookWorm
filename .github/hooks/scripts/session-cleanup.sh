#!/bin/bash
# BookWorm — Session end cleanup hook
# Cleans up temporary resources and logs session summary.
set -e

INPUT=$(cat)
REASON=$(echo "$INPUT" | jq -r '.reason')
TIMESTAMP=$(echo "$INPUT" | jq -r '.timestamp')
CWD=$(echo "$INPUT" | jq -r '.cwd')

LOG_DIR="${CWD}/.github/hooks/logs"
mkdir -p "$LOG_DIR"

SESSION_LOG="${LOG_DIR}/session.log"
AUDIT_LOG="${LOG_DIR}/audit.jsonl"

echo "=== Session ended ===" >> "$SESSION_LOG"
echo "  Reason: $REASON" >> "$SESSION_LOG"
echo "  Time:   $(date -d @$((TIMESTAMP / 1000)) 2>/dev/null || date)" >> "$SESSION_LOG"

# Summarize tool usage from audit log
if [ -f "$AUDIT_LOG" ]; then
  TOTAL=$(wc -l < "$AUDIT_LOG" 2>/dev/null || echo "0")
  FAILURES=$(grep -c '"result":"failure"' "$AUDIT_LOG" 2>/dev/null || echo "0")
  echo "  Tools invoked: $TOTAL (failures: $FAILURES)" >> "$SESSION_LOG"
fi

echo "" >> "$SESSION_LOG"
exit 0
