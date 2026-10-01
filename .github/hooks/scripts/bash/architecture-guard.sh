#!/usr/bin/env bash
# BookWorm — Architecture boundary guard pre-tool hook
# Enforces microservice boundaries by preventing cross-service internal references.
set -e

INPUT=$(cat)
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
json_get() { printf '%s' "$INPUT" | python3 "$SCRIPT_DIR/hook-json.py" get "$@"; }
json_deny() { python3 "$SCRIPT_DIR/hook-json.py" deny "$1"; }
TOOL_NAME=$(json_get toolName)
TOOL_ARGS=$(json_get toolArgs)

# Only check file creation/edits in service directories
if [[ "$TOOL_NAME" != "edit" ]] && [[ "$TOOL_NAME" != "create" ]]; then
  exit 0
fi

FILE_PATH=$(printf '%s' "$TOOL_ARGS" | python3 "$SCRIPT_DIR/hook-json.py" get path filePath)
CONTENT=$(printf '%s' "$TOOL_ARGS" | python3 "$SCRIPT_DIR/hook-json.py" get content newText new_string)

# Service names in the project
SERVICES=("Catalog" "Basket" "Ordering" "Rating" "Chat" "Finance" "Notification" "Scheduler" "McpTools")

# Determine which service this file belongs to
CURRENT_SERVICE=""
for svc in "${SERVICES[@]}"; do
  if echo "$FILE_PATH" | grep -q "Services/$svc/"; then
    CURRENT_SERVICE="$svc"
    break
  fi
done

# If not in a service directory, allow
if [[ -z "$CURRENT_SERVICE" ]]; then
  exit 0
fi

# Check for direct references to other services' internal namespaces
for svc in "${SERVICES[@]}"; do
  if [[ "$svc" = "$CURRENT_SERVICE" ]]; then
    continue
  fi

  # Check for using statements or direct namespace references to other services
  if echo "$CONTENT" | grep -qE "using\s+BookWorm\.$svc\.(Domain|Infrastructure|Features|Grpc)"; then
    json_deny "Cross-service boundary violation: $CURRENT_SERVICE service must not directly reference $svc internal namespaces. Use integration events (Wolverine), gRPC contracts, or SharedKernel instead."
    exit 0
  fi

  # Check for direct project references to other services
  if echo "$CONTENT" | grep -qE "ProjectReference.*BookWorm\.$svc[/\\\\]"; then
    json_deny "Cross-service boundary violation: $CURRENT_SERVICE cannot have a direct ProjectReference to $svc. Services communicate via messaging (Wolverine/Kafka) or gRPC."
    exit 0
  fi
done

# Allow
exit 0
