import argparse

from ._common import graphql_request, parse_pr_url, print_json

REVIEW_THREADS_QUERY = """
query($owner: String!, $repo: String!, $number: Int!, $cursor: String) {
  repository(owner: $owner, name: $repo) {
    pullRequest(number: $number) {
      reviewThreads(first: 100, after: $cursor) {
        pageInfo {
          hasNextPage
          endCursor
        }
        nodes {
          id
          isResolved
          isOutdated
          path
          line
          startLine
          comments(first: 100) {
            nodes {
              id
              databaseId
              body
              createdAt
              updatedAt
              url
              author {
                login
              }
              replyTo {
                databaseId
              }
            }
          }
        }
      }
    }
  }
}
"""


BOT_MARKERS = (
    "github-code-quality",
    "copilot",
    "codeql",
    "sonarqube",
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        prog="get-github-pr-comments",
        description="Coleta comentarios de PR no GitHub e retorna JSON.",
    )
    parser.add_argument("--owner", help="Owner do repositorio no GitHub.")
    parser.add_argument("--repo", help="Nome do repositorio no GitHub.")
    parser.add_argument(
        "--pull-request-number",
        type=int,
        help="Numero do Pull Request.",
    )
    parser.add_argument(
        "--pr-url",
        help="URL completa do PR (ex.: https://github.com/owner/repo/pull/210).",
    )
    parser.add_argument(
        "--open-threads-only",
        action="store_true",
        help="Retorna somente threads nao resolvidas.",
    )
    parser.add_argument(
        "--bots-only",
        action="store_true",
        help="Retorna somente comentarios de bots de revisao.",
    )
    return parser.parse_args()


def resolve_target(args: argparse.Namespace) -> tuple[str, str, int]:
    if args.pr_url:
        return parse_pr_url(args.pr_url)

    if not args.owner or not args.repo or not args.pull_request_number:
        raise RuntimeError(
            "Informe --pr-url ou o conjunto --owner --repo --pull-request-number."
        )

    return args.owner, args.repo, args.pull_request_number


def is_bot_comment(author: str | None, content: str | None) -> bool:
    normalized_author = (author or "").strip().lower()
    normalized_content = (content or "").strip().lower()

    return any(
        marker in normalized_author or marker in normalized_content
        for marker in BOT_MARKERS
    )


def fetch_review_threads(owner: str, repo: str, pr_number: int) -> list[dict]:
    items: list[dict] = []
    cursor = None

    while True:
        data = graphql_request(
            REVIEW_THREADS_QUERY,
            {
                "owner": owner,
                "repo": repo,
                "number": pr_number,
                "cursor": cursor,
            },
        )

        threads = ((data.get("repository") or {}).get("pullRequest") or {}).get(
            "reviewThreads"
        ) or {}
        thread_nodes = threads.get("nodes") or []

        for thread in thread_nodes:
            thread_status = "closed" if thread.get("isResolved") else "active"
            thread_path = thread.get("path")
            thread_line = thread.get("line") or thread.get("startLine")

            comments = (thread.get("comments") or {}).get("nodes") or []
            for comment in comments:
                items.append(
                    {
                        "threadId": thread.get("id"),
                        "threadStatus": thread_status,
                        "filePath": thread_path,
                        "line": thread_line,
                        "commentId": comment.get("databaseId"),
                        "parentCommentId": (
                            (comment.get("replyTo") or {}).get("databaseId")
                        ),
                        "author": ((comment.get("author") or {}).get("login")),
                        "publishedDate": comment.get("createdAt"),
                        "lastUpdatedDate": comment.get("updatedAt"),
                        "commentType": "text",
                        "content": comment.get("body"),
                        "isDeleted": False,
                        "isResolved": bool(thread.get("isResolved")),
                        "isOutdated": bool(thread.get("isOutdated")),
                        "url": comment.get("url"),
                    }
                )

        page_info = threads.get("pageInfo") or {}
        if not page_info.get("hasNextPage"):
            break

        cursor = page_info.get("endCursor")

    return items


def main() -> None:
    args = parse_args()
    owner, repo, pr_number = resolve_target(args)

    items = fetch_review_threads(owner, repo, pr_number)

    if args.open_threads_only:
        items = [item for item in items if item.get("threadStatus") == "active"]

    if args.bots_only:
        items = [
            item
            for item in items
            if is_bot_comment(item.get("author"), item.get("content"))
        ]

    print_json(items)


if __name__ == "__main__":
    main()
