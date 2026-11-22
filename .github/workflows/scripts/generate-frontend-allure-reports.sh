#!/usr/bin/env bash
set -euo pipefail

apps=(backoffice storefront commerce)

for app in "${apps[@]}"; do
  if [[ -d "apps/${app}" ]]; then
    mkdir -p "apps/${app}/.allure"
    curl -sfL --proto '=https' --tlsv1.2 "https://foxminchan.github.io/BookWorm/reports/${app}/history.jsonl" -o "apps/${app}/.allure/history.jsonl" || rm -f "apps/${app}/.allure/history.jsonl"
  fi
done

./node_modules/.bin/turbo run allure:generate --affected --continue

for app in "${apps[@]}"; do
  if [[ -d "apps/${app}/allure-report" && -f "apps/${app}/.allure/history.jsonl" ]]; then
    cp "apps/${app}/.allure/history.jsonl" "apps/${app}/allure-report/history.jsonl"
  fi
done
