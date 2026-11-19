#!/usr/bin/env bash
set -euo pipefail

shopt -s nullglob
result_dirs=(../../allure-results/unit/allure-results ../../allure-results/contract/allure-results)
existing_dirs=()
for dir in "${result_dirs[@]}"; do
  if [ -d "$dir" ] && [ -n "$(ls -A "$dir" 2>/dev/null)" ]; then
    existing_dirs+=("$dir")
  fi
done
if [ ${#existing_dirs[@]} -gt 0 ]; then
  mkdir -p .allure
  curl -sfL --proto '=https' --tlsv1.2 "https://foxminchan.github.io/BookWorm/reports/backend/history.jsonl" -o .allure/history-backend.jsonl || rm -f .allure/history-backend.jsonl
  ./node_modules/.bin/allure generate "${existing_dirs[@]}" --clean --config ./allurerc.mjs --output ../../allure-report-backend
  if [ -f .allure/history-backend.jsonl ]; then
    cp .allure/history-backend.jsonl ../../allure-report-backend/history.jsonl
  fi
else
  echo "No Backend Allure results found to generate report."
fi
