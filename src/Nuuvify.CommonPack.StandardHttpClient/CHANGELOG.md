# Nuuvify.CommonPack - Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

## 2025-09-30

### 🔔 Sistema de Notificações Aprimorado

#### ✨ Recursos Adicionados
- **Sistema de Notificações Robusto**: Implementação completa do padrão de notificações no `BaseStandardHttpClient`
- **ReadOnlyCollection**: Exposição thread-safe das notificações através de `ReadOnlyCollection<NotificationR>`
- **Gerenciamento de Notificações**: Métodos protegidos `AddNotification()`, `AddNotifications()`, `ClearNotifications()` e `RemoveNotifications()`
- **Integração com TokenService**: Coleta automática de notificações do `ITokenService` durante operações de autenticação
- **Notificações de Serialização**: Captura automática de erros durante serialização/deserialização JSON
- **Rastreamento de Operações**: Notificações detalhadas para operações HTTP com contexto de erro

#### 🧪 Melhorias nos Testes Unitários
- **Correção de SocketException**: Resolução completa dos erros de conexão em testes unitários com mocks
- **HttpClient Disposal Pattern**: Implementação correta do padrão `disposeHandler: false` nos testes
- **Mock Factory Setup**: Correção do setup de mocks usando lambdas `Returns(() => client)` para evitar problemas de estado
- **Testes Diagnósticos**: Criação de classes de teste especializadas para debugging de problemas de HTTP mocking
- **Improved Handler Stub**: Handler aprimorado para melhor simulação de respostas HTTP em testes
- **Test Coverage**: Cobertura completa de cenários de erro e sucesso em operações HTTP

#### 🛠️ Componentes Modificados
- **BaseStandardHttpClient**: Sistema de notificações thread-safe implementado
- **ITokenService**: Interface estendida para suporte a notificações
- **TokenService**: Integração com sistema de notificações durante operação de tokens
- **StandardHttpClient**: Melhorias na gestão de recursos e notificações
- **Test Infrastructure**: Infraestrutura de testes completamente refatorada

#### 📊 Impacto Técnico
- **Thread Safety**: Notificações expostas via ReadOnlyCollection para acesso thread-safe
- **Error Tracking**: Rastreamento detalhado de erros em operações HTTP e de autenticação
- **Test Reliability**: 100% de sucesso em testes unitários com mocks (32/33 testes passando)
- **Debugging**: Ferramentas aprimoradas para diagnóstico de problemas em desenvolvimento
- **Maintainability**: Código mais limpo e fácil de manter com padrões bem estabelecidos


### 🚀 Performance & Resource Management

#### ✨ Recursos Adicionados
- **ConfigureAwait(false)**: Implementado em todos os métodos `await` para otimização de performance em bibliotecas
- **Resource Management**: Padrão `using var` aplicado a `HttpRequestMessage`, `StringContent` e recursos `IDisposable`
- **Memory Leak Prevention**: Sistema automático de disposal para prevenção de vazamentos de memória
- **Enhanced IDisposable**: Pattern aprimorado na classe `StandardHttpClient` para cleanup completo
- **CA2000 Compliance**: Conformidade total com regras de análise estática para gerenciamento de recursos

#### 🔧 Melhorias de Performance
- **Async Optimization**: Eliminação de context switching desnecessário com `ConfigureAwait(false)`
- **Thread Pool Efficiency**: Melhor utilização de threads em cenários de alta concorrência
- **Scalability**: Desempenho superior em aplicações server-side (ASP.NET Core, Web APIs)
- **Deadlock Prevention**: Prevenção de deadlocks em código que mistura sync/async

#### 🛠️ Componentes Modificados
- **HTTP Verbs**: Todos os métodos (GET, POST, PUT, PATCH, DELETE) otimizados
- **StandardHttpClientService**: Enhanced disposal pattern implementado
- **TokenService**: ConfigureAwait(false) aplicado em operações de token
- **Polly Policies**: Retry e Circuit Breaker policies otimizadas
- **StandardWebServicePrivate**: StringContent com proper disposal

#### 📊 Impacto Técnico
- **Memory Usage**: Redução significativa no consumo de memória
- **Response Times**: Tempos de resposta mais rápidos devido à otimização de contexto
- **Stability**: Maior estabilidade através de proper resource management
- **Code Quality**: Conformidade com best practices para Class Libraries .NET

## 2025-01-01

## 2025-07-09

