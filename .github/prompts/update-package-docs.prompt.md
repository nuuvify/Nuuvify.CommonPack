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
