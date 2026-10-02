#!/usr/bin/env bash
set -euo pipefail

if [[ "$TEST_SESSION_TIMEOUT" =~ ^([0-9]+)(s|m|h)$ ]]; then
  session_timeout_value=${BASH_REMATCH[1]}
  session_timeout_unit=${BASH_REMATCH[2]}
  case "$session_timeout_unit" in
    s) session_timeout_seconds=$session_timeout_value ;;
    m) session_timeout_seconds=$((session_timeout_value * 60)) ;;
    h) session_timeout_seconds=$((session_timeout_value * 3600)) ;;
    *)
      echo "Unsupported testSessionTimeout unit: $session_timeout_unit" >&2
      exit 1
      ;;
  esac
else
  echo "Unsupported testSessionTimeout value: $TEST_SESSION_TIMEOUT" >&2
  exit 1
fi
session_start=$SECONDS

test_args=(
  --configuration Release
  --no-restore
  --report-trx
  --coverage
  --coverage-settings Coverage.runsettings
  --results-directory ./test-results
  --github-reporter-style full
  --hangdump
  --hangdump-timeout "$TEST_HANG_TIMEOUT"
  --crashdump
  -p:SolutionDir="$GITHUB_WORKSPACE/"
)
if [[ "$NO_BUILD" == "true" ]]; then
  test_args+=(--no-build)
fi

run_test() {
  local project="$1"
  shift
  local elapsed=$((SECONDS - session_start))
  local remaining=$((session_timeout_seconds - elapsed))
  if (( remaining <= 0 )); then
    echo "The complete test session exceeded $TEST_SESSION_TIMEOUT." >&2
    return 1
  fi

  dotnet test "$project" "${test_args[@]}" --timeout "${remaining}s" "$@"
}

mapfile -d '' unit_projects < <(
  find src tests -type f \
    \( -name '*UnitTests.csproj' -o -name '*ArchTests.csproj' -o -name '*IntegrationTests.csproj' \) \
    -print0 | sort -z
)
for project in "${unit_projects[@]}"; do
  run_test "$project"
done

consumer_projects=(
  src/Services/Basket/BookWorm.Basket.ContractTests/BookWorm.Basket.ContractTests.csproj
  src/Services/Catalog/BookWorm.Catalog.ContractTests/BookWorm.Catalog.ContractTests.csproj
  src/Services/Finance/BookWorm.Finance.ContractTests/BookWorm.Finance.ContractTests.csproj
  src/Services/Notification/BookWorm.Notification.ContractTests/BookWorm.Notification.ContractTests.csproj
  src/Services/Ordering/BookWorm.Ordering.ContractTests/BookWorm.Ordering.ContractTests.csproj
  src/Services/Rating/BookWorm.Rating.ContractTests/BookWorm.Rating.ContractTests.csproj
)
for project in "${consumer_projects[@]}"; do
  run_test "$project" --treenode-filter '/*/*/*/*[Category=PactConsumer]'
done

provider_projects=(
  src/Services/Basket/BookWorm.Basket.ContractTests/BookWorm.Basket.ContractTests.csproj
  src/Services/Catalog/BookWorm.Catalog.ContractTests/BookWorm.Catalog.ContractTests.csproj
  src/Services/Finance/BookWorm.Finance.ContractTests/BookWorm.Finance.ContractTests.csproj
  src/Services/Ordering/BookWorm.Ordering.ContractTests/BookWorm.Ordering.ContractTests.csproj
  src/Services/Rating/BookWorm.Rating.ContractTests/BookWorm.Rating.ContractTests.csproj
  src/Services/Scheduler/BookWorm.Scheduler.ContractTests/BookWorm.Scheduler.ContractTests.csproj
)
for project in "${provider_projects[@]}"; do
  run_test "$project" --treenode-filter '/*/*/*/*[Category=PactProvider]'
done
