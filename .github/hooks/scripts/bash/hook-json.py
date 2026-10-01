#!/usr/bin/env python3
import json
import sys
from datetime import datetime, timezone


def get_value(data, path):
    value = data
    for key in path.split("."):
        if not isinstance(value, dict) or key not in value:
            return None
        value = value[key]
    return value


def raw_string(value):
    if value is None:
        return "null"
    if isinstance(value, (dict, list)):
        return json.dumps(value, separators=(",", ":"))
    if isinstance(value, bool):
        return str(value).lower()
    return str(value)


def main():
    action = sys.argv[1]

    if action == "get":
        data = json.load(sys.stdin)
        for path in sys.argv[2:]:
            value = get_value(data, path)
            if value is not None:
                if isinstance(value, (dict, list)):
                    print(json.dumps(value, separators=(",", ":")))
                elif isinstance(value, bool):
                    print(str(value).lower())
                else:
                    print(value)
                return
        return

    if action == "deny":
        print(
            json.dumps(
                {
                    "permissionDecision": "deny",
                    "permissionDecisionReason": sys.argv[2],
                },
                separators=(",", ":"),
            )
        )
        return

    if action == "audit":
        data = json.load(sys.stdin)
        tool_result = data.get("toolResult") or {}
        print(
            json.dumps(
                {
                    "timestamp": raw_string(data.get("timestamp")),
                    "date": datetime.now(timezone.utc)
                    .isoformat(timespec="seconds")
                    .replace("+00:00", "Z"),
                    "tool": raw_string(data.get("toolName")),
                    "result": raw_string(tool_result.get("resultType")),
                },
                separators=(",", ":"),
            )
        )
        return

    raise ValueError(f"Unsupported action: {action}")


if __name__ == "__main__":
    main()
