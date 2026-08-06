import json
from datetime import UTC, datetime


def utc_now_iso() -> str:
    return datetime.now(UTC).isoformat()


def print_json(data: object) -> None:
    print(json.dumps(data, ensure_ascii=False, indent=2))
