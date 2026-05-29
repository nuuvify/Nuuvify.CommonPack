# Plano: Padronização e Automação de CHANGELOGs

Este documento registra o plano completo para padronizar os arquivos `CHANGELOG.md` do repositório e dos pacotes individuais, e para automatizar a geração e validação dessas informações no CI/CD.

## Decisões

1. CHANGELOGs quebrados (contendo apenas uma URL) devem ser **apagados e reconstruídos do zero** com apenas `## [Não Lançado]` vazio.
2. PR validation deve **falhar** ao detectar alteração de código em qualquer pacote sem atualização do `CHANGELOG.md` correspondente — sem exceção por pacote.
3. A automação do CHANGELOG raiz usa **Commitizen como mecanismo principal** no CI; fallback por leitura de labels de PRs via `gh` quando Commitizen não detectar commits válidos.

---

## Diagnóstico inicial

### Estado dos CHANGELOGs por pacote (auditado em 2026-05-28)

| Pacote                      | Idioma             | Seção aberta       | Formato release             | Problema                                          |
| --------------------------- | ------------------ | ------------------ | --------------------------- | ------------------------------------------------- |
| raiz `CHANGELOG.md`         | pt-BR              | `## [Não Lançado]` | `## [x.y.z] - yyyy-mm-dd`   | ✅ correto                                         |
| BackgroundService           | pt-BR              | `## [Unreleased]`  | correto                     | seções em inglês (Added/Changed/Fixed)            |
| Email                       | inglês             | `## [Unreleased]`  | correto                     | roadmap em `Planned Features`; conteúdo em inglês |
| AzureServiceBus             | pt-BR              | ausente            | `## 2025-10-13` (data pura) | sem versão na entrada                             |
| AzureServiceBus.Abstraction | pt-BR              | ausente            | `## 2025-10-30` (data pura) | sem versão na entrada                             |
| UnitOfWork                  | pt-BR/inglês misto | `## [Unreleased]`  | `## 2025-10-30` (data pura) | misto; roadmap em `Aguardando`                    |
| UnitOfWork.Abstraction      | inglês             | `## [Unreleased]`  | data pura                   | misto; roadmap                                    |
| Security                    | —                  | ausente            | ausente                     | contém só uma URL — quebrado                      |
| Extensions                  | —                  | ausente            | ausente                     | contém só uma URL — quebrado                      |
| HealthCheck                 | —                  | ausente            | ausente                     | contém só uma URL — quebrado                      |
| AutoHistory                 | —                  | ausente            | ausente                     | contém só uma URL — quebrado                      |

### Gaps de automação identificados

- Commitizen está configurado em `.cz.toml` com `changelog_incremental = true` mas **não é executado no CI**.
- `build-release-notes.ps1` aceita apenas a variante `Não Lançado` via regex — qualquer CHANGELOG com `Unreleased` falha silenciosamente.
- `community-validation.yml` **não valida** o formato do `CHANGELOG.md` raiz.
- `pr-validation.yml` **não verifica** se o CHANGELOG do pacote alterado foi atualizado.
- `publish-release.yml` usa `--notes-file` (extrato manual) e `--generate-notes` (GitHub automático) simultaneamente, gerando risco de duplicação e divergência nas notas de release.
- Nenhum template formal de CHANGELOG está definido para IA ou humanos.

---

## Templates canônicos

### CHANGELOG raiz — fonte das GitHub Release Notes

O CHANGELOG raiz (`CHANGELOG.md` na raiz do repositório) resume mudanças transversais ao projeto e é a **fonte única** do texto das GitHub Releases. Deve seguir o formato abaixo.

```markdown
# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

### Alterado

### Corrigido

### Removido

### Segurança

### Performance

### Documentação

## [x.y.z] - yyyy-mm-dd

### Adicionado
- Descrição voltada ao consumidor.
```

### CHANGELOG de pacote — fonte do `PackageReleaseNotes` NuGet

Cada `src/<Pacote>/CHANGELOG.md` documenta o histórico daquele pacote específico. É embarcado automaticamente no `.nupkg` via `Directory.Build.props` e exposto no NuGet.org como release notes do pacote.

**Regra central:** o conteúdo deve responder à pergunta "o que o consumidor NuGet precisa saber para atualizar com segurança?".

```markdown
# Changelog - Nuuvify.CommonPack.<Pacote>

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

### Alterado

### Corrigido

### Removido

### Segurança

## [x.y.z] - yyyy-mm-dd

### Adicionado
- Descrição curta voltada ao consumidor da API pública.
```

**Proibido em changelogs de pacote:**
- Seções `Planned Features`, `Aguardando`, `Current Version`, `Technical Debt`, `Observability & Debugging`
- Blocos de código longos com exemplos de uso (pertencem ao README)
- Roadmap de funcionalidades futuras (pertencem ao README ou issues)
- Conteúdo em inglês (manter pt-BR)
- Histórico de alterações internas sem impacto público

**Obrigatório ao registrar breaking change:**
- Descrever o impacto
- Indicar o guia de migração (ou incluí-lo diretamente)
- Identificar qual pacote é afetado

---

## Fases de execução

### Dependências entre fases

```
Fase 1 ──────────────────────────────────┐
   │                                     │
   ├──► Fase 2 (corrigir arquivos)        ├──► Fase 4 (Commitizen + fallback)
   │                                     │         │
   └──► Fase 3 (CI validação)  ──────────┘         ├──► Fase 5 (pacotes via IA)
             │                                     │
             └─────────────────────────────────────┴──► Fase 6 (unificar release notes)

Fase 3c pode ser executada imediatamente, independente de qualquer outra.
```

---

### Fase 1 — Definir o contrato único

**Objetivo:** estabelecer o formato canônico antes de corrigir ou automatizar qualquer arquivo.

**Entregáveis:**
- Atualizar `.github/instructions/package-docs.instructions.md` com os templates acima, as categorias permitidas e as seções proibidas.
- Atualizar `.github/prompts/update-package-docs.prompt.md` com os templates inline de ambas as variantes (raiz e pacote), para que a IA nunca improvise o formato.

**Verificação:** a IA consegue atualizar o CHANGELOG de qualquer pacote sem instruções extras além do template da instruction, produzindo conteúdo no formato correto.

---

### Fase 2 — Corrigir CHANGELOGs existentes

**Objetivo:** levar todos os arquivos ao formato canônico definido na Fase 1.

Executar em dois PRs separados:

#### PR A — Grupo reconstrução zero

Pacotes: Security, Extensions, HealthCheck, AutoHistory.

Ação: substituir o conteúdo inteiro pelo template mínimo — cabeçalho padrão + `## [Não Lançado]` vazio. Nenhum histórico inventado.

#### PR B — Grupos de conversão e tradução

| Grupo | Pacotes                                      | Ação                                                                                                                                             |
| ----- | -------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| B     | AzureServiceBus, AzureServiceBus.Abstraction | Converter `## 2025-10-13` → `## [x.y.z] - yyyy-mm-dd` usando versão vigente em `Directory.Build.props`. Adicionar `## [Não Lançado]` no topo.    |
| C     | UnitOfWork, UnitOfWork.Abstraction           | Igual ao grupo B. Remover seção `Aguardando` (mover para issue ou README).                                                                       |
| D     | BackgroundService                            | Traduzir seções: `Added` → `Adicionado`, `Changed` → `Alterado`, `Fixed` → `Corrigido`, `Security` → `Segurança`. Manter conteúdo integralmente. |
| E     | Email                                        | Traduzir cabeçalho e seções para pt-BR. Mover `Planned Features` para `README.md` do pacote. Manter histórico real.                              |

**Regra de versão histórica:** quando não há versão conhecida para uma entrada de data, usar `## [2.4.0] - yyyy-mm-dd` com a versão mais recente publicada naquele período, ou `## [Sem versão registrada] - yyyy-mm-dd` se não for possível inferir.

**Verificação:** `dotnet pack` de cada pacote gera `PackageReleaseNotes` com conteúdo real (não URL, não vazio).

---

### Fase 3 — Validação automática no CI

**Objetivo:** impedir que o repositório retroceda ao estado inconsistente após as correções da Fase 2.

#### 3a — Validar formato do CHANGELOG raiz

Arquivo: `.github/workflows/community-validation.yml`

Adicionar step PowerShell com as seguintes verificações:

```powershell
$content = Get-Content -Path 'CHANGELOG.md' -Raw

# Seção aberta obrigatória
if ($content -notmatch '## \[Não Lançado\]') {
    Write-Error "CHANGELOG.md não contém a seção '## [Não Lançado]'"
}

# Pelo menos uma release fechada
if ($content -notmatch '## \[\d+\.\d+\.\d+\] - \d{4}-\d{2}-\d{2}') {
    Write-Error "CHANGELOG.md não contém nenhuma release no formato '## [x.y.z] - yyyy-mm-dd'"
}

# Seções proibidas
$forbidden = @('Planned Features', 'Aguardando', 'Current Version', 'Technical Debt')
foreach ($section in $forbidden) {
    if ($content -match [regex]::Escape("### $section")) {
        Write-Error "CHANGELOG.md contém seção proibida: '### $section'"
    }
}
```

#### 3b — Bloquear PR sem CHANGELOG de pacote atualizado

Arquivo: `.github/workflows/pr-validation.yml`

Adicionar job ou step que:
1. Detecta quais diretórios `src/<Pacote>/` tiveram arquivos `.cs` ou `.csproj` modificados no PR (via `git diff --name-only origin/$BASE_BRANCH`).
2. Para cada pacote detectado: verifica se `src/<Pacote>/CHANGELOG.md` está entre os arquivos modificados.
3. Se não estiver: **falha com `exit 1`** — para todos os pacotes, sem exceção.
4. A mensagem de erro nomeia exatamente qual pacote não foi atualizado.

#### 3c — Tornar `build-release-notes.ps1` robusto (executar imediatamente)

Arquivo: `.github/scripts/build-release-notes.ps1`

Alterar o regex da função `Get-ChangelogExcerpt` de:
```powershell
$match = [regex]::Match($content, '## \[Não Lançado\](?<section>[\s\S]*?)(\r?\n## \[|$)')
```
Para:
```powershell
$match = [regex]::Match($content, '## \[(Não Lançado|Unreleased)\](?<section>[\s\S]*?)(\r?\n## \[|$)')
```

Após a Fase 2 estar concluída e todos os arquivos estarem em pt-BR: reverter para o regex original restritivo.

**Verificação 3a:** PR que remove `## [Não Lançado]` do changelog raiz falha em `community-validation.yml`.
**Verificação 3b:** PR alterando `src/Email/**` sem tocar `src/Nuuvify.CommonPack.Email/CHANGELOG.md` falha em `pr-validation.yml` com mensagem identificando o pacote.
**Verificação 3c:** `build-release-notes.ps1` funciona sem erros com arquivo contendo `Unreleased`.

---

### Fase 4 — Automação do CHANGELOG raiz via Commitizen e fallback por labels

**Objetivo:** gerar entradas do CHANGELOG raiz automaticamente a partir dos commits convencionais, sem edição manual no momento do release.

**Pré-requisito humano:** todos os commits que tocam código de pacote devem usar Conventional Commits com scope de pacote obrigatório. Exemplos:

```
feat(email): adiciona suporte a múltiplos destinatários Bcc
fix(unitofwork): corrige paginação para provider DB2
refactor(backgroundservice): reduz complexidade cognitiva do HandleMessageAsync
docs(security): atualiza guia de migração JWT
```

#### 4a — Configurar `change_type_map` pt-BR no `.cz.toml`

```toml
[tool.commitizen]
# ... configurações existentes ...
changelog_file = "CHANGELOG.md"

[tool.commitizen.change_type_map]
feat = "Adicionado"
fix = "Corrigido"
refactor = "Alterado"
docs = "Documentação"
perf = "Performance"
"breaking change" = "Removido"
```

#### 4b — Step Commitizen em `publish-release.yml`

Adicionar antes do step `Build release notes`:

```yaml
- name: Generate changelog via Commitizen
  id: cz_changelog
  continue-on-error: true
  shell: bash
  run: |
      pip install commitizen
      cz changelog --incremental --file-name CHANGELOG.md
      if [ $? -ne 0 ] || [ -z "$(git diff --name-only CHANGELOG.md)" ]; then
        echo "cz_changed=false" >> $GITHUB_OUTPUT
      else
        echo "cz_changed=true" >> $GITHUB_OUTPUT
      fi

- name: Fallback changelog via PR labels
  if: ${{ steps.cz_changelog.outputs.cz_changed == 'false' }}
  shell: pwsh
  env:
      GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
      # Obtém data da última tag para filtrar PRs
      $lastTag = git tag --list 'v[0-9]*' | Sort-Object { [version]($_ -replace '^v','') } | Select-Object -Last 1
      $lastTagDate = git log -1 --format="%aI" $lastTag
      # Chama build-release-notes.ps1 no modo fallback de labels
      ./.github/scripts/build-release-notes.ps1 `
        -Mode 'labels-fallback' `
        -Version '${{ steps.version.outputs.version }}' `
        -LastTagDate $lastTagDate

- name: Commit changelog update
  if: ${{ steps.cz_changelog.outputs.cz_changed == 'true' }}
  shell: bash
  run: |
      git config user.name "github-actions[bot]"
      git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
      git add CHANGELOG.md
      git commit -m "docs(changelog): atualiza para v${{ steps.version.outputs.version }} [skip ci]"
      git push origin HEAD
```

#### 4c — Expandir `build-release-notes.ps1` com modo fallback por labels

O script recebe um parâmetro `-Mode` opcional:
- `default` (existente): extrai seção `[Não Lançado]` do arquivo
- `labels-fallback` (novo): consulta `gh pr list`, mapeia labels para categorias do `.github/release.yml`, insere bloco gerado na seção `[Não Lançado]` do CHANGELOG raiz

**Verificação:** merge em `main` com commits convencionais válidos gera e commita o CHANGELOG atualizado automaticamente. Merge com commits sem scope ativa o fallback por labels e produz notas coerentes.

---

### Fase 5 — Automação assistida dos CHANGELOGs de pacote

**Objetivo:** garantir que cada `src/<Pacote>/CHANGELOG.md` reflita as mudanças daquele pacote, com conteúdo orientado ao consumidor NuGet.

A geração por parsing de commits por scope é viável quando o scope está padronizado (garantido pelo pré-requisito da Fase 4). A abordagem adotada é **automação assistida por IA com validação CI obrigatória**:

1. O prompt `.github/prompts/update-package-docs.prompt.md` (reformulado na Fase 1) instrui explicitamente a atualizar o CHANGELOG do pacote com o template correto a cada mudança.
2. O CI da Fase 3b bloqueia o PR se o CHANGELOG não foi atualizado.
3. A IA gera o conteúdo conforme o template; o CI confirma que existe.

**Opcional para iteração futura:** script `.github/scripts/update-package-changelog.ps1` que:
1. Filtra commits convencionais por scope de pacote: `git log --pretty=format:"%s" v<ultima-tag>..HEAD | grep "^(feat|fix|refactor|docs|perf)(nome-do-pacote):"`.
2. Gera rascunho da seção `## [Não Lançado]` do CHANGELOG do pacote.
3. O desenvolvedor revisa e ajusta o conteúdo antes de commitar.

**Verificação:** a IA atualiza o CHANGELOG de um pacote com conteúdo conciso e voltado ao consumidor NuGet, sem roadmap, sem blocos de código extensos e no formato canônico.

---

### Fase 6 — Unificar a fonte das GitHub Release Notes

**Objetivo:** eliminar a duplicação entre `--notes-file` e `--generate-notes` no `publish-release.yml`.

**Problema atual:** o step `Create GitHub release` usa ambos simultaneamente:
```yaml
--notes-file ./artifacts/release-notes.md \
--generate-notes \
```
Isso cria duas seções nas notas de release, potencialmente com conteúdo sobrepostoo.

**Solução:**
1. Remover `--generate-notes` do step `Create GitHub release` em `publish-release.yml`.
2. Expandir `build-release-notes.ps1` para gerar um arquivo completo e autossuficiente:
   - Cabeçalho com versão, canal e branch
   - Extrato do CHANGELOG raiz correspondente à versão publicada (seção que estava em `[Não Lançado]`)
   - Lista de PRs mergeados agrupados por categoria de label (já gerada pelo fallback da Fase 4)

**Verificação:** a GitHub Release exibe exatamente um bloco de notas sem itens duplicados. O conteúdo é rastreável ao CHANGELOG raiz e aos PRs mergeados.

---

## Tabela de arquivos por fase

| Arquivo                                                           | Ação                                                                    | Fase   |
| ----------------------------------------------------------------- | ----------------------------------------------------------------------- | ------ |
| `.github/instructions/package-docs.instructions.md`               | Adicionar templates canônicos, categorias permitidas e seções proibidas | 1      |
| `.github/prompts/update-package-docs.prompt.md`                   | Adicionar templates inline de CHANGELOG raiz e de pacote                | 1      |
| `src/Nuuvify.CommonPack.Security/CHANGELOG.md`                    | Reconstrução do zero                                                    | 2-A    |
| `src/Nuuvify.CommonPack.Extensions/CHANGELOG.md`                  | Reconstrução do zero                                                    | 2-A    |
| `src/Nuuvify.CommonPack.HealthCheck/CHANGELOG.md`                 | Reconstrução do zero                                                    | 2-A    |
| `src/Nuuvify.CommonPack.AutoHistory/CHANGELOG.md`                 | Reconstrução do zero                                                    | 2-A    |
| `src/Nuuvify.CommonPack.AzureServiceBus/CHANGELOG.md`             | Converter data pura → versão; adicionar `[Não Lançado]`                 | 2-B    |
| `src/Nuuvify.CommonPack.AzureServiceBus.Abstraction/CHANGELOG.md` | Idem                                                                    | 2-B    |
| `src/Nuuvify.CommonPack.UnitOfWork/CHANGELOG.md`                  | Converter + remover roadmap                                             | 2-C    |
| `src/Nuuvify.CommonPack.UnitOfWork.Abstraction/CHANGELOG.md`      | Idem                                                                    | 2-C    |
| `src/Nuuvify.CommonPack.BackgroundService/CHANGELOG.md`           | Traduzir seções para pt-BR                                              | 2-D    |
| `src/Nuuvify.CommonPack.Email/CHANGELOG.md`                       | Traduzir + mover roadmap para README                                    | 2-E    |
| `.github/workflows/community-validation.yml`                      | Adicionar validação de formato do CHANGELOG raiz                        | 3a     |
| `.github/workflows/pr-validation.yml`                             | Adicionar bloqueio de PR sem CHANGELOG de pacote                        | 3b     |
| `.github/scripts/build-release-notes.ps1`                         | Aceitar `Unreleased` no regex (transição)                               | 3c     |
| `.cz.toml`                                                        | Adicionar `change_type_map` com traduções pt-BR                         | 4a     |
| `.github/workflows/publish-release.yml`                           | Adicionar steps Commitizen, fallback e commit do changelog              | 4b     |
| `.github/scripts/build-release-notes.ps1`                         | Adicionar modo `labels-fallback` e geração completa                     | 4c + 6 |
| `.github/workflows/publish-release.yml`                           | Remover `--generate-notes`                                              | 6      |

---

## Checklist de verificação por fase

| Fase      | Critério de conclusão                                                                                                                      |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| 1         | IA atualiza CHANGELOG de pacote sem instruções adicionais, no formato correto                                                              |
| 2-A       | `dotnet pack` dos 4 pacotes gera `PackageReleaseNotes` não vazio e sem URL                                                                 |
| 2-B a 2-E | Todos os CHANGELOGs têm `## [Não Lançado]` em pt-BR e pelo menos uma entrada `## [x.y.z] - yyyy-mm-dd`                                     |
| 3a        | PR que remove `## [Não Lançado]` do CHANGELOG raiz falha em `community-validation.yml`                                                     |
| 3b        | PR alterando `src/Email/**` sem atualizar `Email/CHANGELOG.md` falha em `pr-validation.yml` com nome do pacote na mensagem                 |
| 3c        | `build-release-notes.ps1` processa arquivos com `Unreleased` sem erro                                                                      |
| 4         | Merge em `main` com commits convencionais gera commit automático do CHANGELOG; merge sem commits convencionais ativa o fallback por labels |
| 5         | IA produz CHANGELOG de pacote com conteúdo conciso, sem roadmap e no template canônico                                                     |
| 6         | GitHub Release exibe um único bloco de notas sem duplicação                                                                                |

---

## Referências

- [Processo de release](./release-process.md)
- [Arquitetura do repositório](./architecture.md)
- [`.github/instructions/package-docs.instructions.md`](../../.github/instructions/package-docs.instructions.md)
- [`.github/scripts/build-release-notes.ps1`](../../.github/scripts/build-release-notes.ps1)
- [`.github/workflows/publish-release.yml`](../../.github/workflows/publish-release.yml)
- [`.cz.toml`](../../.cz.toml)
