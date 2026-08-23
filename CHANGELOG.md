# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

- Nova extensão `AddContainerSecrets` no pacote `Nuuvify.CommonPack.Middleware` para secrets montados via `KeyPerFile`.
- Construtores canônicos baseados em `IOperationContextAccessor` para adapters HTTP, repositórios e processamento de mensagens.
- Esquema de autenticação `ApiKey` com claim canônica e integração opt-in ao pipeline ASP.NET Core.

### Alterado

- Repositórios e adapters migrados para correlation por operação ou mensagem, sem estado global mutável.
- APIs legadas de configuração, validação e arquivos passaram a emitir avisos de obsolescência não bloqueantes, mantendo compatibilidade na linha 2.x.

### Corrigido

- Carregamento silenciosamente vazio de variáveis de ambiente no método legado do pacote `Nuuvify.CommonPack.Middleware`.

### Removido

### Segurança

- Eliminada a persistência temporária de secrets em arquivos locais pelo pacote `Nuuvify.CommonPack.Middleware`.
- O filtro legado de API key emite simultaneamente as claims legada e canônica durante a transição.

### Performance

### Documentação

- Guia transversal de migração criado em `docs/middleware-migration.md`.

## [2.8.0] - 2026-08-06

### Adicionado

- Suporte a `ReceiveMode` em componentes de consumo do Azure Service Bus (`Nuuvify.CommonPack.AzureServiceBus` e `Nuuvify.CommonPack.BackgroundService`).
- Suporte a CNPJ alfanumérico no pacote `Nuuvify.CommonPack.Domain`.
- Suporte opt-in a `IUnitOfWorkFactory<TContext>` no pacote `Nuuvify.CommonPack.UnitOfWork` para criação de `UnitOfWork` curto por operação com `IDbContextFactory<TContext>`.
- Suporte opt-in a `IShortLivedDbContextFactory<TContext>` e `IWorkerDbContextFactory<TContext>` no pacote `Nuuvify.CommonPack.UnitOfWork` para criação de `DbContext` curto com auditoria em cenários de worker/background.
- Novos pacotes para integração MFT Mailbox: `Nuuvify.CommonPack.MftMailbox.Abstraction`, `Nuuvify.CommonPack.MftMailbox`, `Nuuvify.CommonPack.MftMailbox.Sftp` e `Nuuvify.CommonPack.MftMailbox.Http`.
- Novo pacote para integração MFT Mailbox com Redis distribuído: `Nuuvify.CommonPack.MftMailbox.Redis`.
- Implementação de fluxos bidirecionais com envio/recebimento por streaming, consulta de status, ACK/NACK, idempotência e auditoria para SFTP e HTTPS Mailbox.

### Alterado

- Política de versionamento centralizada: `VersionPrefix` em `src/Directory.Build.props` como única fonte de versão base; `AssemblyVersion` estabilizada por major (`X.0.0.0`); `FileVersion` e `InformationalVersion` derivados deterministicamente pelo CI.
- Commitizen removido: `.cz.toml` excluído; bump de versão agora é explícito via workflow `prepare-release.yml` (workflow_dispatch).
- CI/CD migrado integralmente para GitHub Actions, com validação de PR, empacotamento, artefatos, publicação por canal e GitHub Releases.
- Geração de Id em `DomainEntity` no pacote `Nuuvify.CommonPack.Extensions` alterada para UUID orientado a banco de dados (UUID v7).
- Fluxo de processamento de mensagens ajustado para evitar operações de settlement (`Complete`, `Abandon`, `DeadLetter`) quando `ReceiveMode` for `ReceiveAndDelete`.
- Scripts e organização de execução de testes revisados para melhorar seleção por traits e execução local/CI.
- Setup Redis do MFT Mailbox ajustado para orientar registro correto de `IConnectionMultiplexer` e aplicar decoração de `IMftStatusClient` com suporte a registros por tipo, factory e instância.

### Corrigido

- Tratamento de exceções em `ReceiveAndDelete` ajustado para evitar falhas secundárias ao processar mensagens já removidas da fila.
- Validação do CNPJ reforçada para rejeitar entradas inválidas (formato, tamanho e repetição total de caracteres).
- `TokenService.GetToken` no pacote `Nuuvify.CommonPack.StandardHttpClient` agora resolve `ClientId`/`ClientSecret` também a partir de chaves planas com separador `--` (ex.: `AzureAdOpenID--cc--ClientId`), cobrindo cenários em que segredos do Key Vault são carregados via `AddKeyPerFile` sem tradução de `--` para `:`. Isso corrige falha de obtenção de token em workers/produção.
- Lifecycle de objetos descartáveis no cliente SFTP do MFT Mailbox corrigido para evitar vazamento de recursos de autenticação.
- Tratamento de exceções no cliente HTTP e nos serviços Redis do MFT Mailbox ajustado para não ocultar falhas críticas e melhorar diagnóstico.

### Removido

### Segurança

### Performance

- Adoção de UUID v7 em `DomainEntity` para melhor ordenação de inserções e menor probabilidade de page split em banco de dados.

### Documentação

- Padronização dos `CHANGELOG.md` dos pacotes e atualização dos changelogs de `Domain`, `AzureServiceBus`, `BackgroundService` e `Extensions` com alterações recentes.
- Atualização de documentação de mantenedores e contribuição (onboarding, setup e processo de release).
- Criação do guia central de consumo em projetos de terceiros em `docs/consumo-em-projetos-terceiros.md`, com onboarding, matriz de pacotes, configuração e exemplos de integração.
- Atualização do README do pacote `Nuuvify.CommonPack.MftMailbox` com seção de troubleshooting e revisão dos changelogs de `Nuuvify.CommonPack.MftMailbox` e `Nuuvify.CommonPack.MftMailbox.Redis` para formato canônico.

## [2.6.0] - 2025-11-01

### 🆕 Adicionado

- **Suporte completo a filtros com tipos complexos** usando QueryOperatorAttribute
- **Navegação em propriedades aninhadas** com notação de ponto
- **Operadores especializados** para listas e coleções
- **Validação automática** de estruturas de propriedades

### ⚡ Performance

- **Compiled queries** para filtros frequentemente utilizados
- **Parameterização automática** de valores constantes
- **Cache de reflection** para metadata de propriedades
- **Otimização de expressões** LINQ dinâmicas

### 📚 Documentação

- Exemplos completos de filtros complexos
- Guias de performance e melhores práticas
- Documentação de troubleshooting
- Referência completa de operadores

## [2.5.0] - 2025-10-15

### 🆕 Adicionado

- **BackgroundService v2.0** com diagnóstico avançado
- **Propriedades contextuais** para Dead Letter Queue e Abandon
- **Arquitetura modular** refatorada com menor complexidade cognitiva
- **Observabilidade avançada** para monitoramento

### 🔧 Melhorado

- **Retry policies** mais robustas
- **Logging estruturado** aprimorado
- **Health checks** mais detalhados
- **Documentação técnica** expandida

## [2.4.0] - 2025-09-20

### 🆕 Adicionado

- **Email Service** com suporte a SMTP via MailKit
- **Templates HTML** para e-mails
- **Anexos** (file e stream)
- **Múltiplos destinatários** (To, Cc, Bcc)
- **Retry logic** integrado

### 🔧 Melhorado

- **StandardHttpClient** com resiliência aprimorada
- **Configurações flexíveis** para diferentes provedores
- **Testes unitários** expandidos
- **Documentação** com exemplos práticos

## [2.3.0] - 2025-08-10

### 🆕 Adicionado

- **Dynamic Queries** para UnitOfWork
- **Expressões LINQ** construídas dinamicamente
- **Type-safe filters** com atributos
- **Paginação avançada** com PagedList

### ⚡ Performance

- **Query compilation** automática
- **Expression caching** para consultas repetidas
- **Otimizações** específicas do Entity Framework
- **Benchmark suite** para performance

### 🧪 Testes

- **12 projetos de teste** cobrindo todos os cenários
- **Testes de integração** com Testcontainers
- **Testes unitários** com InMemory providers
- **Cobertura de código** > 85%

## [2.2.0] - 2025-07-05

### 🆕 Adicionado

- **Azure Service Bus** client completo
- **Topic e Queue** support
- **Retry policies** configuráveis
- **Dead letter** handling
- **Batch operations**

### 🔒 Segurança

- **JWT Authentication** aprimorado
- **Azure AD** integration
- **Token refresh** automático
- **Security headers** middleware

## [2.1.0] - 2025-06-15

### 🆕 Adicionado

- **Azure Storage** client
- **Blob operations** otimizadas
- **File upload/download** com progress
- **SAS tokens** generation
- **CDN integration**

### 🏗️ Arquitetura

- **Clean Architecture** patterns
- **Domain-Driven Design** support
- **CQRS** base implementations
- **Event sourcing** abstractions

## [2.0.0] - 2025-05-01

### 💥 Breaking Changes

- **Migração para .NET 8.0**
- **Novos namespaces** organizacionais
- **API consistente** entre todos os pacotes
- **Dependency injection** nativa

### 🆕 Adicionado

- **Entity Framework exceptions** para DB2 e Oracle
- **Middleware collection** completa
- **OpenAPI** configurations
- **Health checks** customizados

### 🔧 Melhorado

- **Performance** geral dos pacotes
- **Memory allocation** reduzida
- **Async patterns** consistentes
- **Error handling** padronizado

## [1.5.0] - 2025-03-20

### 🆕 Adicionado

- **StandardHttpClient** com Polly
- **Retry strategies** configuráveis
- **Circuit breaker** patterns
- **Timeout management**
- **Request/Response** logging

### 📊 Observabilidade

- **Structured logging** com Serilog support
- **Metrics collection** com System.Diagnostics
- **Distributed tracing** ready
- **Performance counters**

## [1.4.0] - 2025-02-15

### 🆕 Adicionado

- **UnitOfWork pattern** completo
- **Repository abstraction** genérica
- **Transaction management**
- **Change tracking** otimizado
- **Bulk operations** support

### 🧪 Testes

- **Test framework** estabelecido
- **Mock patterns** padronizados
- **Integration testing** setup
- **Performance benchmarks**

## [1.3.0] - 2025-01-10

### 🆕 Adicionado

- **Domain entities** base classes
- **Value objects** abstractions
- **AutoHistory** para auditoria
- **Soft delete** support
- **Concurrency tokens**

### 📚 Documentação

- **README** completo por pacote
- **Code examples** práticos
- **Architecture** documentation
- **Best practices** guide

## [1.2.0] - 2024-12-05

### 🆕 Adicionado

- **Extensions collection** inicial
- **Notification pattern**
- **Validation helpers**
- **String extensions**
- **DateTime utilities**

### 🔧 Infraestrutura

- **CI/CD pipeline** com Azure DevOps
- **Automated testing**
- **NuGet publishing**
- **Code quality** gates

## [1.1.0] - 2024-11-01

### 🆕 Adicionado

- **Security abstractions**
- **JWT utilities**
- **Cryptography helpers**
- **Authentication middleware**

### 🐛 Corrigido

- **Dependency conflicts** resolvidos
- **Memory leaks** em alguns cenários
- **Thread safety** melhorado
- **Error messages** mais claros

## [1.0.0] - 2024-10-01

### 🎉 Lançamento Inicial

- **Projeto criado** com estrutura base
- **Package organization** estabelecida
- **Core abstractions** definidas
- **Initial NuGet** packages published

### 🏗️ Fundação

- **.NET Standard 2.1** e .NET 8.0 support
- **Microsoft.Extensions** integration
- **Entity Framework Core** ready
- **Dependency Injection** support

---

## Tipos de Mudanças

- 🆕 **Adicionado** para novas funcionalidades
- 🔧 **Alterado** para mudanças em funcionalidades existentes
- 🐛 **Corrigido** para correções de bugs
- 💥 **Removido** para funcionalidades removidas (Breaking Changes)
- 🔒 **Segurança** para correções de vulnerabilidades
- ⚡ **Performance** para melhorias de performance
- 📚 **Documentação** para mudanças apenas na documentação
- 🧪 **Testes** para adições ou mudanças em testes

## Versionamento

Este projeto segue [Semantic Versioning](https://semver.org/):

- **MAJOR** version para mudanças incompatíveis na API
- **MINOR** version para funcionalidades adicionadas de forma compatível
- **PATCH** version para correções de bugs compatíveis

## Links Relacionados

- [Releases](https://github.com/nuuvify/CommonPack/releases)
- [Issues](https://github.com/nuuvify/CommonPack/issues)
- [Pull Requests](https://github.com/nuuvify/CommonPack/pulls)
- [Milestones](https://github.com/nuuvify/CommonPack/milestones)
