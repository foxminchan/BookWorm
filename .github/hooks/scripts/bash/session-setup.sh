#!/usr/bin/env bash
# BookWorm — Session start environment validation hook
# Validates that the development environment is properly configured.
set -e

INPUT=$(cat)
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
json_get() { printf '%s' "$INPUT" | python3 "$SCRIPT_DIR/hook-json.py" get "$@"; }
SOURCE=$(json_get source)
CWD=$(json_get cwd)
TIMESTAMP=$(json_get timestamp)

LOG_DIR="${CWD}/.github/hooks/logs"
mkdir -p "$LOG_DIR"

SESSION_LOG="${LOG_DIR}/session.log"

echo "=== Session started ===" >> "$SESSION_LOG"
echo "  Source: $SOURCE" >> "$SESSION_LOG"
echo "  Time:   $(date -d @$((TIMESTAMP / 1000)) 2>/dev/null || date)" >> "$SESSION_LOG"
echo "  CWD:    $CWD" >> "$SESSION_LOG"

# Validate .NET SDK
if [[ -x "$(command -v dotnet)" ]]; then
  SDK_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
  echo "  .NET SDK: $SDK_VERSION" >> "$SESSION_LOG"
else
  echo "  WARNING: dotnet SDK not found" >> "$SESSION_LOG"
fi

# Validate dotnet tools
if [[ -n "$(dotnet tool list 2>/dev/null | grep -o 'csharpier')" ]]; then
  echo "  CSharpier: installed" >> "$SESSION_LOG"
else
  echo "  WARNING: CSharpier not found — run 'dotnet tool restore'" >> "$SESSION_LOG"
fi

# Check if solution builds are cached
if [[ -d "${CWD}/artifacts" ]]; then
  echo "  Build artifacts: present" >> "$SESSION_LOG"
else
  echo "  Build artifacts: not found (initial build may be required)" >> "$SESSION_LOG"
fi

echo "" >> "$SESSION_LOG"
exit 0
