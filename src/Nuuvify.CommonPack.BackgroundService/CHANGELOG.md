# Changelog - Nuuvify.CommonPack.BackgroundService

Todas as mudanças notáveis deste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added
- **🆕 Propriedades de diagnóstico avançado** para Dead Letter Queue e Abandon Message
- **🆕 Métodos especializados de tratamento de exceções** com separação de responsabilidades
- **🆕 Classe parcial `ServiceBusBackgroundService.ExceptionHandling.cs`** para organização modular
- **🆕 Método `CreateDeadLetterProperties`** para criação de metadados contextuais
- **🆕 Método `CreateAbandonProperties`** para criação de propriedades de diagnóstico
- **🆕 Rastreamento detalhado** com informações de versão, instância e correlação
- Interface `IBackgroundServiceAbstract<T>` para definir o contrato da classe base
- Documentação XML completa para todos os métodos e propriedades
- Tratamento robusto de erros com diferentes estratégias (abandon vs dead letter)
- Validação do resultado do `ExecuteRule` para determinar o destino da mensagem
- Telemetria integrada com OpenTelemetry
- Logs estruturados com diferentes níveis de severidade
- Exemplo de implementação com `OrderProcessingBackgroundService`
- Testes unitários abrangentes
- Suporte para configuração via connection string ou Azure credentials
- Documentação completa no README.md

### Changed
- **🔧 Refatoração do método `HandleMessageAsync`** para reduzir complexidade cognitiva (16 → <15)
- **🔧 Arquitetura modular** com separação em arquivos especializados
- **🔧 Métodos de tratamento de exceções especializados**:
  - `HandleServiceBusSpecificExceptionAsync`
  - `HandleServiceBusCommunicationExceptionAsync`
  - `HandleOperationCanceledExceptionAsync`
  - `HandleGenericExceptionAsync`
  - `HandleBusinessLogicFailureAsync`
- **🔧 Uso do parâmetro `propertiesToModify`** em todas as chamadas de DeadLetter e Abandon
- Método `ExecuteRule` alterado de virtual para abstract
- Melhorado o tratamento de cancelamento de operações
- Aprimorado o método `Dispose` para seguir as melhores práticas
- Parâmetro `cancellationToken` renomeado para `stoppingToken` no `ExecuteAsync`
- Melhorada a configuração do Service Bus com validações mais robustas

### Fixed
- **✅ Complexidade cognitiva** do método `HandleMessageAsync` reduzida
- **✅ Maintainability** melhorada com métodos menores e mais focados
- **✅ Code quality** aprimorada seguindo princípios SOLID
- **✅ Suppressão CA1031** com justificativa técnica adequada
- Propagação correta do `CancellationToken` em todas as operações assíncronas
- Tratamento adequado de recursos disposable
- Correção de warnings do analisador de código
- Implementação correta do padrão Dispose

### Security
- **🔒 Informações sensíveis** protegidas nas propriedades de diagnóstico
- **🔒 Validação robusta** de parâmetros de configuração
- Validação de parâmetros de configuração para evitar falhas de segurança
- Tratamento seguro de credenciais Azure

### Technical Debt
- **📉 Complexidade cognitiva reduzida** de 16 para <15 no método principal
- **📉 Código duplicado eliminado** com métodos especializados
- **📈 Testabilidade aumentada** com métodos menores
- **📈 Reutilização melhorada** com métodos virtuais sobrescrevíveis

### Observability & Debugging
- **🔍 Troubleshooting aprimorado** com propriedades contextuais detalhadas
- **📊 Métricas customizáveis** baseadas em propriedades de diagnóstico
- **🎯 Root cause analysis** facilitado com informações de contexto
- **📈 Monitoramento avançado** com timestamps, versões e instâncias
- **🔗 Rastreamento de correlação** end-to-end melhorado
- **📝 Logs estruturados** com informações de diagnóstico integradas

### Example Properties Added:
```json
// Dead Letter Properties
{
  "ErrorDetails": "Business logic returned false",
  "FailureTime": "2025-10-07T19:26:30.123Z",
  "WorkerVersion": "1.2.3.0",
  "CorrelationId": "abc-123-def",
  "DeliveryAttempt": 3,
  "ExceptionType": "BusinessLogicFailure",
  "WorkerInstance": "SERVER01",
  "ProcessedBy": "OrderProcessingService"
}

// Abandon Properties
{
  "AbandonReason": "Operation was cancelled",
  "AbandonTime": "2025-10-07T19:26:30.123Z",
  "RetryCount": 2,
  "NextRetryHint": "2025-10-07T19:27:30.123Z",
  "WorkerInstance": "SERVER01",
  "ProcessedBy": "OrderProcessingService"
}
```

## [1.0.0] - 2025-01-XX

### Added
- Versão inicial da classe `BackgroundServiceAbstract<T>`
- Integração básica com Azure Service Bus
- Processamento de mensagens com Service Bus Processor

---

Para mais informações, visite: [https://github.com/nuuvify/Nuuvify.CommonPack](https://github.com/nuuvify/Nuuvify.CommonPack)
