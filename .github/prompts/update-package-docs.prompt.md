---
description: "Atualizar README, CHANGELOG ou documentação de pacote Nuuvify.CommonPack após mudanças de comportamento ou API pública."
name: "Update Package Docs"
argument-hint: "Pacote, mudança feita, público-alvo, nível de detalhe"
agent: "agent"
model: ["Claude Sonnet 4.5 (copilot)", "GPT-5 (copilot)"]
---

Atualize a documentação necessária para a mudança informada.

Objetivos:
- Explicar apenas o que o consumidor do pacote precisa saber.
- Manter exemplos curtos e corretos.
- Registrar impacto de upgrade quando houver breaking change.
- Evitar texto genérico e duplicação entre arquivos.

Se a documentação estiver concentrada em `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote correspondente em `.github/prompts/`.

Se a documentação do pacote estiver incompleta, use o código e os testes como fonte primária antes de escrever texto novo.

Considere, conforme o caso:
- `src/<Pacote>/README.md`
- `src/<Pacote>/CHANGELOG.md`
- `Readme.md`
- `docs/`

Use também:
- [Nuuvify Package Docs](../instructions/package-docs.instructions.md)
- [Nuuvify Package README NuGet](../instructions/package-readme-nuget.instructions.md)

Para criação/reescrita de README de pacote no padrão nuget.org, prefira:
- [Create Package README NuGet](./create-package-readme-nuget.prompt.md)

---

## Templates canônicos de CHANGELOG

Ao atualizar qualquer `CHANGELOG.md`, use **exatamente** um dos templates abaixo — nunca improvise o formato.

### CHANGELOG raiz

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

### CHANGELOG de pacote

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

### Regras inegociáveis

- Categorias permitidas: `Adicionado`, `Alterado`, `Corrigido`, `Removido`, `Segurança`.
- **NUNCA** adicionar: `Planned Features`, `Aguardando`, `Current Version`, `Technical Debt`, `Roadmap`, `Known Issues`, `Observability & Debugging`.
- **NUNCA** escrever em inglês — todo o conteúdo deve estar em pt-BR.
- **NUNCA** incluir blocos de código extensos, exemplos de uso ou explicações técnicas internas (pertencem ao README).
- Cada item deve responder: "o que o consumidor NuGet precisa saber para atualizar com segurança?"

---

## Fluxo de atualização de changelog

Siga estas regras sempre que atualizar qualquer `CHANGELOG.md` neste repositório.

### Responsabilidade por seção

| Seção | Criada por | Quando |
|---|---|---|
| `## [Não Lançado]` | Humano / IA (você) | A cada mudança de comportamento ou API |
| `## [X.Y.Z] - yyyy-mm-dd` | 🤖 `prepare-release.yml` | Ao disparar o release PR via `workflow_dispatch` |

**Nunca crie uma seção `[X.Y.Z]` manualmente.** O workflow `prepare-release.yml` fecha `[Não Lançado]` para `[X.Y.Z]` automaticamente ao abrir o PR de release.

### Quais arquivos atualizar

Sempre que houver mudança de comportamento ou API pública em um pacote, atualize **os dois arquivos no mesmo commit**:

1. `CHANGELOG.md` (raiz do repositório) — entry visível no GitHub Release e nos release notes.
2. `src/<Pacote>/CHANGELOG.md` — entry embarcada no `.nupkg` publicado no NuGet.

Se a mudança afetar mais de um pacote, atualize o `CHANGELOG.md` de **cada** pacote afetado.

### Bloqueios de CI que você precisa evitar

- `PR Validation / changelog-check` — falha se o CHANGELOG do pacote alterado não foi atualizado. Corrija antes de mergear.
- `publish-release.yml` — aborta se `[Não Lançado]` estiver vazio no momento do publish. Garanta pelo menos um item antes de mergear em canal de release.

### Resumo da ação esperada

```
Você alterou src/Nuuvify.CommonPack.<Pacote>/
  → adicione um item em ## [Não Lançado] de src/<Pacote>/CHANGELOG.md
  → adicione um item em ## [Não Lançado] de CHANGELOG.md (raiz)
  → NÃO crie seção [X.Y.Z] — isso é feito por prepare-release.yml
```
