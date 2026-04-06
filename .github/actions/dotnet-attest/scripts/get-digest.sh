#!/usr/bin/env bash
set -euo pipefail

# Retrieves the latest container image digest from GitHub Packages API.
# Retries with incremental backoff to handle GHCR propagation delays.
#
# Required environment variables:
#   GH_TOKEN       - GitHub token for API authentication
#   SERVICE        - Service name (e.g., "catalog", "ordering")
#   REPOSITORY     - Full repository name (e.g., "foxminchan/BookWorm")
#   GITHUB_OUTPUT  - GitHub Actions output file path

REPO_NAME=$(echo "$REPOSITORY" | cut -d'/' -f2 | tr '[:upper:]' '[:lower:]')
PACKAGE="${REPO_NAME}/${SERVICE}"
ENCODED_PACKAGE=$(echo "$PACKAGE" | sed 's|/|%2F|g')
OWNER=$(echo "$REPOSITORY" | cut -d'/' -f1)

MAX_ATTEMPTS=5
SLEEP_SECONDS=2
DIGEST=""

for ATTEMPT in $(seq 1 "$MAX_ATTEMPTS"); do
  DIGEST=$(gh api "/users/${OWNER}/packages/container/${ENCODED_PACKAGE}/versions?per_page=1" \
    --jq '.[0].name' 2>/dev/null) || \
  DIGEST=$(gh api "/orgs/${OWNER}/packages/container/${ENCODED_PACKAGE}/versions?per_page=1" \
    --jq '.[0].name' 2>/dev/null) || true

  if [[ -n "$DIGEST" && "$DIGEST" != "null" && "$DIGEST" =~ ^sha256:[0-9a-f]{64}$ ]]; then
    break
  fi

  DIGEST=""

  if [[ "$ATTEMPT" -lt "$MAX_ATTEMPTS" ]]; then
    echo "Digest for ${SERVICE} not available yet (attempt ${ATTEMPT}/${MAX_ATTEMPTS}); retrying in ${SLEEP_SECONDS}s..."
    sleep "$SLEEP_SECONDS"
    SLEEP_SECONDS=$((SLEEP_SECONDS + 2))
  fi
done

if [[ -z "$DIGEST" || ! "$DIGEST" =~ ^sha256:[0-9a-f]{64}$ ]]; then
  echo "::error::Failed to get valid digest for ${SERVICE} after ${MAX_ATTEMPTS} attempts" >&2
  exit 1
fi

echo "digest=${DIGEST}" >> "$GITHUB_OUTPUT"
