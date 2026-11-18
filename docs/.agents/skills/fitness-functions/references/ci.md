# Running fitness functions in CI

`eventcatalog fitness check` exits with `1` when an `error`-severity function finds violations or a function can't be evaluated, so a CI job running it fails exactly when the architecture breaks a rule. Warnings are printed but don't fail the job.

| Exit code | Meaning                                       | CI result |
| --------- | --------------------------------------------- | --------- |
| `0`       | Every function passed, or only warned         | passes    |
| `1`       | A function failed, or couldn't be evaluated   | fails     |
| `2`       | The check couldn't run (e.g. wrong directory) | fails     |

## GitHub Actions

```yaml
# .github/workflows/fitness.yml
name: Architecture fitness

on:
  pull_request:
  push:
    branches: [main]

jobs:
  fitness:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 20
          cache: npm
      - run: npm ci
      - run: npx eventcatalog fitness check
```

If the catalog isn't at the repository root, set `working-directory` on the step (or `PROJECT_DIR`) for `eventcatalog`, or pass the directory to the CLI package:

```yaml
- run: npx @eventcatalog/cli --dir ./catalog fitness check
```

`@eventcatalog/cli` runs the same check without installing `@eventcatalog/core` (which brings in the whole site build), which makes for a lighter job:

```yaml
- uses: actions/checkout@v4
- uses: actions/setup-node@v4
  with:
    node-version: 20
- run: npx @eventcatalog/cli fitness check
```

### Keep the report

The report is written to `.eventcatalog/fitness/results.json` (gitignored). Upload it so it can be inspected after the run, even when the check fails:

```yaml
- run: npx eventcatalog fitness check
- uses: actions/upload-artifact@v4
  if: always()
  with:
    name: fitness-report
    path: .eventcatalog/fitness/results.json
```

### Summarise it on the run

```yaml
- run: npx eventcatalog fitness check --format json > fitness.json || true
- run: |
    jq -r '"### Architecture fitness\n\n\(.summary.passed) passed, \(.summary.warnings) warnings, \(.summary.failed) failed, \(.summary.errors) errors\n",
           (.functions[] | select(.status != "passed") | "- **\(.name)** (\(.status)): \(.violations | length) violation(s)")' fitness.json >> "$GITHUB_STEP_SUMMARY"
- run: jq -e '.summary.failed == 0 and .summary.errors == 0' fitness.json
```

The last step fails the job when a rule failed. That's needed because `|| true` let the first step pass so the summary could still be written.

## GitLab CI

```yaml
fitness:
  image: node:20
  script:
    - npm ci
    - npx eventcatalog fitness check
  artifacts:
    when: always
    paths:
      - .eventcatalog/fitness/results.json
```

## Alongside the linter and governance

They check different things and can run in the same job:

```yaml
- run: npx @eventcatalog/linter # files are valid
- run: npx eventcatalog fitness check # the architecture follows the team's rules
- run: npx @eventcatalog/cli governance check --base origin/main # what this change affects
```

## Introducing a rule to an existing catalog

A new rule often finds many existing violations. To avoid blocking every pull request:

1. Add it with `severity: 'warning'` and merge it. CI stays green, and the warnings are visible on every run.
2. Fix the existing violations (or narrow the rule).
3. Change it to `severity: 'error'` so new violations fail CI.

## Reading the JSON report

```bash
# Functions that didn't pass
jq -r '.functions[] | select(.status != "passed") | "\(.status)\t\(.id)\t\(.source)"' .eventcatalog/fitness/results.json

# Every resource a violation points at
jq -r '.functions[].violations[].resources[] | "\(.type):\(.id)"' .eventcatalog/fitness/results.json | sort -u

# Why functions errored
jq -r '.functions[] | select(.status == "error") | "\(.source): \(.error.message)"' .eventcatalog/fitness/results.json
```
