# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### 🆕 Adicionado
- **Filtros com tipos complexos** - Suporte completo a propriedades de navegação aninhadas
- **Notação de ponto (dot notation)** para filtros em relacionamentos (ex: `Customer.Address.City`)
- **Validação automática** de propriedades aninhadas no ExpressionFactory
- **Parameterização otimizada** para EF Core com `GetClosureOverConstant`
- **Documentação avançada** com exemplos de filtros complexos

### 🔧 Alterado
- **ExpressionFactory** refatorado para suportar propriedades aninhadas de múltiplos níveis
- **FiltersExtensions** aprimorado com melhor tratamento de case-sensitivity
- **Testes** reestruturados com gerenciamento adequado de recursos EF Core

### 🐛 Corrigido
- **ObjectDisposedException** em testes com contextos EF Core
- **Gerenciamento de ciclo de vida** de DbContext e UnitOfWork nos testes
- **Memory leaks** em fixtures de teste

## [3.0.0] - 2025-11-01

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
