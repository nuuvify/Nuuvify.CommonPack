# Tutorial de configuração manual no GitHub

Este documento lista o que ainda depende de ação manual no GitHub para o fluxo de CI/CD e
release do repositório. Rulesets, opções de merge, permissões do `GITHUB_TOKEN` e Environments
são aplicados pelo `groundwork github` a partir de
[.github/repository-policy.json](../../.github/repository-policy.json); não os configure pela
interface. A visão geral está em [github-setup.md](./github-setup.md) e o processo de release em
[release-process.md](./release-process.md).

## Pré-requisitos

- Ser administrador do repositório e da organização `nuuvify` no GitHub.
- Ter a CLI `gh` autenticada e a CLI `groundwork` do Altivy.Groundwork instalada.
- Ter em mãos uma chave do NuGet.org com permissão de push para os pacotes `Nuuvify.CommonPack.*`.

## 1. Habilitar features básicas

Caminho: `Settings > General > Features`

- `Issues`: habilitado
- `Discussions`: habilitado
- `Projects` e `Wiki`: opcionais

As opções de merge (somente squash, apagar branch após o merge) são aplicadas pelo
`groundwork github --sync`.

## 2. Validar CODEOWNERS

Abra um PR de teste para `main` e confirme que o GitHub solicita revisão de `@lzocateli`.

## 3. GitHub App de release

O App `Altivy Release` precisa estar instalado na organização com acesso ao repositório, e a
variável `RELEASE_APP_ID` e o secret `RELEASE_APP_PRIVATE_KEY` precisam estar disponíveis para o
repositório no nível da organização. Sem eles, o Release PR não dispara checks e o workflow
`Release` falha de forma explícita.

## 4. Aplicar as políticas do repositório

```bash
groundwork github                         # conferência, somente leitura
groundwork github --sync                  # cria rulesets gw-*, Environments e opções
groundwork github --sync --replace-legacy # remove a proteção clássica depois de validar o CI
```

Use `--replace-legacy` somente depois que o check `Result` estiver verde em um PR, para não
deixar a `main` sem o check obrigatório.

## 5. Cadastrar o `NUGET_API_KEY`

Depois do `--sync`, o Environment `release` existe. Cadastre a chave nele:

```bash
gh secret set NUGET_API_KEY --repo nuuvify/Nuuvify.CommonPack --env release
```

O comando lê o valor do terminal sem gravá-lo no histórico do shell.

## 6. Sincronizar labels e validar triagem

1. Execute o workflow `Label Sync` em `Actions`, se necessário.
2. Confirme os labels em `Issues > Labels`.
3. Abra um PR de teste mexendo em `docs/` ou `test/` e confirme que o `PR Triage` aplicou labels.

## 7. Teste controlado do fluxo

1. Abra um PR para `main` e confirme que o merge fica bloqueado até o `Result` verde.
2. Faça o merge e confirme o job `Snapshot` publicando `X.Y.(Z+1)-preview.N` no GitHub Packages.
3. Confirme que o release-please abriu ou atualizou o Release PR e que o workflow
   `Package changelog release` fechou os CHANGELOGs dos pacotes.
4. Faça o merge do Release PR, aprove o Environment `release` e confirme os pacotes no NuGet.org
   e a GitHub Release publicada.

## 8. Solução de problemas rápida

| Sintoma | Verifique |
| --- | --- |
| Merge bloqueado com `Result` verde | Proteção clássica ainda ativa: rode `groundwork github --sync --replace-legacy` |
| Release PR sem checks | `RELEASE_APP_ID`, `RELEASE_APP_PRIVATE_KEY` e instalação do App |
| `Publish packages` sem credencial | `NUGET_API_KEY` no Environment `release` |
| Labels não criados | Execução do `Label Sync` e validade de `.github/labels.yml` |
| PR sem labels automáticos | Execução do `PR Triage` e cobertura de `.github/labeler.yml` |

## Resumo do que é manual

- habilitar Issues e Discussions;
- garantir o App de release e seus secrets na organização;
- executar `groundwork github --sync` e, após validar o CI, `--replace-legacy`;
- cadastrar `NUGET_API_KEY` no Environment `release`;
- executar `Label Sync` e validar o fluxo com um PR real.
