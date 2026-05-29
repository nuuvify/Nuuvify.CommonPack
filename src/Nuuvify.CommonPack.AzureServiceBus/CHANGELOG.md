# Changelog - Nuuvify.CommonPack.AzureServiceBus

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

### Alterado

### Corrigido

### Removido

### Segurança

## [Sem versão registrada] - 2025-10-13

### Alterado
- Removido `#nullable enable` individualizado de todos os arquivos fonte; configuração centralizada no projeto.
- Dependências redundantes removidas (herdadas via projeto de abstração): `Azure.Messaging.ServiceBus`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.DependencyInjection.Abstractions`.
- `using` statements centralizados em `GlobalUsings.cs`.

### Adicionado
- Tags de `PackageTags` no `.csproj` para melhor descoberta no NuGet.

## [Sem versão registrada] - 2025-10-08

### Adicionado
- Release inicial do `Nuuvify.CommonPack.AzureServiceBus`.
- Interface `IServiceBusMessageSender` com operações para filas e tópicos: envio simples, envio em lote, agendamento e cancelamento de mensagens agendadas.
- Classe de configuração `ServiceBusConfiguration` via `IOptions` pattern.
- Classe de opções `ServiceBusMessageOptions` para personalização de mensagens (id, correlação, sessão, TTL, agendamento).
- Extensões de DI: `AddAzureServiceBus` com sobrecarga por `IConfiguration`, `Action<ServiceBusConfiguration>` e factory com `IServiceProvider`.
- Suporte a `CancellationToken` em todas as operações.
- Implementação de `IDisposable` e `IAsyncDisposable`.

