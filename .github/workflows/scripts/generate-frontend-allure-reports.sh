#!/usr/bin/env bash
set -euo pipefail

mkdir -p apps/backoffice/.allure apps/storefront/.allure
curl -sfL --proto '=https' --tlsv1.2 "https://foxminchan.github.io/BookWorm/reports/backoffice/history.jsonl" -o apps/backoffice/.allure/history.jsonl || rm -f apps/backoffice/.allure/history.jsonl
curl -sfL --proto '=https' --tlsv1.2 "https://foxminchan.github.io/BookWorm/reports/storefront/history.jsonl" -o apps/storefront/.allure/history.jsonl || rm -f apps/storefront/.allure/history.jsonl
./node_modules/.bin/turbo run allure:generate --affected --continue
if [ -d apps/backoffice/allure-report ] && [ -f apps/backoffice/.allure/history.jsonl ]; then
  cp apps/backoffice/.allure/history.jsonl apps/backoffice/allure-report/history.jsonl
fi
if [ -d apps/storefront/allure-report ] && [ -f apps/storefront/.allure/history.jsonl ]; then
  cp apps/storefront/.allure/history.jsonl apps/storefront/allure-report/history.jsonl
fi
