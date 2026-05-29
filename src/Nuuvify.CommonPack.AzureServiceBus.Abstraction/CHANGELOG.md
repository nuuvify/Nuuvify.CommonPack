# Changelog - Nuuvify.CommonPack.AzureServiceBus.Abstraction

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
- Tags de `PackageTags` no `.csproj` atualizadas para melhor descoberta no NuGet.

## [Sem versão registrada] - 2025-10-08

### Adicionado
- Release inicial do `Nuuvify.CommonPack.AzureServiceBus.Abstraction`.
- Interface `IServiceBusMessageSender` com operações para filas e tópicos: envio simples, envio em lote, agendamento e cancelamento de mensagens agendadas.
- Classe de configuração `ServiceBusConfiguration` com métodos `IsValid()` e `ValidateConfiguration()`.
- Classe de opções `ServiceBusMessageOptions` para personalização de mensagens (id, correlação, sessão, TTL, agendamento, propriedades customizadas).
- Suporte a `CancellationToken` em todas as operações.
- Interface estende `IAsyncDisposable`.


