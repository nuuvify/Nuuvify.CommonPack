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
- Ter acesso à conta NuGet.org proprietária dos pacotes `Nuuvify.CommonPack.*`.

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

## 5. Configurar Trusted Publishing no NuGet.org

Depois do `--sync`, os Environments `preview` e `release` existem.

### Preencher a tela Trusted Publishing

1. Acesse [NuGet.org > Trusted Publishing](https://www.nuget.org/account/trustedpublishing)
   com a conta proprietária dos pacotes e abra o formulário de criação.
2. Crie **duas políticas**, uma para cada coluna. Preencha todos os campos assim:

   | Campo da tela | Preview | Release |
   | --- | --- | --- |
   | **Policy Name** | `NuuvifyCommonPackPreview` | `NuuvifyCommonPackRelease` |
   | **Package Owner** | `lincoln.zocateli` | `lincoln.zocateli` |
   | **CI/CD Provider** | `GitHub Actions` | `GitHub Actions` |
   | **Repository Owner** | `nuuvify` | `nuuvify` |
   | **Repository** | `Nuuvify.CommonPack` | `Nuuvify.CommonPack` |
   | **Workflow File** | `ci.yml` | `gw-release.yml` |
   | **Environment** | `preview` | `release` |
   | **Select Scopes: Push** | Marcar | Marcar |
   | **Push new packages and package versions** | Selecionar | Selecionar |
   | **Push only new package versions** | Não selecionar | Não selecionar |
   | **Unlist or relist package versions** | Desmarcar | Desmarcar |
   | **Stage packages** | Desmarcar | Desmarcar |
   | **Glob Patterns and Packages (Separate by a new line)** | `Nuuvify.CommonPack.*` | `Nuuvify.CommonPack.*` |

   Os nomes das políticas são sugestões para identificá-las em **Manage**.
   **Package Owner** é o proprietário no NuGet.org: selecione `lincoln.zocateli`, dono,
   por exemplo, de [MftMailbox.Sftp](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Sftp/)
   e [AutoHistory](https://www.nuget.org/packages/Nuuvify.CommonPack.AutoHistory/).
   Não selecione `Altivy` para esses pacotes enquanto ela não for proprietária.
   **Repository Owner** é diferente: `nuuvify` identifica a organização no GitHub.

   Em **Workflow File**, informe somente o nome do arquivo, sem `.github/workflows/`.
   Para release, use `gw-release.yml`: ele é o workflow chamador de `publish.yml`.
   O padrão `Nuuvify.CommonPack.*` limita a política aos IDs iniciados por
   `Nuuvify.CommonPack.`; não use `*` sozinho, que permitiria todos os pacotes da conta.

   O escopo **Push new packages and package versions** também permite criar novos pacotes
   dentro desse padrão. Para limitar a política aos pacotes já existentes, escolha
   **Push only new package versions**; será necessário ajustar a permissão antes de publicar
   um novo pacote.

   Para recuperar uma release estável em rascunho de uma tag anterior ao Trusted Publishing,
   crie uma terceira política com os mesmos campos da coluna **Release**, exceto
   **Policy Name** = `NuuvifyCommonPackStableRecovery` e **Workflow File** =
   `recover-stable-release.yml`. O **Environment** continua sendo `release`.
3. Clique em **Create** para cada política e confira-as em **Manage**.
   Quando aplicável, uma política fica temporariamente ativa por sete dias e passa à ativação
   permanente após uma publicação bem-sucedida; se expirar, reative-a no NuGet.org.

### Configurar o usuário no GitHub

Cadastre a variável de Actions `NUGET_USER` com o nome de usuário NuGet.org
`lincoln.zocateli`, associado às políticas (não use e-mail nem o login do GitHub):

```bash
gh variable set NUGET_USER --repo nuuvify/Nuuvify.CommonPack --body lincoln.zocateli
```

O job solicita `id-token: write` e executa `NuGet/login` imediatamente antes do push.
A credencial emitida expira em uma hora e não é persistida em secrets nem artifacts.
Depois de validar o novo fluxo, remova os secrets antigos `NUGET_API_KEY` dos Environments
`preview` e `release` e revogue as chaves permanentes no NuGet.org.

Consulte a [documentação oficial](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

## 6. Sincronizar labels e validar triagem

1. Execute o workflow `Label Sync` em `Actions`, se necessário.
2. Confirme os labels em `Issues > Labels`.
3. Abra um PR de teste mexendo em `docs/` ou `test/` e confirme que o `PR Triage` aplicou labels.

## 7. Teste controlado do fluxo

1. Abra um PR para `main` e confirme que o merge fica bloqueado até o `Result` verde.
2. Faça o merge e, se o título for releasable, confirme o job `Snapshot` publicando a próxima
   versão SemVer com sufixo `-preview.N` no NuGet.org. Commits de documentação e o merge do
   Release PR não publicam snapshot.
3. Confirme que o release-please abriu ou atualizou o Release PR e que o workflow
   `Package changelog release` fechou os CHANGELOGs dos pacotes.
4. Faça o merge do Release PR, aprove o Environment `release` e confirme os pacotes no NuGet.org
   e a GitHub Release publicada.

## 8. Solução de problemas rápida

| Sintoma | Verifique |
| --- | --- |
| Merge bloqueado com `Result` verde | Proteção clássica ainda ativa: rode `groundwork github --sync --replace-legacy` |
| Release PR sem checks | `RELEASE_APP_ID`, `RELEASE_APP_PRIVATE_KEY` e instalação do App |
| Login NuGet falha | `NUGET_USER`, proprietário/escopo da política, workflow chamador e Environment; confirme `id-token: write` |
| Política pendente de ativação | Execute a primeira publicação no prazo indicado pelo NuGet.org; reative a política se necessário |
| Labels não criados | Execução do `Label Sync` e validade de `.github/labels.yml` |
| PR sem labels automáticos | Execução do `PR Triage` e cobertura de `.github/labeler.yml` |

## Resumo do que é manual

- habilitar Issues e Discussions;
- garantir o App de release e seus secrets na organização;
- executar `groundwork github --sync` e, após validar o CI, `--replace-legacy`;
- cadastrar as duas políticas Trusted Publishing e a variável `NUGET_USER`;
- executar `Label Sync` e validar o fluxo com um PR real.
