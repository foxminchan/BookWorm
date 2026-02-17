#!/bin/bash
# BookWorm — Architecture boundary guard pre-tool hook
# Enforces microservice boundaries by preventing cross-service internal references.
set -e

INPUT=$(cat)
TOOL_NAME=$(echo "$INPUT" | jq -r '.toolName')
TOOL_ARGS=$(echo "$INPUT" | jq -r '.toolArgs')

# Only check file creation/edits in service directories
if [[ "$TOOL_NAME" != "edit" ]] && [[ "$TOOL_NAME" != "create" ]]; then
  exit 0
fi

FILE_PATH=$(echo "$TOOL_ARGS" | jq -r '.path // .filePath // empty')
CONTENT=$(echo "$TOOL_ARGS" | jq -r '.content // .newText // .new_string // empty')

# Service names in the project
SERVICES=("Catalog" "Basket" "Ordering" "Rating" "Chat" "Finance" "Notification" "Scheduler")

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
    jq -n --arg reason "Cross-service boundary violation: $CURRENT_SERVICE service must not directly reference $svc internal namespaces. Use integration events (MassTransit), gRPC contracts, or SharedKernel instead." \
      '{permissionDecision: "deny", permissionDecisionReason: $reason}'
    exit 0
  fi

  # Check for direct project references to other services
  if echo "$CONTENT" | grep -qE "ProjectReference.*BookWorm\.$svc[/\\\\]"; then
    jq -n --arg reason "Cross-service boundary violation: $CURRENT_SERVICE cannot have a direct ProjectReference to $svc. Services communicate via messaging (MassTransit/RabbitMQ) or gRPC." \
      '{permissionDecision: "deny", permissionDecisionReason: $reason}'
    exit 0
  fi
done

# Allow
exit 0
