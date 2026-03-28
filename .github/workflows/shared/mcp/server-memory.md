---
tools:
  cache-memory:
    key: daily-status-${{ github.repository }}-${{ github.workflow }}
    description: Keep short-term (7-day) status snapshots for trend comparison.
    allowed-extensions: [.json, .md, .txt]
    retention-days: 30
  repo-memory:
    description: Persist long-term daily status trends and recommendation history.
    file-glob: [memory/default/*.md]
    allowed-extensions: [.md]
    max-file-size: 51200
---
