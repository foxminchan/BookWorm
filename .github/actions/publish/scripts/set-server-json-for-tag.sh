#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${GITHUB_REF_NAME:-}" ]]; then
  echo "::error::GITHUB_REF_NAME is required"
  exit 1
fi

if [[ -z "${NORMALIZED_REPOSITORY:-}" ]]; then
  echo "::error::NORMALIZED_REPOSITORY is required"
  exit 1
fi

TAG_NAME="${GITHUB_REF_NAME}"
VERSION="${TAG_NAME#v}"

if [[ -z "${VERSION}" ]]; then
  echo "::error::Unable to derive version from tag '${TAG_NAME}'"
  exit 1
fi

MCPTOOLS_IMAGE_REPOSITORY="ghcr.io/${NORMALIZED_REPOSITORY}/mcptools"
MCPTOOLS_IMAGE_REF=$(docker image ls --format '{{.Repository}}:{{.Tag}}' \
  | grep -E "^${MCPTOOLS_IMAGE_REPOSITORY}:aspire-deploy-[0-9]{14}$" \
  | sort \
  | tail -n1)

if [[ -z "${MCPTOOLS_IMAGE_REF}" ]]; then
  echo "::error::Unable to resolve pushed mcptools image reference from local Docker images"
  exit 1
fi

jq --arg v "$VERSION" --arg image "$MCPTOOLS_IMAGE_REF" '
  .version = $v |
  .packages[0].identifier = $image
' server.json > server.tmp && mv server.tmp server.json
