# Políticas do repositório

Arquivo gerenciado pelo Altivy.Groundwork (componente `repository-policy`). Não edite:
mudanças vão para o catálogo. O contrato versionado é
[repository-policy.json](repository-policy.json), aplicado por `groundwork github`.

## Rulesets

| Ruleset | Alvo | Regras | Bypass |
| --- | --- | --- | --- |
| `gw-main-integrity` | branch padrão (`main`) | sem deleção, sem force push, histórico linear, check `Result` (modo strict) | **nenhum** |
| `gw-main-review` | branch padrão | PR obrigatório com 1 aprovação(ões), revisões antigas descartadas em novo push, conversas resolvidas, somente squash | admins **somente via PR** |
| `gw-release-tags` | `refs/tags/v*` | criação, atualização e deleção bloqueadas | somente o GitHub App de release |

Os rulesets são separados porque o bypass vale por ruleset: um admin pode fazer merge do
próprio PR sem aprovação de terceiros (bypass em modo `pull_request`, sem push direto), mas não
consegue fazer merge com o `Result` vermelho nem com histórico não linear.

## Opções do repositório

- Merge somente squash, com título e corpo do PR; branch apagada após o merge; botão de
  atualização da branch habilitado.
- `GITHUB_TOKEN` somente leitura por padrão e sem permissão de aprovar PRs.
- Alertas e security updates do Dependabot habilitados.
- Em repositórios públicos, secret scanning e push protection habilitados.

## Environments

- `release`: revisores `@lzocateli`; aceita `main` e `tag:v*`
  (publicação inicial e `republish`). Em repositório privado sem plano que suporte revisores, o
  controle humano é o merge do Release PR sob `gw-main-review` (reportado como *compensado*).
- `preview`: sem revisores; aceita somente `main` (snapshots).

`prevent_self_review` fica desligado para que o mantenedor único possa aprovar.

## Comandos

| Comando | Efeito | Saída |
| --- | --- | --- |
| `groundwork github` | Lê a configuração remota via `gh` e compara com o contrato; não altera nada | `0` conforme, `2` divergente, `1` erro |
| `groundwork github --sync --dry-run` | Mostra o plano de criação/atualização | `2` se houver mudanças |
| `groundwork github --sync` | Cria ou atualiza somente o que pertence ao contrato e confirma com nova leitura | `0` após confirmação |
| `groundwork github --sync --replace-legacy` | Também remove a proteção clássica da branch padrão e os rulesets listados em `legacyRulesets`, depois de confirmar os `gw-*` | idem |

Garantias:

- nunca remove nem altera rulesets, Environments ou secrets fora do contrato sem
  `--replace-legacy` explícito;
- não lê, cria nem exibe valores de secrets: só confere os nomes de `vars.RELEASE_APP_ID` e
  `RELEASE_APP_PRIVATE_KEY`, que são criados manualmente (`gh variable set`, `gh secret set`);
- exige `gh` autenticado com permissão de administrador; sem ela, falha com código `1`;
- recursos indisponíveis no plano do GitHub são reportados como *não aplicado*, nunca como
  sucesso;
- não é executado por `init`, `add` ou `update`.

## Sequência segura de adoção

1. `groundwork github --sync --dry-run` e revisão do plano.
2. `groundwork github --sync`: os `gw-*` convivem com as proteções antigas (são mais restritivos).
3. Um PR de teste passa com o check `Result`.
4. `groundwork github --sync --replace-legacy`: remove proteções antigas que anulariam o bypass
   de admins via PR.
5. `groundwork github` retorna `0`.
