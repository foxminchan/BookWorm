#!/usr/bin/env bash
set -euo pipefail

mkdir -p docs/dist/reports/tunit
shopt -s globstar nullglob
files=(docs/dist/reports/tunit/**/*.html)
{
  echo "["
  for i in "${!files[@]}"; do
    rel="${files[$i]#docs/dist/reports/tunit/}"
    name=$(basename "$rel" .html)
    comma=","
    if [ "$i" -eq $((${#files[@]} - 1)) ]; then comma=""; fi
    printf '  {"name": "%s", "path": "/reports/tunit/%s"}%s\n' "$name" "$rel" "$comma"
  done
  echo "]"
} > docs/dist/reports/tunit/manifest.json
