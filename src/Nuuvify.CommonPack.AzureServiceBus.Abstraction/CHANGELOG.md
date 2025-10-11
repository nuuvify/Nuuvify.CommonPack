# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Alterado
- 📝 Documentação completa atualizada (README.md)
- 🏷️ Tags de PackageTags otimizadas para melhor descoberta no NuGet
- ✨ Descrição e Summary do pacote atualizadas com foco em abstrações
- 📊 Seção de dependências expandida com explicações detalhadas

### Adicionado
- 🆔 Badges do NuGet Version e Licença MIT no README
- 📚 Seção sobre outros pacotes da CommonPack
- 🏢 Informações sobre a empresa Nuuvify
- 📈 Seção de versionamento seguindo Semantic Versioning
- 💬 Links para GitHub Discussions e Issues atualizados
- 🎯 Seção explicando o objetivo das abstrações
- 🧪 Exemplos detalhados de testabilidade com Mock e Moq
- 🏗️ Diagrama de arquitetura de dependências

## [1.0.0] - 2025-10-08

### Adicionado
- 🎉 **Release inicial do Nuuvify.CommonPack.AzureServiceBus.Abstraction**
- 🔌 Interface principal `IServiceBusMessageSender` com todas as operações
- ⚙️ Classe de configuração `ServiceBusConfiguration` com validações
- 📝 Classe de opções `ServiceBusMessageOptions` para personalização de mensagens
- 📋 Documentação XML completa em todas as interfaces públicas
- ✅ Validações robustas de configuração com métodos `IsValid()` e `ValidateConfiguration()`

### Funcionalidades Principais

#### Interface IServiceBusMessageSender
Contrato completo para operações com Azure Service Bus:

- **Operações Simples**:
  - `SendMessageToQueueAsync<T>()` - Envio para fila
  - `SendMessageToTopicAsync<T>()` - Envio para tópico

- **Operações em Lote**:
  - `SendBatchMessagesToQueueAsync<T>()` - Lote para fila
  - `SendBatchMessagesToTopicAsync<T>()` - Lote para tópico

- **Mensagens Agendadas**:
  - `ScheduleMessageToQueueAsync<T>()` - Agendamento para fila
  - `ScheduleMessageToTopicAsync<T>()` - Agendamento para tópico

- **Cancelamento de Agendamento**:
  - `CancelScheduledMessageInQueueAsync()` - Cancelar em fila
  - `CancelScheduledMessageInTopicAsync()` - Cancelar em tópico

- **Recursos Avançados**:
  - Suporte completo a `CancellationToken`
  - Implementação de `IAsyncDisposable`
  - Opções personalizáveis via `ServiceBusMessageOptions`

#### ServiceBusConfiguration
Classe de configuração tipada com propriedades:

- **Conectividade**:
  - `ConnectionString` - String de conexão do Azure Service Bus
  - `DefaultQueueName` - Fila padrão para operações
  - `DefaultTopicName` - Tópico padrão para operações

- **Timeouts e Retry**:
  - `OperationTimeoutSeconds` - Timeout para operações (padrão: 30s)
  - `MaxRetryAttempts` - Máximo de tentativas (padrão: 3)
  - `RetryDelaySeconds` - Delay entre retries (padrão: 5s)

- **Performance**:
  - `MaxBatchSize` - Tamanho máximo de batch (padrão: 100)
  - `EnableSessions` - Habilitar sessões para ordem (padrão: false)
  - `EnablePartitioning` - Particionamento (padrão: false)

- **Mensagens**:
  - `DefaultMessageTtlMinutes` - TTL padrão (padrão: 60min)

- **Validação**:
  - `IsValid()` - Validação rápida da configuração
  - `ValidateConfiguration()` - Validação detalhada com lista de erros

#### ServiceBusMessageOptions
Opções avançadas para personalização de mensagens:

- **Identificação**:
  - `MessageId` - ID único da mensagem
  - `CorrelationId` - ID de correlação para rastreamento
  - `SessionId` - ID da sessão para mensagens ordenadas

- **Controle de Vida**:
  - `TimeToLive` - Tempo de vida da mensagem
  - `ScheduledEnqueueTime` - Agendamento para entrega futura

- **Metadados**:
  - `Subject` - Assunto/rótulo da mensagem
  - `ContentType` - Tipo de conteúdo (application/json, etc.)
  - `ApplicationProperties` - Propriedades customizadas

- **Roteamento**:
  - `ReplyTo` - Destino para resposta
  - `ReplyToSessionId` - Sessão para resposta
  - `PartitionKey` - Chave de particionamento
  - `ViaPartitionKey` - Chave de partição via

### Arquitetura e Design

#### Princípios SOLID
- **Single Responsibility**: Cada classe tem responsabilidade única e bem definida
- **Open/Closed**: Interfaces abertas para extensão, fechadas para modificação
- **Liskov Substitution**: Implementações podem ser substituídas sem afetar o código cliente
- **Interface Segregation**: Interface focada apenas em operações de messaging
- **Dependency Inversion**: Aplicação depende de abstrações, não de implementações

#### Benefícios das Abstrações
1. **Testabilidade**: Facilita criação de mocks e testes unitários
2. **Flexibilidade**: Permite múltiplas implementações
3. **Desacoplamento**: Reduz dependências entre camadas
4. **Manutenibilidade**: Mudanças de implementação não afetam consumidores

#### Compatibilidade
- **.NET Standard 2.1**: Máxima compatibilidade com diferentes frameworks
- **Async/Await**: Suporte completo a programação assíncrona
- **Nullable Reference Types**: Segurança de tipos melhorada
- **Generic Support**: Métodos genéricos para qualquer tipo de mensagem

### Dependências
- **Azure.Messaging.ServiceBus** - SDK oficial do Azure Service Bus
- **Microsoft.Extensions.Logging.Abstractions** - Abstrações de logging
- **Microsoft.Extensions.Configuration.Abstractions** - Abstrações de configuração
- **Microsoft.Extensions.Options** - Sistema de opções do .NET
- **Microsoft.Extensions.DependencyInjection.Abstractions** - Abstrações de DI

### Target Framework
- **.NET Standard 2.1** - Compatibilidade máxima com ecossistema .NET
- **Nullable Reference Types** habilitado
- **LangVersion: latest** para recursos modernos do C#

### Casos de Uso Suportados

#### Para Desenvolvedores de Aplicação
- Injeção de dependência via `IServiceBusMessageSender`
- Configuração via `ServiceBusConfiguration`
- Testes unitários com mocks da interface

#### Para Desenvolvedores de Biblioteca
- Implementação da interface `IServiceBusMessageSender`
- Extensão de funcionalidades via herança
- Integração com containers de DI

#### Ambientes Suportados
- **APIs REST**: Controllers que enviam mensagens
- **Worker Services**: Processamento em background
- **Console Applications**: Aplicações de linha de comando
- **Blazor Applications**: Aplicações web interativas
- **WinForms/WPF**: Aplicações desktop

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

### Breaking Changes Policy para Abstrações

#### Mudanças que QUEBRAM compatibilidade:
- Remoção de métodos da interface `IServiceBusMessageSender`
- Mudança de assinatura de métodos existentes
- Remoção de propriedades de `ServiceBusConfiguration` ou `ServiceBusMessageOptions`
- Alteração de tipos de retorno ou parâmetros

#### Mudanças que MANTÊM compatibilidade:
- Adição de novos métodos com implementação padrão (`default interface methods`)
- Adição de novas propriedades com valores padrão
- Adição de novos parâmetros opcionais
- Melhorias na documentação XML

#### Estratégia de Migração
- **v1.x**: Estabilidade máxima - mudanças breaking serão evitadas
- **v2.x**: Mudanças breaking serão bem documentadas com guia de migração
- **Deprecation**: Funcionalidades serão marcadas como `[Obsolete]` por pelo menos uma versão minor antes da remoção

### Release Cycle

- **Patch releases**: Conforme necessário para correções críticas
- **Minor releases**: Mensalmente com novas funcionalidades
- **Major releases**: Anualmente ou quando necessário para evolução da arquitetura

---

## Links Úteis

- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [.NET Standard 2.1 API](https://docs.microsoft.com/dotnet/standard/net-standard)
- [Dependency Injection in .NET](https://docs.microsoft.com/dotnet/core/extensions/dependency-injection)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [Nuuvify CommonPack Repository](https://github.com/nuuvify/Nuuvify.CommonPack)
- [NuGet Package](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus.Abstraction/)

---

**Nota**: Este changelog é mantido manualmente seguindo as convenções do [Keep a Changelog](https://keepachangelog.com/).
Para ver todas as mudanças detalhadas, consulte o [histórico de commits](https://github.com/nuuvify/Nuuvify.CommonPack/commits/backgroundservice) do repositório.
