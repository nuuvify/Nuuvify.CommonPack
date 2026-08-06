import argparse

from ._common import graphql_request, parse_pr_url, print_json, request_json

GET_THREAD_QUERY = """
query($threadId: ID!) {
  node(id: $threadId) {
    ... on PullRequestReviewThread {
      id
      isResolved
      comments(last: 1) {
        nodes {
          databaseId
          url
        }
      }
    }
  }
}
"""


RESOLVE_THREAD_MUTATION = """
mutation($threadId: ID!) {
  resolveReviewThread(input: {threadId: $threadId}) {
    thread {
      id
      isResolved
    }
  }
}
"""


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        prog="update-github-pr-comments",
        description="Responde e opcionalmente resolve uma thread de PR no GitHub.",
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
        "--thread-id",
        required=True,
        help="ID GraphQL da thread de review no PR.",
    )
    parser.add_argument(
        "--comment",
        required=True,
        help="Texto da resposta que sera enviada para a thread.",
    )
    parser.add_argument(
        "--close-thread",
        action="store_true",
        help="Resolve a thread apos enviar o comentario.",
    )
    parser.add_argument(
        "--what-if",
        action="store_true",
        help="Simula a operacao sem enviar comentario nem resolver thread.",
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


def get_last_comment_id(thread_id: str) -> int:
    data = graphql_request(GET_THREAD_QUERY, {"threadId": thread_id})
    node = data.get("node") or {}

    comments = (node.get("comments") or {}).get("nodes") or []
    if not comments:
        raise RuntimeError("A thread informada nao possui comentario para resposta.")

    comment_id = comments[-1].get("databaseId")
    if not comment_id:
        raise RuntimeError("Nao foi possivel identificar o commentId da thread.")

    return int(comment_id)


def post_reply(owner: str, repo: str, comment_id: int, content: str) -> dict:
    url = f"https://api.github.com/repos/{owner}/{repo}/pulls/comments/{comment_id}/replies"
    return request_json("POST", url, {"body": content})


def resolve_thread(thread_id: str) -> bool:
    data = graphql_request(RESOLVE_THREAD_MUTATION, {"threadId": thread_id})
    thread = (data.get("resolveReviewThread") or {}).get("thread") or {}
    return bool(thread.get("isResolved"))


def main() -> None:
    args = parse_args()
    owner, repo, pr_number = resolve_target(args)

    if args.what_if:
        print_json(
            {
                "wouldPostComment": True,
                "wouldCloseThread": bool(args.close_thread),
                "owner": owner,
                "repo": repo,
                "pullRequestNumber": pr_number,
                "threadId": args.thread_id,
                "commentPreview": args.comment,
            }
        )
        return

    last_comment_id = get_last_comment_id(args.thread_id)
    posted = post_reply(owner, repo, last_comment_id, args.comment)

    thread_closed = False
    if args.close_thread:
        thread_closed = resolve_thread(args.thread_id)

    print_json(
        {
            "commentCreated": True,
            "commentId": posted.get("id"),
            "threadClosed": thread_closed,
            "owner": owner,
            "repo": repo,
            "pullRequestNumber": pr_number,
            "threadId": args.thread_id,
            "replyUrl": posted.get("html_url"),
        }
    )


if __name__ == "__main__":
    main()
