# tools/scripts

Projeto Python dedicado aos scripts executados com `uv`.

## Estrutura

- `pyproject.toml`: define metadados, dependencias e entrypoints CLI.
- `github_pr_tools/`: pacote atual de comentarios de PR no GitHub.
- `release_tools/`: pacote template para futuras automacoes de release/publicacao.

## Executar comandos

A partir da raiz do repositório, use:

```bash
uv run --project tools/scripts get-github-pr-comments --help
uv run --project tools/scripts update-github-pr-comments --help
uv run --project tools/scripts release-template --help
```

## Como adicionar novos scripts

1. Crie um novo pacote dentro de `tools/scripts` (ex.: `release_tools/`).
2. Adicione o entrypoint em `[project.scripts]` no `pyproject.toml`.
3. Mantenha nomes de comando em kebab-case e funções `main()` por script.
4. Prefira reaproveitar utilitários comuns em modulo `_common.py` do pacote.

## Pacote template

Use `release_tools/` como base para novos pacotes:

1. Copie a estrutura `__init__.py`, `_common.py` e um script `*_template.py`.
2. Renomeie o comando no `pyproject.toml` para refletir o dominio (ex.: `version-bump`, `publish-dry-run`).
3. Mantenha interface CLI consistente com `--what-if` sempre que houver efeito colateral.
