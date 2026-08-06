# Plano de Correcao dos Comentarios do PR #210

## Gestao de Status

| Campo | Valor |
|---|---|
| Status | Em andamento |
| Criado em | 2026-08-06 |
| Atualizado em | 2026-08-06 |
| Responsavel | Time mantenedor |
| Ultima revisao | 2026-08-06 |

### Historico de Status

| Data | De -> Para | Motivo |
|---|---|---|
| 2026-08-06 | N/A -> Em andamento | Plano consolidado para execucao do PR |

## Contexto

- PR: https://github.com/nuuvify/Nuuvify.CommonPack/pull/210
- Escopo observado: novos pacotes MFT Mailbox (Core/Abstraction/SFTP/HTTP/Redis), ajustes de tooling e documentacao.
- Objetivo deste plano: resolver os comentarios de review com foco em risco tecnico, preservar compatibilidade publica e fechar threads com validacao objetiva.

## Premissas de execucao

- Branch de trabalho sincronizada com o PR.
- Ferramentas locais:
  - `dotnet` (build/test)
  - `uv` (scripts em `tools/scripts`)
- Token GitHub em uma destas variaveis:
  - `GITHUB_LZOCATELI_TOKEN` (preferencial)
  - `GH_TOKEN`
  - `GITHUB_TOKEN`

## Coleta de comentarios

### Modo autenticado (recomendado)

```powershell
uv run --project tools/scripts get-github-pr-comments --pr-url https://github.com/nuuvify/Nuuvify.CommonPack/pull/210 --open-threads-only
```

### Fallback sem token (somente leitura publica)

```powershell
Invoke-RestMethod -Uri "https://api.github.com/repos/nuuvify/Nuuvify.CommonPack/pulls/210/comments?per_page=100" -Headers @{"User-Agent"="copilot-agent"}
```

## Priorizacao

### P0 - Corrigir risco de build e carga da solucao

1. Corrigir valor de `OsPlatform` com espaco indevido em `src/Directory.Build.props`.
2. Corrigir GUID invalido em `Nuuvify.CommonPack.sln` e garantir consistencia em todas as ocorrencias.

Criterio de aceite:

- `dotnet build Nuuvify.CommonPack.sln` sem erro.
- Solucao abre normalmente no IDE.

### P1 - Corrigir comportamento funcional e DI

1. Ajustar mensagem de orientacao de registro Redis em `src/Nuuvify.CommonPack.MftMailbox.Redis/RedisMftMailboxSetup.cs` para nao induzir uso de `AddStackExchangeRedisCache` como registro de `IConnectionMultiplexer`.
2. Revisar decoracao de `IMftStatusClient` no mesmo arquivo para garantir aplicacao real e evitar caminho com `ImplementationType == null`.

Criterio de aceite:

- Registro DI consistente para `type`, `factory` e `instance`.
- Nao ocorrer `InvalidOperationException` por orientacao incorreta do setup.

### P1 - Corrigir lifecycle de IDisposable

Arquivo: `src/Nuuvify.CommonPack.MftMailbox.Sftp/SftpMftMailboxClient.cs`

1. Resolver objetos descartaveis sinalizados:
   - `PrivateKeyFile` (2 ocorrencias)
   - `PrivateKeyAuthenticationMethod`
   - `PasswordConnectionInfo`
2. Garantir descarte tambem em fluxo com excecao.

Criterio de aceite:

- Sem findings de disposable leak nesse arquivo.

### P2 - Endurecer tratamento de excecoes

Arquivos:

- `src/Nuuvify.CommonPack.MftMailbox.Http/HttpMftMailboxClient.cs`
- `src/Nuuvify.CommonPack.MftMailbox.Redis/Services/RedisAuditStreamSink.cs`
- `src/Nuuvify.CommonPack.MftMailbox.Redis/Services/RedisCachedStatusClient.cs`

Acoes:

1. Substituir `catch` generico por excecoes especificas quando possivel.
2. Quando mantido catch amplo, justificar com log estruturado e rethrow/resultado seguro.

Criterio de aceite:

- Nao ocultar falhas criticas.
- Mensagens e telemetria suficientes para diagnostico.

### P2 - Alinhar documentacao tecnica com implementacao

Arquivo: `src/Nuuvify.CommonPack.MftMailbox.Redis/Services/RedisAuditStreamSink.cs`

1. Corrigir XML docs para refletir comportamento real de `WriteAsync` (atualmente aguardado/await).

Criterio de aceite:

- Documentacao sem conflito semantico com o codigo.

### P3 - Padronizacao e legibilidade

1. Aplicar ajuste de ternario em:
   - `src/Nuuvify.CommonPack.MftMailbox/MftMailboxSetup.cs`
   - `src/Nuuvify.CommonPack.MftMailbox.Redis/RedisMftMailboxSetup.cs`
2. Atualizar documentacao de pacote:
   - `src/Nuuvify.CommonPack.MftMailbox/README.md` com secao de troubleshooting.
3. Padronizar changelogs de pacote para o formato canonico do repositorio:
   - `src/Nuuvify.CommonPack.MftMailbox/CHANGELOG.md`
   - `src/Nuuvify.CommonPack.MftMailbox.Redis/CHANGELOG.md`

Criterio de aceite:

- README com troubleshooting minimo acionavel.
- CHANGELOGs com secoes permitidas e texto em pt-BR.

## Mapeamento de comentarios -> acao

- `src/Directory.Build.props` (Copilot): corrigir `OsPlatform` com espaco.
- `Nuuvify.CommonPack.sln` (Copilot): corrigir GUID invalido.
- `src/Nuuvify.CommonPack.MftMailbox.Redis/RedisMftMailboxSetup.cs` (Copilot): setup Redis e decoracao `IMftStatusClient`.
- `src/Nuuvify.CommonPack.MftMailbox.Sftp/SftpMftMailboxClient.cs` (github-code-quality): disposables.
- `src/Nuuvify.CommonPack.MftMailbox.Http/HttpMftMailboxClient.cs` (github-code-quality): generic catch.
- `src/Nuuvify.CommonPack.MftMailbox.Redis/Services/RedisAuditStreamSink.cs` (github-code-quality + Copilot): generic catch e XML docs.
- `src/Nuuvify.CommonPack.MftMailbox.Redis/Services/RedisCachedStatusClient.cs` (github-code-quality): generic catch.
- `src/Nuuvify.CommonPack.MftMailbox/README.md` (Copilot): troubleshooting.
- `src/Nuuvify.CommonPack.MftMailbox/CHANGELOG.md` (Copilot): formato canonico.
- `src/Nuuvify.CommonPack.MftMailbox.Redis/CHANGELOG.md` (Copilot): formato canonico.
- `CHANGELOG.md` (Copilot): alinhar descricao de escopo da release/PR.

## Estrategia de execucao em lotes

### Lote A (bloqueadores)

- P0 completo.
- Validar build da solucao.

### Lote B (funcional e recursos)

- P1 completo (DI + IDisposable).
- Rodar validacao focada dos pacotes MFT Mailbox.

### Lote C (robustez e docs)

- P2 e P3.
- Revisar README/CHANGELOGs de pacote.

### Lote D (encerramento do PR)

1. Recoletar comentarios abertos:

```powershell
uv run --project tools/scripts get-github-pr-comments --pr-url https://github.com/nuuvify/Nuuvify.CommonPack/pull/210 --open-threads-only
```

2. Responder/fechar thread por thread:

```powershell
uv run --project tools/scripts update-github-pr-comments --pr-url https://github.com/nuuvify/Nuuvify.CommonPack/pull/210 --thread-id <THREAD_ID_GRAPHQL> --comment "Corrigido em <commit>." --close-thread
```

## Checklist operacional

- [ ] Build da solucao apos Lote A
- [ ] Validacao funcional DI apos Lote B
- [ ] Verificacao de findings de IDisposable
- [ ] Revisao de catches genericos
- [ ] XML docs consistentes com comportamento real
- [ ] README de pacote com troubleshooting
- [ ] CHANGELOGs no formato canonico
- [ ] Recoleta de comentarios abertos
- [ ] Threads respondidas e resolvidas

## Riscos e mitigacoes

- Risco: alteracao ampla de DI introduzir regressao em composicao.
  - Mitigacao: validar com cenarios minimos por adaptador (SFTP/HTTP/Redis).
- Risco: correcoes de catch mascararem erro real.
  - Mitigacao: manter log estruturado e nao suprimir excecao sem contrato explicito.
- Risco: documentacao divergente do codigo apos ajustes finais.
  - Mitigacao: revisar README/CHANGELOG no ultimo lote, apos merge do codigo.

## Observacao de manutencao

Este plano deve ser tratado como baseline de execucao para o PR #210. Se surgirem novos comentarios durante a revisao, adicionar no mapeamento e replanejar por lote sem quebrar a ordem de prioridade.
