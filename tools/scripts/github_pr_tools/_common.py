import json
import os
import re
import urllib.error
import urllib.request

API_VERSION = "2022-11-28"
PR_URL_PATTERN = re.compile(r"^https://github\.com/([^/]+)/([^/]+)/pull/(\d+)(?:/.*)?$")


def get_token() -> str:
    token = (
        os.getenv("GITHUB_LZOCATELI_TOKEN")
        or os.getenv("GH_TOKEN")
        or os.getenv("GITHUB_TOKEN")
        or ""
    ).strip()
    if not token:
        raise RuntimeError(
            "As variaveis de ambiente GITHUB_LZOCATELI_TOKEN/GH_TOKEN/GITHUB_TOKEN nao estao definidas."
        )
    return token


def build_headers() -> dict[str, str]:
    return {
        "Authorization": f"Bearer {get_token()}",
        "Accept": "application/vnd.github+json",
        "Content-Type": "application/json",
        "X-GitHub-Api-Version": API_VERSION,
    }


def request_json(method: str, url: str, payload: dict | None = None) -> dict:
    data = None
    if payload is not None:
        data = json.dumps(payload).encode("utf-8")

    request = urllib.request.Request(
        url=url,
        data=data,
        headers=build_headers(),
        method=method.upper(),
    )

    try:
        with urllib.request.urlopen(request, timeout=60) as response:
            response_body = response.read().decode("utf-8")
            return json.loads(response_body) if response_body else {}
    except urllib.error.HTTPError as exc:
        body = exc.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"HTTP {exc.code} ao chamar GitHub. {body}") from exc
    except urllib.error.URLError as exc:
        raise RuntimeError(f"Falha de conexao com GitHub. {exc.reason}") from exc


def graphql_request(query: str, variables: dict | None = None) -> dict:
    payload = {"query": query, "variables": variables or {}}
    response = request_json("POST", "https://api.github.com/graphql", payload)

    if response.get("errors"):
        raise RuntimeError(
            f"Erro GraphQL no GitHub. {json.dumps(response['errors'], ensure_ascii=False)}"
        )

    return response.get("data") or {}


def parse_pr_url(pr_url: str) -> tuple[str, str, int]:
    url = (pr_url or "").strip()
    match = PR_URL_PATTERN.match(url)
    if not match:
        raise RuntimeError(
            "URL de PR invalida. Use o formato https://github.com/<owner>/<repo>/pull/<numero>."
        )

    owner, repo, pr_number = match.groups()
    return owner, repo, int(pr_number)


def print_json(data: object) -> None:
    print(json.dumps(data, ensure_ascii=False, indent=2))
