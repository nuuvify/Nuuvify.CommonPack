---
description: "Use when updating README, CHANGELOG, package docs, contribution docs, or release notes for Nuuvify.CommonPack. Covers concise package documentation, public API communication, migration notes, and token-efficient AI-generated docs."
name: "Nuuvify Package Docs"
applyTo: "Readme.md, docs/**/*.md, src/**/README.md, src/**/CHANGELOG.md"
---

# Diretrizes para documentação

- Documente comportamento público, pré-requisitos, exemplos e limites relevantes; omita detalhes internos sem valor para o consumidor.
- Em `README.md` de pacote, priorize: propósito, instalação, configuração mínima, exemplo de uso e observações de compatibilidade.
- Em `CHANGELOG.md`, registre apenas o que um consumidor precisa saber para atualizar com segurança.
- Se houver breaking change, inclua impacto, migração e pacote afetado.
- Use linguagem direta, exemplos curtos e blocos de código mínimos.
- Não duplique explicações extensas já cobertas em outro arquivo; prefira apontar o local certo.

## Documentação produzida com IA

- Reduza texto genérico e marketing excessivo.
- Preserve consistência de termos entre `Readme.md`, `docs/` e documentação dos pacotes.
- Quando a mudança for local a um pacote, atualize o `README.md` e o `CHANGELOG.md` daquele pacote antes de tocar documentação global.

---

## Formato canônico do CHANGELOG

### CHANGELOG raiz (`CHANGELOG.md` na raiz do repositório)

Fonte única das GitHub Release Notes. Use exatamente este template:

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

### CHANGELOG de pacote (`src/<Pacote>/CHANGELOG.md`)

Embarcado no `.nupkg` via `Directory.Build.props`. Use exatamente este template:

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

### Categorias permitidas em CHANGELOG de pacote

`Adicionado`, `Alterado`, `Corrigido`, `Removido`, `Segurança`

### Seções PROIBIDAS em CHANGELOGs de pacote

- `Planned Features`, `Aguardando`, `Current Version`, `Technical Debt`
- `Observability & Debugging`, `Roadmap`, `Known Issues`
- Blocos de código longos com exemplos de uso (pertencem ao README)
- Roadmap de funcionalidades futuras (pertencem ao README ou issues)
- Conteúdo em inglês (manter pt-BR em todo o arquivo)
- Histórico de alterações internas sem impacto público

### Regras obrigatórias ao registrar breaking change

1. Descrever o impacto para o consumidor.
2. Indicar o guia de migração (ou incluí-lo diretamente, de forma concisa).
3. Identificar qual pacote é afetado.

### Regra de idioma

Todos os CHANGELOGs (raiz e de pacote) devem estar inteiramente em **pt-BR**. Seções, categorias e conteúdo — sem exceção.

---

## Alinhamento com o processo de release

O CHANGELOG está diretamente integrado ao `prepare-release.yml` e ao `build-release-notes.ps1`. Respeite as responsabilidades abaixo — violar qualquer uma pode abortar a publicação.

| Momento | Ator | Arquivo | Ação |
|---|---|---|---|
| Durante desenvolvimento | Humano / IA | `CHANGELOG.md` (raiz) e `src/pkg/CHANGELOG.md` | Adicionar itens em `## [Não Lançado]` |
| Abertura do release PR | 🤖 `prepare-release.yml` | `CHANGELOG.md` (raiz) | Fecha `[Não Lançado]` → `## [X.Y.Z] - yyyy-mm-dd` |
| Publicação stable | 🤖 `build-release-notes.ps1` | `CHANGELOG.md` (raiz) | Lê a seção fechada `[X.Y.Z]` para as release notes |
| Publicação preview/dev | 🤖 `build-release-notes.ps1` | `CHANGELOG.md` (raiz) | Lê `[Não Lançado]` para as release notes |

### Regras derivadas

- **Sempre adicione a `[Não Lançado]`** — nunca crie manualmente uma seção `[X.Y.Z]`; isso é responsabilidade exclusiva do `prepare-release.yml`.
- Atualize **ambos** os changelogs na mesma alteração de código: o raiz **e** o do pacote afetado.
- O CHANGELOG raiz é a **fonte das GitHub Release Notes**; omita detalhes internos sem impacto para o consumidor externo.
- O CHANGELOG de pacote é **embarcado no `.nupkg`** via `Directory.Build.props`; mantenha-o conciso e orientado ao consumidor NuGet.
- Se `[Não Lançado]` estiver vazio ou sem itens válidos no momento do publish, o `build-release-notes.ps1` aborta a publicação — garanta pelo menos um item antes de mergear em qualquer canal de release.
- O `PR Validation / changelog-check` bloqueia o PR se o CHANGELOG do pacote alterado não for atualizado — não tente mergear sem corrigir.
