---
safe-outputs:
  threat-detection:
    engine:
      id: copilot
      model: gpt-5-mini
  add-labels:
    allowed:
      - bug
      - feature
      - priority/p0
      - priority/p1
      - priority/p2
    max: 3
  add-comment:
    max: 1
  noop:
---
