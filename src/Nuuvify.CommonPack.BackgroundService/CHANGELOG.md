# Changelog - Nuuvify.CommonPack.BackgroundService

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Não Lançado]

### Adicionado
- Suporte a `ReceiveMode` no `ServiceBusBackgroundService`, com identificação explícita do modo `ReceiveAndDelete` para classes derivadas.

### Alterado
- Fluxo de sucesso ajustado para não executar `CompleteMessageAsync` quando o processor estiver em `ReceiveAndDelete`.
- Tratamento de exceções e retorno de regra de negócio ajustado para não tentar `DeadLetter`/`Abandon` em modos sem lock da mensagem.

### Corrigido
- Corrigido o fluxo de tratamento para mensagens já removidas em `ReceiveAndDelete`, evitando exceções secundárias durante o error handling.

### Removido

### Segurança

## [Sem versão registrada] - 2025-10-07

### Adicionado
- Propriedades de diagnóstico avançado para Dead Letter Queue e Abandon Message com informações de versão, instância, correlação, timestamp e tentativa de entrega.
- Método `CreateDeadLetterProperties` para criação de metadados contextuais de dead letter.
- Método `CreateAbandonProperties` para criação de propriedades de diagnóstico de abandon.
- Interface `IBackgroundServiceAbstract<T>` definindo o contrato da classe base.

### Alterado
- `HandleMessageAsync` refatorado para reduzir complexidade cognitiva (16 → <15) com métodos especializados:
  - `HandleServiceBusSpecificExceptionAsync`
  - `HandleServiceBusCommunicationExceptionAsync`
  - `HandleOperationCanceledExceptionAsync`
  - `HandleGenericExceptionAsync`
  - `HandleBusinessLogicFailureAsync`
- Todas as chamadas de `DeadLetter` e `Abandon` passam `propertiesToModify` com as propriedades de diagnóstico.
- Método `ExecuteRule` alterado de `virtual` para `abstract`.
- Parâmetro `cancellationToken` renomeado para `stoppingToken` em `ExecuteAsync`.

### Corrigido
- Propagação correta do `CancellationToken` em todas as operações assíncronas.

### Segurança
- Informações sensíveis não expostas nas propriedades de diagnóstico.

## [Sem versão registrada] - 2025-01-01

### Adicionado
- Versão inicial da classe `BackgroundServiceAbstract<T>`.
- Integração com Azure Service Bus via `ServiceBusProcessor`.
- Tratamento de erros com estratégias de abandon e dead letter.
- Suporte a configuração via connection string ou Azure credentials.

