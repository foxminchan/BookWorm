#!/usr/bin/env bash
set -euo pipefail

shopt -s nullglob
result_dirs=(../../allure-results-all/*/allure-results)
if [ ${#result_dirs[@]} -gt 0 ]; then
  mkdir -p .allure
  curl -sfL --proto '=https' --tlsv1.2 "https://foxminchan.github.io/BookWorm/reports/history.jsonl" -o .allure/history.jsonl || rm -f .allure/history.jsonl
  ./node_modules/.bin/allure generate "${result_dirs[@]}" --clean --config ./allurerc.mjs --output ../../docs/dist/reports
  if [ -f .allure/history.jsonl ]; then
    cp .allure/history.jsonl ../../docs/dist/reports/history.jsonl
  fi
else
  echo "No Allure results found to merge."
fi
