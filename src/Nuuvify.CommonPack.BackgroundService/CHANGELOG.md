# Changelog - Nuuvify.CommonPack.BackgroundService

Todas as mudanças notáveis deste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added
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
- Método `ExecuteRule` alterado de virtual para abstract
- Melhorado o tratamento de cancelamento de operações
- Aprimorado o método `Dispose` para seguir as melhores práticas
- Parâmetro `cancellationToken` renomeado para `stoppingToken` no `ExecuteAsync`
- Melhorada a configuração do Service Bus com validações mais robustas

### Fixed
- Propagação correta do `CancellationToken` em todas as operações assíncronas
- Tratamento adequado de recursos disposable
- Correção de warnings do analisador de código
- Implementação correta do padrão Dispose

### Security
- Validação de parâmetros de configuração para evitar falhas de segurança
- Tratamento seguro de credenciais Azure

## [1.0.0] - 2025-01-XX

### Added
- Versão inicial da classe `BackgroundServiceAbstract<T>`
- Integração básica com Azure Service Bus
- Processamento de mensagens com Service Bus Processor

---

Para mais informações, visite: [https://github.com/nuuvify/Nuuvify.CommonPack](https://github.com/nuuvify/Nuuvify.CommonPack)
