# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2025-10-13

### Alterado
- 🔧 **Código**: Removido `#nullable enable` de todos os arquivos fonte
- 📝 **Documentação**: Atualização completa da documentação (README.md)
- 🏷️ **Pacote**: Adicionadas tags de PackageTags para melhor descoberta no NuGet
- ✨ **Pacote**: Descrição e Summary do pacote atualizadas com informações detalhadas
- 🧹 **Código**: Centralizados using statements no arquivo GlobalUsings.cs

### Adicionado
- 🆔 **Documentação**: Badges do NuGet e licença MIT no README
- 📚 **Documentação**: Seção sobre outros pacotes da CommonPack
- 🏢 **Documentação**: Informações sobre a empresa Nuuvify
- 📈 **Documentação**: Seção de versionamento seguindo Semantic Versioning
- 💬 **Documentação**: Links para GitHub Discussions e Issues atualizados
- 📄 **Código**: Arquivo GlobalUsings.cs centralizado para todas as classes
- 🔇 **Código**: Supressões SonarQube para regra S2325 (métodos privados não utilizados)

### Corrigido
- ❌ **Dependências**: Removidas dependências redundantes que já são herdadas via projeto de abstração:
  - `Azure.Messaging.ServiceBus` (herdado)
  - `Microsoft.Extensions.Options` (herdado)
  - `Microsoft.Extensions.Logging.Abstractions` (herdado)
  - `Microsoft.Extensions.Configuration.Abstractions` (herdado)
  - `Microsoft.Extensions.DependencyInjection.Abstractions` (herdado)
- ✅ **Dependências**: Mantidas apenas dependências específicas necessárias:
  - `Microsoft.Extensions.Logging` (implementação concreta)
  - `Microsoft.Extensions.Options.ConfigurationExtensions` (extensões de configuração)
- 🏷️ **Nullable**: Removida configuração `#nullable enable` individualizada dos arquivos
- 🧹 **Using**: Removidos using statements duplicados dos arquivos individuais
- 🔇 **Qualidade**: Aplicadas supressões SonarQube apropriadas para falsos positivos

## [2.0.0] - 2025-10-08

### Adicionado
- 🎉 **Release inicial do Nuuvify.CommonPack.AzureServiceBus**
- ✨ Implementação completa thread-safe para Azure Service Bus
- 🚀 Suporte completo a operações com filas (queues)
- 🚀 Suporte completo a operações com tópicos (topics)
- 📦 Operações em lote para alta performance
- ⏰ Suporte a mensagens agendadas
- ❌ Cancelamento de mensagens agendadas
- 🔄 Sistema de retry automático com backoff exponencial
- 📝 Logging integrado com Microsoft.Extensions.Logging
- ⚙️ Configuração flexível via IOptions pattern
- 🧩 Integração com Dependency Injection (.NET)
- ✅ Validação robusta de parâmetros e configurações
- 🧽 Implementação correta de Dispose Pattern (IDisposable e IAsyncDisposable)
- 🔒 Thread-safety completa para uso como Singleton
- 📋 Documentação XML completa em todos os métodos públicos

### Funcionalidades Principais

#### Interface IServiceBusMessageSender
- `SendMessageToQueueAsync<T>()` - Envio simples para fila
- `SendMessageToTopicAsync<T>()` - Envio simples para tópico
- `SendBatchMessagesToQueueAsync<T>()` - Envio em lote para fila
- `SendBatchMessagesToTopicAsync<T>()` - Envio em lote para tópico
- `ScheduleMessageToQueueAsync<T>()` - Agendamento para fila
- `ScheduleMessageToTopicAsync<T>()` - Agendamento para tópico
- `CancelScheduledMessageInQueueAsync()` - Cancelar agendamento em fila
- `CancelScheduledMessageInTopicAsync()` - Cancelar agendamento em tópico

#### Configuração ServiceBusConfiguration
- `ConnectionString` - String de conexão do Azure Service Bus
- `DefaultQueueName` - Fila padrão para operações
- `DefaultTopicName` - Tópico padrão para operações
- `OperationTimeoutSeconds` - Timeout para operações (padrão: 30s)
- `MaxRetryAttempts` - Máximo de tentativas de retry (padrão: 3)
- `RetryDelaySeconds` - Delay base entre retries (padrão: 5s)
- `MaxBatchSize` - Tamanho máximo de batch (padrão: 100)
- `EnableSessions` - Habilitar sessões para ordem (padrão: false)
- `EnablePartitioning` - Habilitar particionamento (padrão: false)
- `DefaultMessageTtlMinutes` - TTL padrão de mensagens (padrão: 60min)

#### Opções de Mensagem ServiceBusMessageOptions
- `MessageId` - ID único da mensagem
- `CorrelationId` - ID de correlação para rastreamento
- `SessionId` - ID da sessão para mensagens ordenadas
- `TimeToLive` - Tempo de vida da mensagem
- `Subject` - Assunto/rótulo da mensagem
- `ContentType` - Tipo de conteúdo (application/json, etc.)
- `ReplyTo` - Destino para resposta
- `ReplyToSessionId` - Sessão para resposta
- `PartitionKey` - Chave de particionamento
- `ViaPartitionKey` - Chave de partição via
- `ScheduledEnqueueTime` - Tempo agendado para entrega

#### Extensões de DI ServiceCollectionExtensions
- `AddAzureServiceBus(IConfiguration)` - Registro básico via configuração
- `AddAzureServiceBus(Action<ServiceBusConfiguration>)` - Configuração programática
- `AddAzureServiceBus(Func<IServiceProvider, ServiceBusConfiguration>)` - Factory com DI

### Arquitetura Técnica

#### Implementação Modular (Partial Classes)
- **ServiceBusMessageSender.cs** - Classe principal, construtor e dispose
- **ServiceBusMessageSender.Validations.cs** - Validações de parâmetros
- **ServiceBusMessageSender.MessageCreation.cs** - Criação e serialização
- **ServiceBusMessageSender.BatchOperations.cs** - Operações em lote
- **ServiceBusMessageSender.RetryHandling.cs** - Lógica de retry
- **ServiceBusMessageSender.Queue.cs** - Operações específicas de filas
- **ServiceBusMessageSender.Topic.cs** - Operações específicas de tópicos

#### Características de Performance
- ✅ **Singleton-Safe**: Thread-safe para uso como singleton em APIs REST
- ✅ **Pool de Conexões**: Reutilização eficiente do ServiceBusClient
- ✅ **Operações Paralelas**: Suporte a envio simultâneo para múltiplas filas
- ✅ **Batch Operations**: Envio em lote para alta throughput
- ✅ **Resource Management**: Dispose automático de recursos nativos

#### Reliability Features
- 🔄 **Exponential Backoff**: Retry inteligente com crescimento exponencial
- ⏱️ **Configurable Timeouts**: Timeouts configuráveis por operação
- 🛡️ **Exception Handling**: Tratamento específico de exceções do Service Bus
- 📊 **Observability**: Logging detalhado para troubleshooting
- ✋ **Cancellation Support**: Suporte completo a CancellationToken

### Dependências Iniciais (v1.0.0)
- **Microsoft.Extensions.Logging** v8.0.1 - Implementação concreta de logging
- **Microsoft.Extensions.Options.ConfigurationExtensions** v8.0.0 - Extensões de configuração
- **Nuuvify.CommonPack.AzureServiceBus.Abstraction** - Interfaces e abstrações

#### Dependências Herdadas (via Abstraction)
- **Azure.Messaging.ServiceBus** v7.20.1 - Cliente oficial Azure Service Bus
- **Microsoft.Extensions.Logging.Abstractions** v8.0.3 - Abstrações de logging
- **Microsoft.Extensions.Configuration.Abstractions** v8.0.0 - Abstrações de configuração
- **Microsoft.Extensions.Options** v8.0.2 - Sistema de opções do .NET
- **Microsoft.Extensions.DependencyInjection.Abstractions** v8.0.2 - Abstrações de DI

### Target Framework
- **.NET 8.0** com `LangVersion: latest`
- **Nullable Reference Types** habilitado
- **Implicit Usings** habilitado
- **XML Documentation** gerada automaticamente

---

## Tipos de Mudanças

- `Adicionado` para novas funcionalidades
- `Alterado` para mudanças em funcionalidades existentes
- `Descontinuado` para funcionalidades que serão removidas em breve
- `Removido` para funcionalidades removidas
- `Corrigido` para correções de bugs
- `Segurança` para correções de vulnerabilidades

## Convenções de Versionamento

Este projeto segue [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0): Mudanças incompatíveis na API
- **MINOR** (0.X.0): Novas funcionalidades mantendo compatibilidade
- **PATCH** (0.0.X): Correções de bugs mantendo compatibilidade

### Breaking Changes Policy

- **v1.x**: Mudanças breaking serão evitadas ao máximo
- **v2.x**: Mudanças breaking serão bem documentadas com migration guide
- **Deprecation**: Funcionalidades serão marcadas como obsoletas antes da remoção

### Release Cycle

- **Patch releases**: Conforme necessário para correções críticas
- **Minor releases**: Mensalmente com novas funcionalidades
- **Major releases**: Anualmente ou quando necessário para mudanças arquiteturais

---

## Links Úteis

- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [.NET 8.0 Release Notes](https://docs.microsoft.com/dotnet/core/release-notes/8.0/)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [Nuuvify CommonPack Repository](https://github.com/nuuvify/Nuuvify.CommonPack)
- [NuGet Package](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus/)

---

**Nota**: Este changelog é mantido manualmente seguindo as convenções do [Keep a Changelog](https://keepachangelog.com/).
Para ver todas as mudanças detalhadas, consulte o [histórico de commits](https://github.com/nuuvify/Nuuvify.CommonPack/commits/backgroundservice) do repositório.
