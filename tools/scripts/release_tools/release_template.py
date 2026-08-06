import argparse

from ._common import print_json, utc_now_iso


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        prog="release-template",
        description="Comando template para novos scripts de release.",
    )
    parser.add_argument(
        "--channel",
        default="stable",
        choices=["stable", "beta", "alpha"],
        help="Canal de release alvo.",
    )
    parser.add_argument(
        "--what-if",
        action="store_true",
        help="Simula a execucao sem efetivar mudancas.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()

    print_json(
        {
            "tool": "release-template",
            "channel": args.channel,
            "whatIf": bool(args.what_if),
            "timestampUtc": utc_now_iso(),
            "message": "Template pronto para evolucao de automacoes de release.",
        }
    )


if __name__ == "__main__":
    main()
