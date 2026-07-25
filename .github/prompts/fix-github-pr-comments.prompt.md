---
name: "Fix GitHub PR Comments"
description: "Buscar comentarios de Pull Request no GitHub por API, priorizar achados e aplicar correcoes locais com validacao focada."
argument-hint: "URL do PR do GitHub ou owner/repo/prNumber"
agent: "nuuvify-GitHub PR Fixer"
model: "GPT-5 (copilot)"
tools: [read, edit, search, execute, todo, vscode_askQuestions]
---

Busque os comentarios do Pull Request no GitHub usando a API e as variaveis de ambiente `GITHUB_LZOCATELI_TOKEN` (preferencial), `GH_TOKEN` ou `GITHUB_TOKEN`.

Use os comandos via projeto dedicado de scripts:
- `uv run --project .tools/scripts get-github-pr-comments`
- `uv run --project .tools/scripts update-github-pr-comments`

Se o usuario informar a URL completa do PR no formato `https://github.com/<owner>/<repo>/pull/<prNumber>`, extraia automaticamente `owner`, `repo` e `prNumber`.

Se a URL nao for informada, pergunte ao usuario apenas os dados faltantes.

Depois: agrupe por arquivo, linha e regra; priorize maior impacto tecnico; aplique correcoes; valide com build ou testes focados; resuma o que foi corrigido e o que restou.

Use tambem:
- [Nuuvify GitHub PR Workflow](../instructions/github-pr.instructions.md)
